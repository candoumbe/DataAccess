namespace Candoumbe.DataAccess.Abstractions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the basic contract of a "unit of work" which can be stated a set of actions that must be consider as a unit.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Asynchronously saves changes made to the context
        /// </summary>
        /// <param name="ct"></param>
        /// <returns>the number of elements modified</returns>
        Task SaveChangesAsync(CancellationToken ct = default);

        /// <summary>
        /// Retrieves the repository
        /// </summary>
        /// <typeparam name="TEntry"></typeparam>
        /// <returns></returns>
        IRepository<TEntry> Repository<TEntry>() where TEntry : class;
    }
}