namespace MedEasy.DAL.Repositories
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// This interface defines methods to implement to support transactional operations
    /// </summary>
    public interface ITransactional
    {
        /// <summary>
        /// Asynchronously saves underlying changes
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>The number of changes successfully commited</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
