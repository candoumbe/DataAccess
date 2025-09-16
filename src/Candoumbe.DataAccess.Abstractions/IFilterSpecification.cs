using System;
using System.Linq.Expressions;

namespace Candoumbe.DataAccess.Repositories;

/// <summary>
/// Defines the filter to apply to the result of the search.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IFilterSpecification<T>
{
    /// <summary>
    /// Filter to apply.
    /// </summary>
    Expression<Func<T, bool>> Filter { get; }
}