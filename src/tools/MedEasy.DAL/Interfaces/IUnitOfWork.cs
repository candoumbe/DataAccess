using System;
using System.Threading;
using System.Threading.Tasks;
using MedEasy.DAL.Repositories;

namespace MedEasy.DAL.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        
        /// <summary>
        /// Asynchronously saves changes made to the context
        /// </summary>
        /// <returns>Task</returns>
        Task<int> SaveChangesAsync();

        /// <summary>
        /// Asynchronously saves changes
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<int> SaveChangesAsync(CancellationToken token);

        /// <summary>
        /// Retrieves the repository
        /// </summary>
        /// <typeparam name="TEntry"></typeparam>
        /// <returns></returns>
        IRepository<TEntry> Repository<TEntry>() where TEntry : class;



    }
}
