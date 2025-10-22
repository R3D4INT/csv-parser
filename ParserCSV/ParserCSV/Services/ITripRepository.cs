using ParserCSV.Model;

namespace ParserCSV.Services;

public interface ITripRepository
{
    Task BulkInsertBatchAsync(IEnumerable<TripRecordDbRow> records);
}
