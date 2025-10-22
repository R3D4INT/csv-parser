using CsvHelper;
using ParserCSV.Extensions;
using ParserCSV.Model;
using System.Globalization;

namespace ParserCSV.Services;

public class EtlOrchestrator
{
    private readonly ITripRepository _repository;

    public EtlOrchestrator(ITripRepository repository)
    {
        _repository = repository;
    }

    public async Task RunAsync(string inputCsvPath, string duplicatesCsvPath, int batchSize = 50000)
    {
        Console.WriteLine("ETL process started...");

        var seenRecords = new HashSet<(DateTime, DateTime, int?)>();
        var batch = new List<TripRecordDbRow>(batchSize);

        long recordsProcessed = 0;
        long duplicatesFound = 0;
        long recordsInserted = 0;

        try
        {
            using var reader = new StreamReader(inputCsvPath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            using var duplicateWriter = new StreamWriter(duplicatesCsvPath);
            using var csvDuplicates = new CsvWriter(duplicateWriter, CultureInfo.InvariantCulture);

            csvDuplicates.Context.RegisterClassMap<TaxiTripCsvRowMap>();
            csvDuplicates.WriteHeader<TripRecordCsvRow>();
            csvDuplicates.NextRecord();

            csv.Read();
            csv.ReadHeader();

            while (csv.Read())
            {
                var rawRecord = csv.GetRecord<TripRecordCsvRow>(); 
                recordsProcessed++;

                var duplicateKey = (rawRecord.TpepPickupDatetime, rawRecord.TpepDropoffDatetime, rawRecord.PassengerCount);

                if (!seenRecords.Add(duplicateKey))
                {
                    csvDuplicates.WriteRecord(rawRecord);
                    csvDuplicates.NextRecord();
                    duplicatesFound++;
                    continue;
                }

                var dbRecord = rawRecord.ToDbRow();
                batch.Add(dbRecord);

                if (batch.Count >= batchSize)
                {
                    await _repository.BulkInsertBatchAsync(batch);
                    recordsInserted += batch.Count;
                    Console.WriteLine($"Inserted {batch.Count} records. Total processed: {recordsProcessed}");
                    batch.Clear();
                }

                if (recordsProcessed % 100000 == 0)
                {
                    Console.WriteLine($"...Processed {recordsProcessed} rows...");
                }
            }

            if (batch.Count > 0)
            {
                await _repository.BulkInsertBatchAsync(batch);
                recordsInserted += batch.Count;
                Console.WriteLine($"Inserted {batch.Count} records. Total processed: {recordsProcessed}"); 
                batch.Clear();
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Critical error during ETL: {ex.Message}"); 
            Console.WriteLine("Process stopped."); 
            Console.ResetColor();
            throw; 
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n--- ETL process finished ---");
        Console.ResetColor();
        Console.WriteLine($"Total rows read: {recordsProcessed}"); 
        Console.WriteLine($"Duplicates found (saved to {duplicatesCsvPath}): {duplicatesFound}"); 
        Console.WriteLine($"Unique records inserted into DB: {recordsInserted}"); 
    }

    private sealed class TaxiTripCsvRowMap : CsvHelper.Configuration.ClassMap<TripRecordCsvRow>
    {
        public TaxiTripCsvRowMap()
        {
            Map(m => m.TpepPickupDatetime).Name("tpep_pickup_datetime");
            Map(m => m.TpepDropoffDatetime).Name("tpep_dropoff_datetime");
            Map(m => m.PassengerCount).Name("passenger_count");
            Map(m => m.TripDistance).Name("trip_distance");
            Map(m => m.StoreAndFwdFlag).Name("store_and_fwd_flag");
            Map(m => m.PULocationID).Name("PULocationID");
            Map(m => m.DOLocationID).Name("DOLocationID");
            Map(m => m.FareAmount).Name("fare_amount");
            Map(m => m.TipAmount).Name("tip_amount");
        }
    }
}
