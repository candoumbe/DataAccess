using MedEasy.DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace System.Linq
{
    /// <summary>
    /// Contains utility methods for Queryable
    /// </summary>
    public static class QueryableExtensions
    {
        /// <summary>
        /// Include a list of properties in a strongly type manner.
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
                queryExpression = Expression.Call(
                    typeof(EntityFrameworkQueryableExtensions),
                    nameof(EntityFrameworkQueryableExtensions.Include),
                    new Type[] { entries.ElementType, includeClause.Expression.ReturnType },
                    entries.Expression, includeClause.Expression
                );
            }
    
            return (IQueryable<T>) entries.Provider.CreateQuery(queryExpression);
        }
    }
}
