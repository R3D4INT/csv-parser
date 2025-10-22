using ParserCSV.Model;
using TimeZoneConverter;

namespace ParserCSV.Extensions;

public static class TransformationExtensions
{
    private static readonly TimeZoneInfo EstZone = TZConvert.GetTimeZoneInfo("Eastern Standard Time");

    public static TripRecordDbRow ToDbRow(this TripRecordCsvRow csvRow)
    {
        var flag = csvRow.StoreAndFwdFlag?.Trim();

        var dbRow = new TripRecordDbRow
        {
            TpepPickupDatetime = TimeZoneInfo.ConvertTimeToUtc(csvRow.TpepPickupDatetime, EstZone),
            TpepDropoffDatetime = TimeZoneInfo.ConvertTimeToUtc(csvRow.TpepDropoffDatetime, EstZone),

            StoreAndFwdFlag = flag switch
            {
                "Y" => "Yes",
                "N" => "No",
                _ => "No"
            },

            PassengerCount = csvRow.PassengerCount,
            TripDistance = csvRow.TripDistance,
            PULocationID = csvRow.PULocationID,
            DOLocationID = csvRow.DOLocationID,
            FareAmount = csvRow.FareAmount,
            TipAmount = csvRow.TipAmount
        };

        return dbRow;
    }
}
