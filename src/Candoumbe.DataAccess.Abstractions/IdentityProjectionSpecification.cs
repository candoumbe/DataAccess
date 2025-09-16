using System;
using System.Linq.Expressions;

namespace Candoumbe.DataAccess.Abstractions;

/// <summary>
/// A specification that returns the same object it is applied to.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class IdentityProjectionSpecification<T> : IProjectionSpecification<T, T>
{
    /// <inheritdoc />
    public Expression<Func<T, T>> Expression { get; }

    private IdentityProjectionSpecification()
    {
        Expression = x => x;
    }

    /// <summary>
    /// Creates an instance of <see cref="IdentityProjectionSpecification{T}"/>.
    /// </summary>
    public static IdentityProjectionSpecification<T> Instance => new();
}