using Microsoft.AspNetCore.Identity;

namespace Simbir.GO.Model;

public class Account : IdentityUser<int>
{
    public DateTimeOffset CreateDate { get; set; }
    public double Balance { get; set; }
    public bool IsAdmin { get; set; }
}