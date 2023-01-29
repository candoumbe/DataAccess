namespace Candoumbe.DataAccess.Repositories
{
    using DataFilters;

    /// <summary>
    /// Wraps criteria to search elements
    /// </summary>
    /// <typeparam name="T">Type of the element to perform search on.</typeparam>
    public class Search<T> where T : class
    {
        /// <summary>
        /// Filter to apply
        /// </summary>
        public IFilter Filter { get; set; }

        /// <summary>
        /// <see cref="IOrder{T}"/> that can be applied to the result of the search.
        /// </summary>
        public IOrder<T> Order { get; set; }
    }
}