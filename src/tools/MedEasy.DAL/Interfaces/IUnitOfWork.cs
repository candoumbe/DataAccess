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
        /// <param name="cancellationToken"></param>
        /// <returns>the number of elements modified</returns>
       Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the repository
        /// </summary>
        /// <typeparam name="TEntry"></typeparam>
        /// <returns></returns>
        IRepository<TEntry> Repository<TEntry>() where TEntry : class;



    }
}
