using System;
using System.Linq.Expressions;

namespace Candoumbe.DataAccess.Abstractions;

/// <inheritdoc />
public class SelectSpecification<TFrom, TTo> : IProjectionSpecification<TFrom, TTo>
{
    /// <inheritdoc />
    public Expression<Func<TFrom, TTo>> Expression { get; }

    /// <summary>
    /// Builds a new <see cref="SelectSpecification{TFrom, TTo}"/> instance.
    /// </summary>
    /// <param name="selector">The</param>
    public SelectSpecification(Expression<Func<TFrom, TTo>> selector)
    {
        Expression = selector;
    }
}