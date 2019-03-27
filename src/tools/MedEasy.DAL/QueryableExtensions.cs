using MedEasy.DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

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
            if (includes != null)
            {
                foreach (IncludeClause<T> includeClause in includes)
                {
                    entries = EntityFrameworkQueryableExtensions.Include(entries, (dynamic)includeClause.Expression);
                }
            }
            return entries;
        }
    }
}
