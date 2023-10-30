namespace Simbir.GO.Contract.Requests;

public class AdminRentRequest
{
    public int TransportId { get; set; }
    public int UserId { get; set; }
    public DateTimeOffset TimeStart { get; set; }
    public DateTimeOffset? TimeEnd { get; set; }
    public double PriceOfUnit { get; set; }
    public string PriceType { get; set; }
    public double? FinalPrice { get; set; }
}