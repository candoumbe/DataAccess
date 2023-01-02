namespace Candoumbe.DataAccess.Abstractions
{
    using Microsoft.EntityFrameworkCore;

    using System;

    /// <summary>
    /// Interface for accessing a set of entries.
    /// </summary>
    public interface IDbContext : ITransactional, IDisposable
    {
        /// <summary>
        /// Set of entries the current instance provide.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        DbSet<T> Set<T>() where T : class;
    }
}