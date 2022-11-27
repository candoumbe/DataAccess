namespace Candoumbe.DataAccess.RavenDb
{
    using Candoumbe.DataAccess.Abstractions;

    using Raven.Client.Documents;

    using System.Collections.Concurrent;
    using System.Collections.Generic;

    /// <summary>
    /// Raven Db implementation of <see cref="IUnitOfWork"/>
    /// </summary>
    public class RavenDbUnitOfWorkFactory : IUnitOfWorkFactory
    {
        private readonly IDocumentStore _store;

        private readonly IDictionary<string, object> _repositories;

        /// <summary>
        /// Builds a new <see cref="RavenDbUnitOfWorkFactory"/> instance;
        /// </summary>
        /// <param name="store"></param>
        public RavenDbUnitOfWorkFactory(IDocumentStore store)
        {
            _store = store;
            _repositories = new ConcurrentDictionary<string, object>();
        }

        /// <inheritdoc/>
        public IUnitOfWork NewUnitOfWork() => new RavenDbUnitOfWork(_store.OpenAsyncSession());
    }
}
