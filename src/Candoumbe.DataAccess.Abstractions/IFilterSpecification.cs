using System;
using System.Linq.Expressions;

namespace Candoumbe.DataAccess.Abstractions;

/// <summary>
/// Defines the filter to apply to the result of the search.
/// </summary>
/// <typeparam name="T">Type onto which the specification can be applied.</typeparam>
public interface IFilterSpecification<T>
{
    /// <summary>
    /// Filter to apply.
    /// </summary>
    Expression<Func<T, bool>> Filter { get; }
}