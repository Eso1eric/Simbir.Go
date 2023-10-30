namespace Simbir.GO.Contract.Responses;

public class AccountDataResponse
{
    public string Username { get; set; }
    public string Password { get; set; }
    public DateTimeOffset CreateDate { get; set; }
    public double Balance { get; set; }
}