using System.Collections.Generic;

namespace Candoumbe.DataAccess.Abstractions
{
    using System;

    /// <summary>
    /// Interface for accessing a set of entries.
    /// </summary>
    public interface IStore : ITransactional, IDisposable
    {
        /// <summary>
        /// Set of entries the current instance provide.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IContainer<T> Set<T>() where T : class;
    }
}