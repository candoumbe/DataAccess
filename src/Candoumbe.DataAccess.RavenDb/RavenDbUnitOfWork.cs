namespace Candoumbe.DataAccess.RavenDb
{
    using Candoumbe.DataAccess.Abstractions;
    using Candoumbe.DataAccess.Repositories;

    using Raven.Client.Documents.Session;

    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Raven db implementation of <see cref="IUnitOfWork"/>
    /// </summary>
    public class RavenDbUnitOfWork : IUnitOfWork
    {
        private readonly IAsyncDocumentSession _session;
        private readonly IDictionary<string, object> _repositories;

        /// <summary>
        /// Builds a new <see cref="RavenDbUnitOfWork"/> instance.
        /// </summary>
        /// <param name="session"></param>
        public RavenDbUnitOfWork(IAsyncDocumentSession session)
        {
            _session = session;
            _repositories = new Dictionary<string, object>();
        }

        /// <inheritdoc/>
        public void Dispose() => _session.Dispose();

        /// <inheritdoc/>
        public IRepository<TEntry> Repository<TEntry>() where TEntry : class
        {
            IRepository<TEntry> repository;

            if (_repositories.ContainsKey(typeof(TEntry).FullName))
            {
                repository = _repositories[typeof(TEntry).FullName] as IRepository<TEntry>;
            }
            else
            {
                repository = new RavenDbRepository<TEntry>(_session);
                _repositories.Add(typeof(TEntry).FullName, repository);
            }

            return repository;
        }

        /// <inheritdoc/>
        public async Task SaveChangesAsync(CancellationToken ct = default) => await _session.SaveChangesAsync(ct).ConfigureAwait(false);
    }
}
