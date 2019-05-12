using System;
using MedEasy.DAL.Interfaces;

namespace MedEasy.DAL.Repositories
{
    /// <summary>
    /// Repository base class
    /// </summary>
    /// <typeparam name="TEntry"></typeparam>
    public abstract class RepositoryBase<TEntry> where TEntry : class
    {
        protected IDbContext Context { get; }

        protected RepositoryBase(IDbContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }
    }
}