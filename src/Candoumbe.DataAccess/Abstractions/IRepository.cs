using Candoumbe.DataAccess.Repositories;

namespace Candoumbe.DataAccess.Abstractions
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
        ///     Reads all entries from the repository.
        /// </summary>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="page">Index of the page.</param>
        /// <param name="orderBy">The order by clause to apply.</param>
        /// <param name="ct">Notifies to cancel the execution of the request</param>
        /// <returns><see cref="Page{T}"/> which holds the result</returns>
        public async Task<Page<TEntry>> ReadPageAsync(PageSize pageSize, PageIndex page, IOrder<TEntry> orderBy = null, CancellationToken ct = default)
            => await ReadPageAsync(pageSize, page, Enumerable.Empty<IncludeClause<TEntry>>(), orderBy, ct).ConfigureAwait(false);

        /// <summary>
        ///     Reads all entries from the repository.
        /// </summary>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="page">Index of the page.</param>
        /// <param name="includedProperties">Properties to eagerly include for each entry.</param>
        /// <param name="orderBy">The order by clause to apply.</param>
        /// <param name="ct">Notifies to cancel the execution of the request</param>
        /// <returns><see cref="Page{TEntry}"/> which holds the result</returns>
        Task<Page<TEntry>> ReadPageAsync(PageSize pageSize,
                                              PageIndex page,
                                              IEnumerable<IncludeClause<TEntry>> includedProperties,
                                              IOrder<TEntry> orderBy = null,
                                              CancellationToken ct = default);

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
        Task<Page<TResult>> ReadPageAsync<TResult>(Expression<Func<TEntry, TResult>> selector,
                                                        PageSize pageSize,
                                                        PageIndex page,
                                                        IOrder<TResult> orderBy = null,
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
        Task<Page<TResult>> ReadPageAsync<TResult>(Expression<Func<TEntry, TResult>> selector,
                                                        PageSize pageSize,
                                                        PageIndex page,
                                                        IOrder<TEntry> orderBy = null,
                                                        CancellationToken ct = default);

        /// <summary>
        /// Gets all entries of the repository
        /// </summary>
        /// <param name="ct">Notifies to cancel the execution of the request</param>
        /// <returns><see cref="IEnumerable{T}"/></returns>
        Task<IEnumerable<TEntry>> ReadAllAsync(CancellationToken ct = default);

        /// <summary>
        /// Gets all entries of the repository after applying <paramref name="selector"/>
        /// </summary>
        /// <typeparam name="TResult">Type of the result</typeparam>
        /// <param name="selector">projection to apply before retrieving the result.</param>
        /// <param name="ct">Token to stop query from running</param>
        /// <returns></returns>
        Task<IEnumerable<TResult>> ReadAllAsync<TResult>(Expression<Func<TEntry, TResult>> selector, CancellationToken ct = default);

        /// <summary>
        /// Gets entries of the repository that satisfied the specified <paramref name="predicate"/>
        /// </summary>
        /// <param name="predicate">Filter the entries to retrieve</param>
        /// <param name="ct"></param>
        /// <returns><see cref="IEnumerable{T}"/></returns>
        Task<IEnumerable<TEntry>> WhereAsync(Expression<Func<TEntry, bool>> predicate, CancellationToken ct = default);

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
        /// <remarks>
        /// This method requires the underlying datastore to remain opened.
        /// </remarks>
        IAsyncEnumerable<TResult> Stream<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate, CancellationToken ct = default);

        /// <summary>
        /// Gets entries of the repository that satisfied the specified <paramref name="predicate"/>
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="selector"></param>
        /// <param name="predicate"></param>
        /// <param name="ct">Notifies to cancel the execution of the request</param>
        /// <returns><see cref="IEnumerable{T}"/></returns>
        Task<IEnumerable<TResult>> WhereAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate, CancellationToken ct = default);

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
        Task<IEnumerable<TResult>> WhereAsync<TKey, TResult>(
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
        Task<IEnumerable<TEntry>> WhereAsync(
            Expression<Func<TEntry, bool>> predicate,
            IOrder<TEntry> orderBy = null,
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
        Task<IEnumerable<TResult>> WhereAsync<TResult>(
            Expression<Func<TEntry, TResult>> selector,
            Expression<Func<TEntry, bool>> predicate,
            IOrder<TResult> orderBy = null,
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
        Task<IEnumerable<TResult>> WhereAsync<TResult>(
           Expression<Func<TEntry, TResult>> selector,
           Expression<Func<TResult, bool>> predicate,
           IOrder<TResult> orderBy = null,
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
        Task<Page<TEntry>> WhereAsync(Expression<Func<TEntry, bool>> predicate,
                                           IOrder<TEntry> orderBy,
                                           PageSize pageSize,
                                           PageIndex page,
                                           CancellationToken cancellationToken = default);

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
        ///  or <paramref name="orderBy"/> is <see langword="null"/>.
        /// </exception>
        Task<Page<TResult>> WhereAsync<TResult>(Expression<Func<TEntry, TResult>> selector,
                                                     Expression<Func<TEntry, bool>> predicate,
                                                     IOrder<TResult> orderBy,
                                                     PageSize pageSize,
                                                     PageIndex page,
                                                     CancellationToken cancellationToken = default);

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
        Task<Page<TResult>> WhereAsync<TResult>(
            Expression<Func<TEntry, TResult>> selector,
            Expression<Func<TResult, bool>> predicate,
            IOrder<TResult> orderBy, PageSize pageSize, PageIndex page, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the max value of the selected element
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="selector"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<TResult> MaxAsync<TResult>(Expression<Func<TEntry, TResult>> selector, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the mininum value after applying the <paramref name="selector"/>
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="selector">The projection to make before getting the minimum</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The minimum value</returns>
        Task<TResult> MinAsync<TResult>(Expression<Func<TEntry, TResult>> selector, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if the current repository contains at least one entry
        /// </summary>
        /// <returns>
        ///     <see langword="true"/> if the repository contains at least one element or <see langword="false"/> otherwise
        /// </returns>
        Task<bool> AnyAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if the current repository contains one entry at least that match <paramref name="predicate"/>
        /// </summary>
        /// <param name="predicate">predicate to match</param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        ///     <see langword="true"/> if the repository contains at least one element or <see langword="false"/> otherwise
        /// </returns>
        Task<bool> AnyAsync(Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the number of entries in the repository.
        /// </summary>
        /// <returns>
        ///     the number of entries in the repository
        /// </returns>
        Task<int> CountAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the number of entries in the repository that honor the <paramref name="predicate"/>.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        ///     the number of entries in the repository
        /// </returns>
        Task<int> CountAsync(Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the entry of the repository
        /// </summary>
        /// <returns>
        /// the single entry of the repository
        /// </returns>
        Task<TEntry> SingleAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the single entry that corresponds to the specified <paramref name="predicate"/>.
        /// </summary>
        /// <param name="predicate">Filter to match</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">if no entry or more than one entry matches <paramref name="predicate"/>.</exception>
#if !NETSTANDARD2_0
        public async Task<TEntry> SingleAsync(Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default)
            => await SingleAsync(predicate, Enumerable.Empty<IncludeClause<TEntry>>(), cancellationToken)
                    .ConfigureAwait(false);
#else
        Task<TEntry> SingleAsync(Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default);
#endif

        /// <summary>
        /// Gets the single entry that corresponds to the specified <paramref name="predicate"/>.
        /// </summary>
        /// <param name="predicate">Filter to match</param>
        /// <param name="includedProperties">Properties to eagerly include.</param>
        /// /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">if no entry or more than one entry matches <paramref name="predicate"/>.</exception>
        Task<TEntry> SingleAsync(Expression<Func<TEntry, bool>> predicate,
                                      IEnumerable<IncludeClause<TEntry>> includedProperties,
                                      CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the single entry that matches <paramref name="predicate"/>.
        /// </summary>
        /// <typeparam name="TResult">Type of the result</typeparam>
        /// <param name="selector">selector to convert from <typeparamref name="TEntry"/> to <typeparamref name="TResult"/></param>
        /// <param name="predicate">predicate the entry to match should match.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The entry that matches <paramref name="predicate"/>.</returns>
        /// <exception cref="InvalidOperationException">if no entry or more than one entry matches <paramref name="predicate"/>.</exception>
        /// <exception cref="ArgumentNullException">if either <paramref name="selector"/> or <paramref name="predicate"/> is <see langword="null"/></exception>
        Task<TResult> SingleAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the single <typeparamref name="TEntry"/> of the repository.
        /// Throws <see cref="ArgumentException"/> if there's more than one entry in the repository
        /// </summary>
        /// <returns><see cref="Option.None{T}"/> if there no entry in the repository</returns>
        Task<Option<TEntry>> SingleOrDefaultAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the single <typeparamref name="TEntry"/> of the repository.
        /// Throws <see cref="ArgumentException"/> if there's more than one entry in the repository
        /// </summary>
        /// <param name="includedProperties">Properties to eagerly fetch and load</param>
        /// <param name="cancellationToken"></param>
        /// <returns><see langword="null"/> if there no entry in the repository</returns>
        Task<Option<TEntry>> SingleOrDefaultAsync(IEnumerable<IncludeClause<TEntry>> includedProperties, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the single <typeparamref name="TEntry"/> element of the repository that fullfill the
        /// <paramref name="predicate"/>
        /// </summary>
        /// <param name="predicate">Predicate which should gets one result at most</param>
        /// <param name="cancellationToken"></param>
        /// <returns>the corresponding entry or <see langword="null"/> if no entry found</returns>
        /// <exception cref="InvalidOperationException">if more than one entry matches <paramref name="predicate"/>.</exception>
        Task<Option<TEntry>> SingleOrDefaultAsync(Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the single <typeparamref name="TEntry"/> element of the repository that fullfill the
        /// <paramref name="predicate"/>
        /// </summary>
        /// <param name="predicate">Predicate which should gets one result at most</param>
        /// <param name="includedProperties">Properties to eagerly include.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>the corresponding entry or <see langword="null"/> if no entry found</returns>
        /// <exception cref="InvalidOperationException">if more than one entry matches <paramref name="predicate"/>.</exception>
        /// <exception cref="ArgumentNullException">if either <paramref name="predicate"/> or <paramref name="includedProperties"/> is <see langword="null"/></exception>
        Task<Option<TEntry>> SingleOrDefaultAsync(Expression<Func<TEntry, bool>> predicate, IEnumerable<IncludeClause<TEntry>> includedProperties, CancellationToken cancellationToken = default);

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
        /// <returns>The entry that matches <paramref name="predicate"/> or <see langword="null"/> if no matches found</returns>
        /// <exception cref="InvalidOperationException">if no entry or more than one entry matches <paramref name="predicate"/>.</exception>
        /// <exception cref="ArgumentNullException">if either <paramref name="selector"/> or <paramref name="predicate"/> is <see langword="null"/></exception>
        Task<Option<TResult>> SingleOrDefaultAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default);

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
        /// <returns>The entry that matches <paramref name="predicate"/> or <see langword="null"/> if no matches found</returns>
        /// <exception cref="InvalidOperationException">if no entry or more than one entry matches <paramref name="predicate"/>.</exception>
        /// <exception cref="ArgumentNullException">if either <paramref name="selector"/> or <paramref name="predicate"/> is <see langword="null"/></exception>
        Task<Option<TResult>> SingleOrDefaultAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TResult, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the first entry of the repository
        /// </summary>
        /// <returns>The first entry of the repository</returns>
        /// <exception cref="InvalidOperationException">if no entry found.</exception>
        Task<TEntry> FirstAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the first entry of the repository
        /// </summary>
        /// <param name="includedProperties">Properties to eagerly load</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The first entry of the repository</returns>
        /// <exception cref="InvalidOperationException">if no entry found.</exception>
        Task<TEntry> FirstAsync(IEnumerable<IncludeClause<TEntry>> includedProperties, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the first entry of the repository
        /// </summary>
        /// <returns>The first entry or <see langword="null"/> if there's no entry.</returns>
#if !NETSTANDARD2_0
        public async Task<Option<TEntry>> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
            => await FirstOrDefaultAsync(Enumerable.Empty<IncludeClause<TEntry>>(), cancellationToken).ConfigureAwait(false);
#else
        Task<Option<TEntry>> FirstOrDefaultAsync(CancellationToken cancellationToken = default);
#endif
        /// <summary>
        /// Gets the first entry of the repository
        /// </summary>
        /// <param name="includedProperties">properties to eagerly include</param>
        /// <param name="cancellation"></param>
        /// <returns>The first entry of the repository or <see langword="null"/> is there's no entry</returns>
        Task<Option<TEntry>> FirstOrDefaultAsync(IEnumerable<IncludeClause<TEntry>> includedProperties, CancellationToken cancellation = default);

        /// <summary>
        /// Gets the first entry of the repository that fullfill the specified <paramref name="predicate"/>
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>The first entry that mat</returns>
        /// <exception cref="InvalidOperationException">if no entry matches <paramref name="predicate"/>.</exception>
        /// <exception cref="ArgumentNullException">if <paramref name="predicate"/> is <see langword="null"/></exception>
#if !NETSTANDARD2_0
        public async Task<TEntry> FirstAsync(Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default)
           => await FirstAsync(predicate, Enumerable.Empty<IncludeClause<TEntry>>(), cancellationToken).ConfigureAwait(false);
#else
        Task<TEntry> FirstAsync(Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default);
#endif

        /// <summary>
        /// Gets the first entry of the repository that fullfill the specified <paramref name="predicate"/>
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="includedProperties">The properties to eagerly load.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The first entry that mat</returns>
        /// <exception cref="InvalidOperationException">if no entry matches <paramref name="predicate"/>.</exception>
        /// <exception cref="ArgumentNullException">if <paramref name="predicate"/> is <see langword="null"/></exception>
        Task<TEntry> FirstAsync(Expression<Func<TEntry, bool>> predicate, IEnumerable<IncludeClause<TEntry>> includedProperties, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the first entry of the repository that fullfill the specified <paramref name="predicate"/>
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>The first entry that mat</returns>
        /// <exception cref="InvalidOperationException">if no entry matches <paramref name="predicate"/>.</exception>
        /// <exception cref="ArgumentNullException">if <paramref name="predicate"/> is <see langword="null"/></exception>
#if !NETSTANDARD2_0
        async Task<Option<TEntry>> FirstOrDefaultAsync(Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default)
            => await FirstOrDefaultAsync(predicate, Enumerable.Empty<IncludeClause<TEntry>>(), cancellationToken).ConfigureAwait(false);
#else
        Task<Option<TEntry>> FirstOrDefaultAsync(Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default);
#endif
        /// <summary>
        /// Gets the first entry of the repository that fullfill the specified <paramref name="predicate"/>
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="includedProperties">The properties to eagerly load.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The first entry that mat</returns>
        /// <exception cref="InvalidOperationException">if no entry matches <paramref name="predicate"/>.</exception>
        /// <exception cref="ArgumentNullException">if <paramref name="predicate"/> is <see langword="null"/></exception>
        Task<Option<TEntry>> FirstOrDefaultAsync(Expression<Func<TEntry, bool>> predicate, IEnumerable<IncludeClause<TEntry>> includedProperties, CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete all entries that match <paramref name="predicate"/>
        /// </summary>
        /// <param name="predicate">Defines which <typeparamref name="TEntry"/> will be deleted.</param>
        /// <param name="ct">Notifies to cancel the execution of the request.</param>
        /// <exception cref="ArgumentNullException">if <paramref name="predicate"/> is <c>nuull</c></exception>
        Task Delete(Expression<Func<TEntry, bool>> predicate, CancellationToken ct = default);

        /// <summary>
        /// Delete all entries of the underlying datastore
        /// </summary>
        Task Clear(CancellationToken ct = default);

        /// <summary>
        /// Gets the first entry that matches <paramref name="predicate"/>
        /// </summary>
        /// <typeparam name="TResult">Type of the result</typeparam>
        /// <param name="selector">Projection to apply to the entry found</param>
        /// <param name="predicate">Filter to match</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The entry that matches <paramref name="predicate"/>.</returns>
        /// <exception cref="InvalidOperationException">if the repository is empty or no entry matches <paramref name="predicate"/></exception>
        Task<TResult> FirstAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the first entry that matches <paramref name="predicate"/>
        /// </summary>
        /// <typeparam name="TResult">Type of the result</typeparam>
        /// <param name="selector">Projection to apply to the entry found</param>
        /// <param name="predicate">Filter to match</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The entry that matches <paramref name="predicate"/> or <see langword="null"/> if no entry found.</returns>
        Task<Option<TResult>> FirstOrDefaultAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default);

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
        Task<bool> AllAsync(Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if all entries of the repository satisfy the specified <paramref name="predicate"/>
        /// </summary>
        /// <param name="selector">projection before testing the <paramref name="predicate"/></param>
        /// <param name="predicate">predicate to evaluate all the entries against</param>
        /// <param name="cancellationToken"></param>
        /// <returns><see langword="true"/> if all entries statifies the <paramref name="predicate"/> and <see langword="false"/> otherwise</returns>
        Task<bool> AllAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TResult, bool>> predicate, CancellationToken cancellationToken = default);
    }
}