using System;

using MedEasy.DAL.Repositories;

using Microsoft.EntityFrameworkCore;

namespace MedEasy.DAL.Interfaces
{
    public interface IDbContext : ITransactional, IDisposable
    {
        DbSet<T> Set<T>() where T : class;
        
    }
}