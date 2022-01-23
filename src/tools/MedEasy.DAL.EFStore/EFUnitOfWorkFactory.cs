namespace MedEasy.DAL.EFStore
{
    using MedEasy.DAL.Interfaces;

    using Microsoft.EntityFrameworkCore;

    using System;

    /// <summary>
    /// Defines a unit of work factory wrapper around Entity Framework
    /// </summary>
    public class EFUnitOfWorkFactory<TContext> : UnitOfWorkFactory
        where TContext : DbContext, IDbContext
    {
        /// <summary>
        /// Options used to create the unit of work instances
        /// </summary>
        public DbContextOptions<TContext> Options { get; }

        /// <summary>
        /// Factory function for new <typeparamref name="TContext"/> instances.
        /// </summary>
        public Func<DbContextOptions<TContext>, TContext> ContextGenerator { get; }

        /// <summary>
        /// Builds a new <see cref="EFUnitOfWorkFactory"/> instance
        /// </summary>
        /// <param name="options">options that will be used by the <see cref="EFUnitOfWork"/> returned by calling <see cref="NewUnitOfWork"/></param>
        /// <param name="contextGenerator">Function to call to create new <typeparamref name="TContext"/> instances.</param>
        /// <exception cref="ArgumentNullException">if <paramref name="contextGenerator"/> is <c>null</c>.</exception>
        public EFUnitOfWorkFactory(DbContextOptions<TContext> options, Func<DbContextOptions<TContext>, TContext> contextGenerator)
        {
            Options = options;
            ContextGenerator = contextGenerator ?? throw new ArgumentNullException(nameof(contextGenerator));
        }

        /// <summary>
        /// Creates new <see cref="EFUnitOfWork"/> instances.
        /// </summary>
        /// <remarks>
        /// Each call returns a new instance of <see cref="EFUnitOfWork"/> (which wraps its own <see cref="DbContext"/> instance)
        /// that can safely be used in multithreaded fashion.
        /// </remarks>
        /// <returns><see cref="EFUnitOfWork"/> instance</returns>
        public override IUnitOfWork NewUnitOfWork() => new EFUnitOfWork<TContext>(ContextGenerator.Invoke(Options));
    }
}