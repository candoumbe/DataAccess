using System;
using System.Collections.Generic;
using System.Linq;

namespace MedEasy.DAL.Repositories
{
    /// <summary>
    /// Helper to encapsulates a page of results.
    /// </summary>
    /// <typeparam name="T">Type of elements the page will contains.</typeparam>
    public sealed class Page<T> 
    {
        /// <summary>
        /// Items of the current page
        /// </summary>
        public IEnumerable<T> Entries { get; }

        /// <summary>
        /// Number of items the result that the current <see cref="Page{T}"/>
        /// </summary>
        public int Total { get; }

        /// <summary>
        /// Number of pages the result that produces the current <see cref="Page{T}"/> contains.
        /// </summary>
        public int Count { get; }

        
        /// <summary>
        /// Number of items per <see cref="Page{T}"/>
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// Builds a new <see cref="Page{T}"/> instances
        /// </summary>
        /// <param name="entries">Items of the current page</param>
        /// <param name="total">Number of items</param>
        /// <param name="size">Number of items per page.</param>
        /// <exception cref="ArgumentOutOfRangeException">if either<paramref name="total"/> or <paramref name="size"/> are negative</exception>
        /// <exception cref="ArgumentNullException">if <paramref name="entries"/> is <c>null</c></exception>
        public Page(IEnumerable<T> entries, int total, int size)
        {
            if (total < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(total));
            }

            if (size < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            Entries = entries ?? throw new ArgumentNullException(nameof(entries));
            Total = total;
            Size = size;
            if (Size >= 1)
            {
                if (Total > 0)
                {
                    Count = (int)Math.Ceiling(Total / (decimal)Size);
                }
                else
                {
                    Count = 1;
                }
            }
            else
            {
                Count = 1;
            }
        }

        /// <summary>
        /// Lazy initialization of the Default value
        /// </summary>
        private static readonly Lazy<Page<T>> _lazy = new Lazy<Page<T>>(() => new Page<T>(Enumerable.Empty<T>(), 0, 0));

        /// <summary>
        /// Gets the default <see cref="Page{T}"/> instance for the specified type
        /// </summary>
        public static Page<T> Default => _lazy.Value;

    }
}