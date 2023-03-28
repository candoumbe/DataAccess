﻿namespace Candoumbe.DataAccess.EFStore.UnitTests
{
    using Candoumbe.DataAccess.Abstractions;

    using Microsoft.EntityFrameworkCore;

    public class FakeDbContext : DbContext, IDbContext
    {

        public DbSet<Dummy> Dummies { get; set; }
    }

    public class Dummy
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
