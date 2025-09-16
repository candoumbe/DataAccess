using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Candoumbe.DataAccess.Abstractions;
using Candoumbe.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Candoumbe.DataAccess;

/// <summary>
/// Contains utility methods for Queryable
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Include a list of properties in a strongly-typed manner.
    /// </summary>
    /// <typeparam name="T">Type of the <see cref="IQueryable{T}"/> items</typeparam>
    /// <param name="entries"></param>
    /// <param name="includes">List of properties to include in the result</param>
    /// <returns></returns>
    public static IQueryable<T> Include<T>(this IQueryable<T> entries, IEnumerable<IncludeClause<T>> includes)
    {
        Expression queryExpression = entries.Expression;
        foreach (IncludeClause<T> includeClause in includes)
        {
            queryExpression = Expression.Call(typeof(EntityFrameworkQueryableExtensions),
                                              nameof(EntityFrameworkQueryableExtensions.Include),
                                              [entries.ElementType, includeClause.Expression.ReturnType],
                                              entries.Expression, includeClause.Expression);
        }

        return (IQueryable<T>)entries.Provider.CreateQuery(queryExpression);
    }

    /// <summary>
    /// Applies an order by clause to a queryable.
    /// </summary>
    /// <param name="entries">The queryable onto which the "order by" will be applied.</param>
    /// <param name="orderBy">The order by clause to apply</param>
    /// <typeparam name="T">Type of data hold by the queryable.</typeparam>
    /// <returns>The qu</returns>
    public static IQueryable<T> OrderBy<T>(this IQueryable<T> entries, IOrderSpecification<T> orderBy)
        => orderBy.Orders.Aggregate(entries, (current, order) => order.Direction switch
        {
            OrderDirection.Ascending => current.OrderBy(order.Expression),
            _                        => current.OrderByDescending(order.Expression)
        });
}