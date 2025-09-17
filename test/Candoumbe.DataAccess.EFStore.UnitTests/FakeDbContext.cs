
using Candoumbe.DataAccess.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Candoumbe.DataAccess.EFStore.UnitTests;

public class FakeDbContext : DbContext
{
    public DbSet<Dummy> Dummies { get; set; }
}

public class Dummy
{
    public int Id { get; set; }

    public string Name { get; set; }
}