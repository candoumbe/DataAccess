namespace Candoumbe.DataAccess.Repositories
{
    using DataFilters;

    using Optional;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Contract for repositories.
    /// </summary>
    /// <typeparam name="TEntry">Type of entity the repository will manage.</typeparam>
    public interface IRepository<TEntry> where TEntry : class
    {
        /// <summary>
        /// <para>
        ///     Reads all entries from the repository.
        /// </para>
        /// <para>
        ///
        /// </para>
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="selector">The selector.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="page">Index of the page.</param>
        /// <param name="orderBy">The order by clause to apply BEFORE <paramref name="selector"/>.</param>
        /// <param name="ct">Notifies to cancel the execution of the request</param>
        /// <returns><see cref="Page{T}"/> which holds the result</returns>
        ValueTask<Page<TResult>> ReadPageAsync<TResult>(Expression<Func<TEntry, TResult>> selector,
                                                        int pageSize,
                                                        int page,
                                                        ISort<TResult> orderBy = null,
                                                        CancellationToken ct = default);

        /// <summary>
        ///     Reads all entries from the repository.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="selector">The selector.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="page">Index of the page.</param>
        /// <param name="orderBy">The order by clause to apply BEFORE <paramref name="selector"/>.</param>
        /// <param name="ct">Notifies to cancel the execution of the request</param>
        /// <returns><see cref="Page{T}"/> which holds the result</returns>
        ValueTask<Page<TResult>> ReadPageAsync<TResult>(Expression<Func<TEntry, TResult>> selector,
                                                        int pageSize,
                                                        int page,
                                                        ISort<TEntry> orderBy = null,
                                                        CancellationToken ct = default);

        /// <summary>
        /// Gets all entries of the repository
        /// </summary>
        /// <param name="ct">Notifies to cancel the execution of the request</param>
        /// <returns><see cref="IEnumerable{T}"/></returns>
        ValueTask<IEnumerable<TEntry>> ReadAllAsync(CancellationToken ct = default);

        /// <summary>
        /// Gets all entries of the repository after applying <paramref name="selector"/>
        /// </summary>
        /// <typeparam name="TResult">Type of the result</typeparam>
        /// <param name="selector">projection to apply before retrieving the result.</param>
        /// <param name="ct">Token to stop query from running</param>
        /// <returns></returns>
        ValueTask<IEnumerable<TResult>> ReadAllAsync<TResult>(Expression<Func<TEntry, TResult>> selector, CancellationToken ct = default);

        //IEnumerable<GroupedResult<TKey, TEntry>>  GroupBy<TKey>(Expression<Func<TEntry, TKey>> keySelector);

        //ValueTask<IEnumerable<GroupedResult<TKey, TEntry>>> GroupByAsync<TKey>(Expression<Func<TEntry, TKey>> keySelector);

        //IEnumerable<GroupedResult<TKey, TResult>> GroupBy<TKey, TResult>( Expression<Func<TEntry, TKey>> keySelector, Expression<Func<TEntry, TResult>> selector);

        //ValueTask<IEnumerable<GroupedResult<TKey, TResult>>> GroupByAsync<TKey, TResult>( Expression<Func<TEntry, TKey>> keySelector, Expression<Func<TEntry, TResult>> selector);

        /// <summary>
        /// Gets entries of the repository that satisfied the specified <paramref name="predicate"/>
        /// </summary>
        /// <param name="predicate">Filter the entries to retrieve</param>
        /// <param name="ct"></param>
        /// <returns><see cref="IEnumerable{T}"/></returns>
        ValueTask<IEnumerable<TEntry>> WhereAsync(Expression<Func<TEntry, bool>> predicate, CancellationToken ct = default);

        /// <summary>
        /// Gets entries of the repository that satisfied the specified <paramref name="predicate"/>
        /// </summary>
        /// <param name="predicate">Filter the entries to retrieve</param>
        /// <param name="ct"></param>
        /// <returns><see cref="IAsyncEnumerable{T}"/></returns>
        IAsyncEnumerable<TEntry> Stream(Expression<Func<TEntry, bool>> predicate, CancellationToken ct = default);

        /// <summary>
        /// Gets entries of the repository that satisfied the specified <paramref name="predicate"/>
        /// </summary>
        /// <param name="selector"></param>
        /// <param name="predicate"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        IAsyncEnumerable<TResult> Stream<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate, CancellationToken ct = default);

        /// <summary>
        /// Gets entries of the repository that satisfied the specified <paramref name="predicate"/>
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="selector"></param>
        /// <param name="predicate"></param>
        /// <param name="ct">Notifies to cancel the execution of the request</param>
        /// <returns><see cref="IEnumerable{T}"/></returns>
        ValueTask<IEnumerable<TResult>> WhereAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate, CancellationToken ct = default);

        /// <summary>
        /// Retrieves entries grouped using the specified <paramref name="keySelector"/>
        /// </summary>
        /// <typeparam name="TKey">Type of the element that will serve to "group" entries together</typeparam>
        /// <typeparam name="TResult">Type of the group result</typeparam>
        /// <param name="predicate">Predicate that will be used to filter groups</param>
        /// <param name="keySelector">Selector which defines how results should be grouped</param>
        /// <param name="groupSelector"></param>
        /// <param name="ct">Notifies to cancel the execution of the request</param>
        /// <returns><see cref="IEnumerable{T}"/></returns>
        ValueTask<IEnumerable<TResult>> WhereAsync<TKey, TResult>(
            Expression<Func<TEntry, bool>> predicate,
            Expression<Func<TEntry, TKey>> keySelector,
            Expression<Func<IGrouping<TKey, TEntry>, TResult>> groupSelector, CancellationToken ct = default);

        /// <summary>
        /// Gets a <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <param name="predicate">predicate to apply</param>
        /// <param name="orderBy">order to apply to the result</param>
        /// <param name="includedProperties">Properties to include in each object</param>
        /// <param name="ct">Notifies to cancel the execution of the request</param>
        /// <returns><see cref="IEnumerable{T}"/> which holds the resu;t</returns>
        ValueTask<IEnumerable<TEntry>> WhereAsync(
            Expression<Func<TEntry, bool>> predicate,
            ISort<TEntry> orderBy = null,
            IEnumerable<IncludeClause<TEntry>> includedProperties = null, CancellationToken ct = default);

        /// <summary>
        /// Gets results that satisfied the <paramref name="predicate"/>
        /// </summary>
        /// <remarks>
        /// The <paramref name="orderBy"/> is applied <strong>AFTER</strong> the <paramref name="selector"/> and <paramref name="predicate"/>.
        /// </remarks>
        /// <typeparam name="TResult">Type of result's items</typeparam>
        /// <param name="selector">Expression to convert from <typeparamref name="TEntry"/> to <typeparamref name="TResult"/></param>
        /// <param name="predicate">Filter to match</param>
        /// <param name="orderBy">Sort to apply.</param>
        /// <param name="includedProperties">Collection of <see cref="IncludeClause{T}"/> that describes properties to eagerly fetch for each item in the result</param>
        /// <param name="ct">Notifies to cancel the execution of the request</param>
        /// <returns>Collection of <typeparamref name="TResult"/> </returns>
        ValueTask<IEnumerable<TResult>> WhereAsync<TResult>(
            Expression<Func<TEntry, TResult>> selector,
            Expression<Func<TEntry, bool>> predicate,
            ISort<TResult> orderBy = null,
            IEnumerable<IncludeClause<TEntry>> includedProperties = null, CancellationToken ct = default);

        /// <summary>
        /// Gets results that satisfied the <paramref name="predicate"/>
        /// </summary>
        /// <remarks>
        /// The <paramref name="orderBy"/> is applied <strong>AFTER</strong> the <paramref name="selector"/> and <paramref name="predicate"/>.
        /// </remarks>
        /// <typeparam name="TResult">Type of result's items</typeparam>
        /// <param name="selector">Expression to convert from <typeparamref name="TEntry"/> to <typeparamref name="TResult"/></param>
        /// <param name="predicate">Filter to match AFTER <paramref name="selector"/> has been applied.</param>
        /// <param name="orderBy">Sort expression to apply to the collection of <typeparamref name="TResult"/>.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        ValueTask<IEnumerable<TResult>> WhereAsync<TResult>(
           Expression<Func<TEntry, TResult>> selector,
           Expression<Func<TResult, bool>> predicate,
           ISort<TResult> orderBy = null,
           CancellationToken cancellationToken = default);

        /// <summary>
        /// gets a <see cref="Page{T}"/>.
        /// </summary>
        /// <param name="predicate">predicate to apply</param>
        /// <param name="orderBy">order to apply to the result</param>
        /// <param name="pageSize">number of items one page can contain at most</param>
        /// <param name="page">the page of result to get (1 for the page, 2 for the second, ...)</param>
        /// <param name="cancellationToken"></param>
        /// <returns><see cref="Page{T}"/> which holds the </returns>
        ValueTask<Page<TEntry>> WhereAsync(
            Expression<Func<TEntry, bool>> predicate,
            ISort<TEntry> orderBy,
            int pageSize,
            int page, CancellationToken cancellationToken = default);

        /// <summary>
        /// gets a <see cref="Page{T}"/> of entries that satisfied the <paramref name="predicate"/>
        /// </summary>
        /// <remarks>
        /// The <paramref name="predicate"/> is apply <strong>BEFORE</strong> <paramref name="selector"/> is applied.
        /// The <paramref name="orderBy"/> is applied <strong>AFTER</strong> both <paramref name="selector"/> and <paramref name="predicate"/> where applied
        /// </remarks>
        /// <typeparam name="TResult">Type of items of the result</typeparam>
        /// <param name="selector">selector to apply</param>
        /// <param name="predicate">filter that entries must satisfied</param>
        /// <param name="orderBy">order to apply</param>
        /// <param name="pageSize">number of items a page can holds at most</param>
        /// <param name="page">the page of result to get.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">if either <paramref name="selector"/> or <paramref name="predicate"/>
        ///  or <paramref name="orderBy"/> is <c>null</c>.
        /// </exception>
        ValueTask<Page<TResult>> WhereAsync<TResult>(
            Expression<Func<TEntry, TResult>> selector,
            Expression<Func<TEntry, bool>> predicate,
            ISort<TResult> orderBy,
            int pageSize,
            int page, CancellationToken cancellationToken = default);

        /// <summary>
        /// gets a <see cref="Page{T}"/> of entries that satisfied the <paramref name="predicate"/>
        /// </summary>
        /// <remarks>
        /// The <paramref name="predicate"/> is apply <strong>AFTER</strong> <paramref name="selector"/> is applied.
        /// The <paramref name="orderBy"/> is applied <strong>AFTER</strong> both <paramref name="selector"/> and <paramref name="predicate"/> where applied
        /// </remarks>
        /// <typeparam name="TResult">Type of items of the result</typeparam>
        /// <param name="selector">selector to apply</param>
        /// <param name="predicate">filter that entries must satisfied</param>
        /// <param name="orderBy">order to apply</param>
        /// <param name="pageSize">number of items a page can holds at most</param>
        /// <param name="page">the page of result to get.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        ValueTask<Page<TResult>> WhereAsync<TResult>(
            Expression<Func<TEntry, TResult>> selector,
            Expression<Func<TResult, bool>> predicate,
            ISort<TResult> orderBy, int pageSize, int page, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the max value of the selected element
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="selector"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        ValueTask<TResult> MaxAsync<TResult>(Expression<Func<TEntry, TResult>> selector, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the mininum value after applying the <paramref name="selector"/>
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="selector">The projection to make before getting the minimum</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The minimum value</returns>
        ValueTask<TResult> MinAsync<TResult>(Expression<Func<TEntry, TResult>> selector, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if the current repository contains at least one entry
        /// </summary>
        /// <returns>
        ///     <see langword="true"/> if the repository contains at least one element or <see langword="false"/> otherwise
        /// </returns>
        ValueTask<bool> AnyAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if the current repository contains one entry at least that match <paramref name="predicate"/>
        /// </summary>
        /// <param name="predicate">predicate to match</param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        ///     <see langword="true"/> if the repository contains at least one element or <see langword="false"/> otherwise
        /// </returns>
        ValueTask<bool> AnyAsync(Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the number of entries in the repository.
        /// </summary>
        /// <returns>
        ///     the number of entries in the repository
        /// </returns>
        ValueTask<int> CountAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the number of entries in the repository that honor the <paramref name="predicate"/>.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        ///     the number of entries in the repository
        /// </returns>
        ValueTask<int> CountAsync(Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the entry entry of the repository
        /// </summary>
        /// <returns>
        /// the single entry of the repository
        /// </returns>
        ValueTask<TEntry> SingleAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the single entry that corresponds to the specified <paramref name="predicate"/>.
        /// </summary>
        /// <param name="predicate">Filter to match</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">if no entry or more than one entry matches <paramref name="predicate"/>.</exception>
        ValueTask<TEntry> SingleAsync(Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the single entry that matches <paramref name="predicate"/>.
        /// </summary>
        /// <typeparam name="TResult">Type of the result</typeparam>
        /// <param name="selector">selector to convert from <typeparamref name="TEntry"/> to <typeparamref name="TResult"/></param>
        /// <param name="predicate">predicate the entry to match should match.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The entry that matches <paramref name="predicate"/>.</returns>
        /// <exception cref="InvalidOperationException">if no entry or more than one entry matches <paramref name="predicate"/>.</exception>
        /// <exception cref="ArgumentNullException">if either <paramref name="selector"/> or <paramref name="predicate"/> is <c>null</c></exception>
        ValueTask<TResult> SingleAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the single <typeparamref name="TEntry"/> of the repository.
        /// Throws <see cref="ArgumentException"/> if there's more than one entry in the repository
        /// </summary>
        /// <returns><see cref="Option.None{T}"/> if there no entry in the repository</returns>
        ValueTask<Option<TEntry>> SingleOrDefaultAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the single <typeparamref name="TEntry"/> of the repository.
        /// Throws <see cref="ArgumentException"/> if there's more than one entry in the repository
        /// </summary>
        /// <param name="includedProperties">Properties to eagerly fetch and load</param>
        /// <param name="cancellationToken"></param>
        /// <returns><c>null</c> if there no entry in the repository</returns>
        ValueTask<Option<TEntry>> SingleOrDefaultAsync(IEnumerable<IncludeClause<TEntry>> includedProperties, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the single <typeparamref name="TEntry"/> element of the repository that fullfill the
        /// <paramref name="predicate"/>
        /// </summary>
        /// <param name="predicate">Predicate which should gets one result at most</param>
        /// <param name="cancellationToken"></param>
        /// <returns>the corresponding entry or <c>null</c> if no entry found</returns>
        /// <exception cref="InvalidOperationException">if more than one entry matches <paramref name="predicate"/>.</exception>
        ValueTask<Option<TEntry>> SingleOrDefaultAsync(Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the single <typeparamref name="TEntry"/> element of the repository that fullfill the
        /// <paramref name="predicate"/>
        /// </summary>
        /// <param name="predicate">Predicate which should gets one result at most</param>
        /// <param name="includedProperties">Properties to eagerly include.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>the corresponding entry or <c>null</c> if no entry found</returns>
        /// <exception cref="InvalidOperationException">if more than one entry matches <paramref name="predicate"/>.</exception>
        /// <exception cref="ArgumentNullException">if either <paramref name="predicate"/> or <paramref name="includedProperties"/> is <c>null</c></exception>
        ValueTask<Option<TEntry>> SingleOrDefaultAsync(Expression<Func<TEntry, bool>> predicate, IEnumerable<IncludeClause<TEntry>> includedProperties, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the one and only entry that match <paramref name="predicate"/>.
        /// </summary>
        /// <remarks>
        ///     The <paramref name="predicate"/> is applied prior the <paramref name="selector"/>.
        /// </remarks>
        /// <typeparam name="TResult">Type of the result of the projection</typeparam>
        /// <param name="selector">Projection to apply after finding the entry that matches <paramref name="predicate"/></param>
        /// <param name="predicate">Filter to match</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The entry that matches <paramref name="predicate"/> or <c>null</c> if no matches found</returns>
        /// <exception cref="InvalidOperationException">if no entry or more than one entry matches <paramref name="predicate"/>.</exception>
        /// <exception cref="ArgumentNullException">if either <paramref name="selector"/> or <paramref name="predicate"/> is <c>null</c></exception>
        ValueTask<Option<TResult>> SingleOrDefaultAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the one and only entry that match <paramref name="predicate"/>.
        /// </summary>
        /// <remarks>
        ///     The <paramref name="predicate"/> is applied prior the <paramref name="selector"/>.
        /// </remarks>
        /// <typeparam name="TResult">Type of the result of the projection</typeparam>
        /// <param name="selector">Projection to apply after finding the entry that matches <paramref name="predicate"/></param>
        /// <param name="predicate">Filter to apply to echj</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The entry that matches <paramref name="predicate"/> or <c>null</c> if no matches found</returns>
        /// <exception cref="InvalidOperationException">if no entry or more than one entry matches <paramref name="predicate"/>.</exception>
        /// <exception cref="ArgumentNullException">if either <paramref name="selector"/> or <paramref name="predicate"/> is <c>null</c></exception>
        ValueTask<Option<TResult>> SingleOrDefaultAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TResult, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the first entry of the repository
        /// </summary>
        /// <returns>The first entry of the repository</returns>
        /// <exception cref="InvalidOperationException">if no entry found.</exception>
        ValueTask<TEntry> FirstAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the first entry of the repository
        /// </summary>
        /// <returns>The first entry or <c>null</c> if there's no entry.</returns>
        ValueTask<Option<TEntry>> FirstOrDefaultAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the first entry of the repository that fullfill the specified <paramref name="predicate"/>
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>The first entry that mat</returns>
        /// <exception cref="InvalidOperationException">if no entry matches <paramref name="predicate"/>.</exception>
        /// <exception cref="ArgumentNullException">if <paramref name="predicate"/> is <c>null</c></exception>
        ValueTask<TEntry> FirstAsync(Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the first entry of the repository that fullfill the specified <paramref name="predicate"/>
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>The first entry that mat</returns>
        /// <exception cref="InvalidOperationException">if no entry matches <paramref name="predicate"/>.</exception>
        /// <exception cref="ArgumentNullException">if <paramref name="predicate"/> is <c>null</c></exception>
        ValueTask<Option<TEntry>> FirstOrDefaultAsync(Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete all entries that match <paramref name="predicate"/>
        /// </summary>
        /// <param name="predicate"></param>
        /// <exception cref="ArgumentNullException">if <paramref name="predicate"/> is <c>nuull</c></exception>
        void Delete(Expression<Func<TEntry, bool>> predicate);

        /// <summary>
        /// Delete all entries of the underlying datastore
        /// </summary>
        void Clear();

        /// <summary>
        /// Gets the first entry that matches <paramref name="predicate"/>
        /// </summary>
        /// <typeparam name="TResult">Type of the result</typeparam>
        /// <param name="selector">Projection to apply to the entry found</param>
        /// <param name="predicate">Filter to match</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The entry that matches <paramref name="predicate"/>.</returns>
        /// <exception cref="InvalidOperationException">if the repository is empty or no entry matches <paramref name="predicate"/></exception>
        ValueTask<TResult> FirstAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the first entry that matches <paramref name="predicate"/>
        /// </summary>
        /// <typeparam name="TResult">Type of the result</typeparam>
        /// <param name="selector">Projection to apply to the entry found</param>
        /// <param name="predicate">Filter to match</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The entry that matches <paramref name="predicate"/> or <c>null</c> if no entry found.</returns>
        ValueTask<Option<TResult>> FirstOrDefaultAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates the specified entry
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        TEntry Create(TEntry entry);

        /// <summary>
        /// Create the specified entries
        /// </summary>
        /// <param name="entries"></param>
        /// <returns></returns>
        IEnumerable<TEntry> Create(IEnumerable<TEntry> entries);

        /// <summary>
        /// Checks if all entries of the repository matches the specified <paramref name="predicate"/>
        /// </summary>
        /// <param name="predicate">The predicate</param>
        /// <param name="cancellationToken"></param>
        /// <returns><see langword="true"/> if all entries matches <paramref name="predicate"/> and <see langword="false"/> otherwise.</returns>
        ValueTask<bool> AllAsync(Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if all entries of the repository satisfy the specified <paramref name="predicate"/>
        /// </summary>
        /// <param name="selector">projection before testing the <paramref name="predicate"/></param>
        /// <param name="predicate">predicate to evaluate all the entries against</param>
        /// <param name="cancellationToken"></param>
        /// <returns><see langword="true"/> if all entries statifies the <paramref name="predicate"/> and <see langword="false"/> otherwise</returns>
        ValueTask<bool> AllAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TResult, bool>> predicate, CancellationToken cancellationToken = default);
    }
}