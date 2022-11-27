namespace Candoumbe.DataAccess.Repositories
{
    using Candoumbe.DataAccess.Abstractions;

    using System;

    /// <summary>
    /// Repository base class
    /// </summary>
    /// <typeparam name="TEntry">Type of entities the repository will manage.</typeparam>
    public abstract class RepositoryBase<TEntry> where TEntry : class
    {
        /// <summary>
        /// <see cref="IDbContext"/> which the current instance operates on.
        /// </summary>
        protected IDbContext Context { get; }

        /// <summary>
        /// Builds a new <see cref="RepositoryBase{TEntry}"/> that handles <typeparamref name="TEntry"/>
        /// </summary>
        /// <param name="context"></param>
        /// <exception cref="ArgumentNullException">if <paramref name="context"/> is <see langword="null"/></exception>
        protected RepositoryBase(IDbContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }
    }
}