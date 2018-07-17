
using MedEasy.DAL.Interfaces;

namespace MedEasy.DAL.EFStore
{
    /// <summary>
    /// Unit of work implementation that relies on Entity Framework.
    /// </summary>
    public class EFUnitOfWork<TContext> : UnitOfWork<TContext> where TContext : IDbContext
    {
        /// <summary>
        /// Builds a new <see cref="EFUnitOfWork"/> instance.
        /// </summary>
        /// <param name="context">The context used by this unit of work instance.</param>
        public EFUnitOfWork(TContext context) : base(context) {}
        
    }
}