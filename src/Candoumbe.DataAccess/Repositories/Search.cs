using Candoumbe.DataAccess.Abstractions;

namespace Candoumbe.DataAccess.Repositories;

/// <summary>
/// Wraps criteria to search elements
/// </summary>
/// <typeparam name="T">Type of the element to perform search on.</typeparam>
public class Search<T> where T : class
{
    /// <summary>
    /// Filter to apply
    /// </summary>
    public IFilterSpecification<T> Filter { get; set; }

    /// <summary>
    /// <see cref="IOrderSpecification{T}"/> that can be applied to the result of the search.
    /// </summary>
    public IOrderSpecification<T> Order { get; set; }
}