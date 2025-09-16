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

/// <inheritdoc />
public class MultiOrderSpecification<T> : IOrderSpecification<T>
{
    private readonly List<(Expression<Func<T, object>> Expression, OrderDirection Direction)> _orders;

    /// <inheritdoc />
    public IReadOnlyList<(Expression<Func<T, object>> Expression, OrderDirection Direction)> Orders => _orders;

    /// <summary>
    /// Builds a new <see cref="MultiOrderSpecification{T}"/> instance.
    /// </summary>
    /// <exception cref="ArgumentNullException">if <paramref name="orders"/> is <see langword="null"/>.</exception>
    public MultiOrderSpecification(IEnumerable<(Expression<Func<T, object>> Expression, OrderDirection Direction)> orders)
    {
        _orders = [.. orders];
    }
}