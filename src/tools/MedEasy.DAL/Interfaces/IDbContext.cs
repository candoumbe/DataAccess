using System;
using MedEasy.DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace MedEasy.DAL.Interfaces
{
    public interface IDbContext : ITransactional, IDisposable
    {
        DbSet<T> Set<T>() where T : class;
        
    }
}