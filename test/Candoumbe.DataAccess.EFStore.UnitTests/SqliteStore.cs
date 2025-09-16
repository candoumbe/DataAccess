using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Candoumbe.DataAccess.Abstractions;
using Candoumbe.DataAccess.EFStore.UnitTests.Entities;
using Microsoft.EntityFrameworkCore;

namespace Candoumbe.DataAccess.EFStore.UnitTests;

public class SqliteStore : DbContext, IStore
{
    public DbSet<Hero> Heroes { get; set; }
    public DbSet<Acolyte> Acolytes { get; set; }

    public DbSet<Weapon> Weapons { get; set; }

    public IQueryable<T> Query<T>() where T : class => Set<T>();

    /// <inheritdoc />
    IContainer<T> IStore.Set<T>() => new EfContainer<T>(Set<T>());

    /// <summary>
    /// Builds a new instance of <see cref="SqliteStore"/>.
    /// </summary>
    /// <param name="options"></param>
    public SqliteStore(DbContextOptions<SqliteStore> options) : base(options) { }

    ///<inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new HeroEntityTypeConfiguration())
            .ApplyConfiguration(new AcolyteEntityTypeConfiguration())
            .ApplyConfiguration(new WeaponEntityTypeConfiguration());
    }

    private class EfContainer<T> : IContainer<T>
        where T : class
    {
        private readonly IQueryable<T> _queryable;

        public EfContainer(IQueryable<T> queryable)
        {
            _queryable = queryable ?? throw new ArgumentNullException(nameof(queryable));
        }

        public Type ElementType => _queryable.ElementType;
        public Expression Expression => _queryable.Expression;
        public IQueryProvider Provider => _queryable.Provider;
        public IEnumerator<T> GetEnumerator() => _queryable.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}