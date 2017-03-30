using System;
using System.Collections.Generic;
using System.Linq;

namespace MedEasy.DAL.Repositories
{
    /// <summary>
    /// Helper to encapsulates a page of results.
    /// </summary>
    /// <typeparam name="T">Type of elements the page will contains.</typeparam>
    public sealed class PagedResult<T> : IPagedResult<T>
    {
        /// <summary>
        /// Items of the current page
        /// </summary>
        public IEnumerable<T> Entries { get; }

        /// <summary>
        /// 
        /// </summary>
        public int Total { get; }

        /// <summary>
        /// Number of pages
        /// </summary>
        public int PageCount { get; }

        
        /// <summary>
        /// Number of items per page
        /// </summary>
        public int PageSize { get; }

        /// <summary>
        /// Builds a new <see cref="PagedResult{T}"/> instances
        /// </summary>
        /// <param name="entries">Items of the current page</param>
        /// <param name="total">Number of items</param>
        /// <param name="pageSize">Number of items per pages</param>
        /// <exception cref="ArgumentOutOfRangeException">if either<paramref name="total"/> or <paramref name="pageSize"/> are negative</exception>
        /// <exception cref="ArgumentNullException">if <paramref name="entries"/> is <c>null</c></exception>
        public PagedResult(IEnumerable<T> entries, int total, int pageSize)
        {
            if (total < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(total));
            }

            if (pageSize < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize));
            }

            Entries = entries ?? throw new ArgumentNullException(nameof(entries));
            Total = total;
            PageSize = pageSize;
            PageCount = PageSize >= 1 
                ? (int) Math.Ceiling(Total / (decimal)pageSize) 
                : 0;
        }

        private static object Lock => new object();
        /// <summary>
        /// Lazy initialization of the Default value
        /// </summary>
        private static readonly Lazy<PagedResult<T>> lazy = new Lazy<PagedResult<T>>(() => new PagedResult<T>(Enumerable.Empty<T>(), 0, 0));

        /// <summary>
        /// Gets the default <see cref="PagedResult{T}"/> instance for the specified type
        /// </summary>
        public static PagedResult<T> Default => lazy.Value;

    }
}