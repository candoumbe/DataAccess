using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Candoumbe.DataAccess.Abstractions;

/// <summary>
/// Defines the order to apply to the result of the search.
/// </summary>
/// <typeparam name="T">Type of the item onto which the specification will apply.</typeparam>
public interface IOrderSpecification<T>
{
    /// <summary>
    /// Defines the order to apply to the result of the search.
    /// </summary>
    IReadOnlyList<(Expression<Func<T, object>> Expression, OrderDirection Direction)> Orders { get; }
}