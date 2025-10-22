using Microsoft.Data.SqlClient;
using ParserCSV.Model;
using System.Data;

namespace ParserCSV.Services;

public class SqlTripRepository : ITripRepository
{
    private readonly string _connectionString;

    public SqlTripRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task BulkInsertBatchAsync(IEnumerable<TripRecordDbRow> records)
    {
        var dataTable = CreateTaxiTripsDataTable();

        foreach (var record in records)
        {
            dataTable.Rows.Add(
                record.TpepPickupDatetime,
                record.TpepDropoffDatetime,
                record.PassengerCount as object ?? DBNull.Value,
                record.TripDistance,
                record.StoreAndFwdFlag,
                record.PULocationID,
                record.DOLocationID,
                record.FareAmount,
                record.TipAmount
            );
        }

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var bulkCopy = new SqlBulkCopy(connection))
            {
                bulkCopy.DestinationTableName = "TaxiTrips";
                bulkCopy.BatchSize = dataTable.Rows.Count;

                bulkCopy.ColumnMappings.Add("tpep_pickup_datetime", "tpep_pickup_datetime");
                bulkCopy.ColumnMappings.Add("tpep_dropoff_datetime", "tpep_dropoff_datetime");
                bulkCopy.ColumnMappings.Add("passenger_count", "passenger_count");
                bulkCopy.ColumnMappings.Add("trip_distance", "trip_distance");
                bulkCopy.ColumnMappings.Add("store_and_fwd_flag", "store_and_fwd_flag");
                bulkCopy.ColumnMappings.Add("PULocationID", "PULocationID");
                bulkCopy.ColumnMappings.Add("DOLocationID", "DOLocationID");
                bulkCopy.ColumnMappings.Add("fare_amount", "fare_amount");
                bulkCopy.ColumnMappings.Add("tip_amount", "tip_amount");

                await bulkCopy.WriteToServerAsync(dataTable);
            }
        }
    }

    private DataTable CreateTaxiTripsDataTable()
    {
        var dataTable = new DataTable("TaxiTrips");

        dataTable.Columns.Add("tpep_pickup_datetime", typeof(DateTime));
        dataTable.Columns.Add("tpep_dropoff_datetime", typeof(DateTime));
        dataTable.Columns.Add("passenger_count", typeof(int));
        dataTable.Columns.Add("trip_distance", typeof(decimal));
        dataTable.Columns.Add("store_and_fwd_flag", typeof(string));
        dataTable.Columns.Add("PULocationID", typeof(int));
        dataTable.Columns.Add("DOLocationID", typeof(int));
        dataTable.Columns.Add("fare_amount", typeof(decimal));
        dataTable.Columns.Add("tip_amount", typeof(decimal));
        dataTable.Columns["passenger_count"]!.AllowDBNull = true;

        return dataTable;
    }
}
