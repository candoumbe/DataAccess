using MedEasy.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace MedEasy.DAL.EFStore
{
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
        public Func<DbContextOptions<TContext>, TContext> ContextGenerator { get; }



        /// <summary>
        /// Builds a new <see cref="EFUnitOfWorkFactory"/> instance
        /// </summary>
        /// <param name="options">options that will be used by the <see cref="EFUnitOfWork"/> returned by calling <see cref="NewUnitOfWork"/></param>
        /// <param name="contextGenerator"></param>
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
        public override IUnitOfWork NewUnitOfWork() => new EFUnitOfWork<TContext>(ContextGenerator(Options));
    }
}