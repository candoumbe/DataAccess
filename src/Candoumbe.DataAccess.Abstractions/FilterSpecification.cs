using System;
using System.Linq.Expressions;

namespace Candoumbe.DataAccess.Abstractions;

/// <inheritdoc />
public class FilterSpecification<T> : IFilterSpecification<T>
{
    /// <inheritdoc />
    public Expression<Func<T, bool>> Filter { get; }

    /// <summary>
    /// Builds a new <see cref="filter"/> instance.
    /// </summary>
    /// <param name="filter"></param>
    /// <exception cref="FilterSpecification{T}"></exception>
    public FilterSpecification(Expression<Func<T, bool>> filter)
    {
        Filter = filter ?? throw new ArgumentNullException(nameof(filter));
    }

    /// <summary>
    /// Implicit conversion from <see cref="FilterSpecification{T}"/> to <see cref="Expression{T}"/>.
    /// </summary>
    /// <param name="specification"></param>
    /// <returns></returns>
    public static implicit operator Expression<Func<T, bool>>(FilterSpecification<T> specification) => specification.Filter;

    /// <summary>
    /// Implicit conversion from <see cref="FilterSpecification{T}"/> to <see cref="Func{T, bool}"/>.
    /// </summary>
    /// <param name="specification"></param>
    /// <returns></returns>
    public static implicit operator Func<T, bool>(FilterSpecification<T> specification) => specification.Filter.Compile();
}