namespace ParserCSV.Model;

public class TripRecordDbRow
{
    public DateTime TpepPickupDatetime { get; set; } // UTC
    public DateTime TpepDropoffDatetime { get; set; } // UTC
    public int? PassengerCount { get; set; }
    public decimal TripDistance { get; set; }
    public string StoreAndFwdFlag { get; set; } // 'Yes'/'No'
    public int PULocationID { get; set; }
    public int DOLocationID { get; set; }
    public decimal FareAmount { get; set; }
    public decimal TipAmount { get; set; }
}
