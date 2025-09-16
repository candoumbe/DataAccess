using System;
using System.Linq.Expressions;
using Candoumbe.MiscUtilities;

namespace Candoumbe.DataAccess.Abstractions;

/// <summary>
/// Specifies the projection to perform from <typeparamref name="TFrom"/> to <typeparamref name="TTo"/>.
/// </summary>
/// <typeparam name="TFrom">Source type</typeparam>
/// <typeparam name="TTo">Destination type</typeparam>
public interface IProjectionSpecification<TFrom, TTo>
{
    /// <summary>
    /// The expression to perform the projection.
    /// </summary>
    Expression<Func<TFrom, TTo>> Expression { get; }
}