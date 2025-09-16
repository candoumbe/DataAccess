using System;
using System.Linq.Expressions;

namespace Candoumbe.DataAccess.Abstractions;

/// <summary>
/// A specification that always returns true.
/// </summary>
/// <typeparam name="T"></typeparam>
public class AlwaysTrueSpecification<T> : IFilterSpecification<T>
{
    /// <inheritdoc />
    public Expression<Func<T, bool>> Filter { get; }

    private AlwaysTrueSpecification()
    {
        Filter = _ => true;
    }

    /// <summary>
    /// An instance of <see cref="AlwaysTrueSpecification{T}"/>.
    /// </summary>
    public static AlwaysTrueSpecification<T> Instance => new();
}


public class NotSpecification<T> : IFilterSpecification<T>
{
    /// <inheritdoc />
    public Expression<Func<T, bool>> Filter { get; }

    public NotSpecification(IFilterSpecification<T> specification)
    {
        Filter = Expression.Lambda<Func<T, bool>>(Expression.Not(specification.Filter.Body), specification.Filter.Parameters);
    }
}