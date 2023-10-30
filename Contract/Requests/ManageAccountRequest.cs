namespace Simbir.GO.Contract.Requests;

public class ManageAccountRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
    public bool IsAdmin { get; set; }
    public double Balance { get; set; }
}