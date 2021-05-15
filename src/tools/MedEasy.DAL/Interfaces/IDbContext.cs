namespace MedEasy.DAL.Interfaces
{
    using System;

    using MedEasy.DAL.Repositories;

    using Microsoft.EntityFrameworkCore;

    public interface IDbContext : ITransactional, IDisposable
    {
        DbSet<T> Set<T>() where T : class;

    }
}