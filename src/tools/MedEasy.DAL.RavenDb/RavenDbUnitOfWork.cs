using MedEasy.DAL.Interfaces;
using MedEasy.DAL.Repositories;

using Raven.Client.Documents.Session;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MedEasy.DAL.RavenDb
{
    public class RavenDbUnitOfWork : IUnitOfWork
    {
        private readonly IAsyncDocumentSession _session;
        private readonly IDictionary<string, object> _repositories;

        public RavenDbUnitOfWork(IAsyncDocumentSession session)
        {
            _session = session;
            _repositories = new Dictionary<string, object>();
        }

        public void Dispose() => _session.Dispose();
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

        public async Task SaveChangesAsync(CancellationToken ct = default) => await _session.SaveChangesAsync(ct);

    }
}
