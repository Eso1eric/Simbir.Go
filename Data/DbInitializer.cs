using Simbir.GO.Model;
using Microsoft.AspNetCore.Identity;

namespace Simbir.GO.Data;

public static class DbInitializer
{
    public static void Initialize(RentContext context, UserManager<Account> userManager)
    {
        context.Database.EnsureCreated();
        
        if (context.Accounts.Any())
            return;

        var adminAccount = new Account() { UserName = "Admin", CreateDate = DateTime.UtcNow, Balance = 2000.50, IsAdmin = true, SecurityStamp = Guid.NewGuid().ToString()};
        adminAccount.NormalizedUserName = userManager.NormalizeName(adminAccount.UserName);
        adminAccount.PasswordHash = userManager.PasswordHasher.HashPassword(adminAccount, "123");
        var accounts = new Account[] { adminAccount };
        foreach (var account in accounts)
            context.Accounts.Add(account);

        context.SaveChanges();
    }
}