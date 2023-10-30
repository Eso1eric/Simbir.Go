namespace Simbir.GO.Model;

public class Rent
{
    public Guid Id { get; set; }
    public int TransportId { get; set; }
    public int AccountId { get; set; }
    public DateTimeOffset TimeStart { get; set; }
    public DateTimeOffset? TimeEnd { get; set; }
    public double PriceOfUnit { get; set; }
    public string PriceType { get; set; }
    public double? FinalPrice { get; set; }
}