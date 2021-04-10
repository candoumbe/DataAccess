using MedEasy.DAL.Interfaces;

using Raven.Client.Documents;

using System.Collections.Generic;

namespace MedEasy.DAL.RavenDb
{
    public class RavenDbUnitOfWorkFactory : IUnitOfWorkFactory
    {
        private readonly IDocumentStore _store;

        private readonly IDictionary<string, object> _repositories;

        public RavenDbUnitOfWorkFactory(IDocumentStore store)
        {
            _store = store;
            _repositories = new Dictionary<string, object>();
        }

        public IUnitOfWork NewUnitOfWork() => new RavenDbUnitOfWork(_store.OpenAsyncSession());
    }
}
