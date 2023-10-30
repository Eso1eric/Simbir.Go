using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Simbir.GO.Model;

namespace Simbir.GO.Data;

public class RentContext : IdentityDbContext<Account, IdentityRole<int>, int>
{
    public RentContext(DbContextOptions<RentContext> options) : base(options)
    {
    }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<Transport> Transports { get; set; }
    public DbSet<Rent> Rents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Account>().ToTable("Accounts");
        modelBuilder.Entity<Transport>().ToTable("Transports");
        modelBuilder.Entity<Rent>().ToTable("Rents");
    }
}