namespace Candoumbe.DataAccess.EFStore.UnitTests;

using Abstractions;
using Entities;
using Microsoft.EntityFrameworkCore;

public class SqliteDbContext : DbContext, IDbContext
{
    public DbSet<Hero> Heroes { get; set; }
    public DbSet<Acolyte> Acolytes { get; set; }

    public DbSet<Weapon> Weapons { get; set; }

    public SqliteDbContext(DbContextOptions<SqliteDbContext> options) : base(options) { }

    ///<inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new HeroEntityTypeConfiguration())
            .ApplyConfiguration(new AcolyteEntityTypeConfiguration())
            .ApplyConfiguration(new WeaponEntityTypeConfiguration());
    }
}