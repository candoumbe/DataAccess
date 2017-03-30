using System.Collections.Generic;

namespace MedEasy.DAL.Repositories
{
    /// <summary>
    /// A page of result
    /// </summary>
    /// <typeparam name="T">Type of items</typeparam>
    public interface IPagedResult<T>
    {
        /// <summary>
        /// Items of the current page
        /// </summary>
        IEnumerable<T> Entries { get; }

        /// <summary>
        /// Number of items of the result
        /// </summary>
        int Total { get; }

        /// <summary>
        /// Page size
        /// </summary>
        int PageSize { get; }

        /// <summary>
        /// Number of pages
        /// </summary>
        int PageCount { get; }
    }
}