namespace Candoumbe.DataAccess.Abstractions;

/// <summary>
/// Direction of order to apply when retrieving data from repositories.
/// </summary>
public enum OrderDirection
{
    /// <summary>
    /// Instructs the repository to order the result in ascending order.
    /// </summary>
    Ascending,
    /// <summary>
    /// Instructs the repository to order the result in descending order.
    /// </summary>
    Descending
}