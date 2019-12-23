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
        /// Number of items the result that the current <see cref="Page{T}"/> contains
        /// </summary>
        public long Total { get; }

        /// <summary>
        /// Number of pages of result the current <see cref="Page{T}"/> is a part of.
        /// </summary>
        public long Count { get; }

        /// <summary>
        /// Number of items per <see cref="Page{T}"/>
        /// </summary>
        public long Size { get; }

        /// <summary>
        /// Builds a new <see cref="Page{T}"/> instances
        /// </summary>
        /// <param name="entries">Items of the current page</param>
        /// <param name="total">Number of items</param>
        /// <param name="size">Number of items per page.</param>
        /// <exception cref="ArgumentOutOfRangeException">if either<paramref name="total"/> or <paramref name="size"/> are negative</exception>
        /// <exception cref="ArgumentNullException">if <paramref name="entries"/> is <c>null</c></exception>
        public Page(IEnumerable<T> entries, long total, long size)
        {
            if (total < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(total), total, $"{nameof(total)} must not be a negative value");
            }

            if (size < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size), size, $"{nameof(size)} must not be a negative value");
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
        /// Gets an empty <see cref="Page{T}"/> instance.
        /// </summary>
        public static Page<T> Empty(in long pageSize) => new Page<T>(Enumerable.Empty<T>(), 0, pageSize);
    }
}