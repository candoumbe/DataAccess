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
        /// Orders the <paramref name="entries"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entries"></param>
        /// <param name="orderBy">List of <see cref="OrderClause{T}"/></param>
        /// <returns></returns>
        public static IQueryable<T> OrderBy<T>(this IQueryable<T> entries, IEnumerable<OrderClause<T>> orderBy)
        {
            OrderClause<T> previousClause = null;
            foreach (OrderClause<T> orderClause in orderBy)
            {
                switch (orderClause.Direction)
                {
                    case SortDirection.Ascending:
                        entries = previousClause != null
                            ? Queryable.ThenBy(entries, (dynamic)orderClause.Expression)
                            : Queryable.OrderBy(entries, (dynamic)orderClause.Expression);

                        break;
                    case SortDirection.Descending:
                        entries = previousClause != null
                            ? Queryable.ThenByDescending(entries, (dynamic)orderClause.Expression)
                            : Queryable.OrderByDescending(entries, (dynamic)orderClause.Expression);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                previousClause = orderClause;
            }
            return entries;
        }

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
