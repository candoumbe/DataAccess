using System.Linq;

namespace Candoumbe.DataAccess.Abstractions;

/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IContainer<out T> : IQueryable<T> where T : class
{
}