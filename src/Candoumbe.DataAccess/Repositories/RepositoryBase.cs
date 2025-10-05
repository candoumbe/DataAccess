using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Candoumbe.DataAccess.Abstractions;
using Microsoft.EntityFrameworkCore;
using Ultimately;

namespace Candoumbe.DataAccess.Repositories;

/// <summary>
/// Repository base class
/// </summary>
/// <typeparam name="TEntry">Type of entities the repository will manage.</typeparam>
public abstract class RepositoryBase<TEntry> : IRepository<TEntry>
    where TEntry : class
{

    /// <summary>
    /// <see cref="IStore"/> which the current instance operates on.
    /// </summary>
    protected IStore Context { get; }

    /// <summary>
    /// Builds a new <see cref="RepositoryBase{TEntry}"/> that handles <typeparamref name="TEntry"/>
    /// </summary>
    /// <param name="context"></param>
    /// <exception cref="ArgumentNullException">if <paramref name="context"/> is <see langword="null"/></exception>
    protected RepositoryBase(IStore context)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc/>
    public virtual async Task Clear(CancellationToken cancellationToken = default) => await Delete(AlwaysTrueSpecification<TEntry>.Instance, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    public virtual async Task<Page<TEntry>> ReadPage(PageSize pageSize,
                                                     PageIndex pageIndex,
                                                     IOrderSpecification<TEntry> orderBy,
                                                     CancellationToken cancellationToken = default)
        => await ReadPage(pageSize, pageIndex, [], orderBy, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    public virtual async Task<Page<TEntry>> ReadPage(PageSize pageSize,
                                                     PageIndex pageIndex,
                                                     IEnumerable<IncludeClause<TEntry>> includedProperties,
                                                     IOrderSpecification<TEntry> orderBy,
                                                     CancellationToken cancellationToken = default)
    {
        IContainer<TEntry> entries = Context.Set<TEntry>();
        int total = await entries.CountAsync(cancellationToken)
                        .ConfigureAwait(false);
        Page<TEntry> pageOfResult = Page<TEntry>.Empty(pageSize);
        if (total > 0)
        {
            IReadOnlyList<TEntry> results =await entries.Include(includedProperties)
                                               .OrderBy(orderBy)
                                               .Skip(ComputeSkipCount(pageIndex, pageSize))
                                               .Take(pageSize)
                                               .ToArrayAsync(cancellationToken)
                                               .ConfigureAwait(false);
            pageOfResult = new Page<TEntry>(results, total, pageSize);
        }

        return pageOfResult;
    }

    private static int ComputeSkipCount(PageIndex index,
                                        PageSize pageSize) => index == 1 ? 0 : ( index - 1 ) * pageSize;

    /// <inheritdoc/>
    public virtual async Task<Page<TResult>> ReadPage<TResult>(IProjectionSpecification<TEntry, TResult> selector,
                                                               PageSize pageSize,
                                                               PageIndex page,
                                                               IOrderSpecification<TResult> orderBy,
                                                               CancellationToken cancellationToken = default)
    {
        IContainer<TEntry> entries = Context.Set<TEntry>();
        int total = await entries.CountAsync(cancellationToken)
                        .ConfigureAwait(false);

        Page<TResult> pageOfResult = Page<TResult>.Empty(pageSize);

        if (total > 0)
        {
            IReadOnlyList<TResult> results = await entries.Select(selector)
                                                 .OrderBy(orderBy)
                                                 .Skip(ComputeSkipCount(page, pageSize))
                                                 .Take(pageSize)
                                                 .ToArrayAsync(cancellationToken)
                                                 .ConfigureAwait(false);
            pageOfResult = new Page<TResult>(results, total, pageSize);
        }

        return pageOfResult;
    }

    /// <inheritdoc/>
    public virtual async Task<Page<TResult>> ReadPage<TResult>(IProjectionSpecification<TEntry, TResult> selector,
                                                               PageSize pageSize,
                                                               PageIndex page,
                                                               IOrderSpecification<TEntry> orderBy,
                                                               CancellationToken cancellationToken = default)
    {
        IContainer<TEntry> entries = Context.Set<TEntry>();
        IQueryable<TEntry> resultQuery = entries;

        if (orderBy is not null)
        {
            resultQuery = resultQuery.OrderBy(orderBy);
        }

        long total = await entries.CountAsync(cancellationToken).ConfigureAwait(false);

        Page<TResult> pageResult;

        if (total > 0)
        {
            IReadOnlyList<TResult> results = await resultQuery
                                                 .Skip(ComputeSkipCount(page, pageSize))
                                                 .Take(pageSize)
                                                 .Select(selector)
                                                 .ToArrayAsync(cancellationToken)
                                                 .ConfigureAwait(false);

            pageResult = new Page<TResult>(results, total, pageSize);
        }
        else
        {
            pageResult = Page<TResult>.Empty(pageSize);
        }
        return pageResult;
    }

    /// <inheritdoc/>
    public virtual async Task<IEnumerable<TResult>> ReadAll<TResult>(IProjectionSpecification<TEntry, TResult> selector,
                                                                     CancellationToken cancellationToken = default)
        => await Context.Set<TEntry>().Select(selector)
               .ToArrayAsync(cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    public virtual async Task<IEnumerable<TEntry>> ReadAll(CancellationToken cancellationToken = default)
        => await ReadAll(IdentityProjectionSpecification<TEntry>.Instance, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    public virtual async Task<IEnumerable<TResult>> Where<TResult>(IProjectionSpecification<TEntry, TResult> selector,
                                                                   IFilterSpecification<TEntry> predicate,
                                                                   CancellationToken cancellationToken = default)
        => await Context.Set<TEntry>()
               .Where(predicate.Filter)
               .Select(selector)
               .ToArrayAsync(cancellationToken)
               .ConfigureAwait(false);

    /// <inheritdoc/>
    public virtual async Task<IEnumerable<TResult>> Where<TKey, TResult>(
        IFilterSpecification<TEntry> predicate,
        Expression<Func<TEntry, TKey>> keySelector,
        Expression<Func<IGrouping<TKey, TEntry>, TResult>> groupSelector,
        CancellationToken cancellationToken = default)
    {
        return predicate is null
                   ? throw new ArgumentNullException(nameof(predicate))
                   : keySelector is null
                       ? throw new ArgumentNullException(nameof(keySelector))
                       : groupSelector is null
                           ? throw new ArgumentNullException(nameof(groupSelector))
                           : (IEnumerable<TResult>)await Context.Set<TEntry>()
                                                       .Where(predicate.Filter)
                                                       .GroupBy(keySelector)
                                                       .Select(groupSelector)
                                                       .ToArrayAsync(cancellationToken)
                                                       .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public virtual async Task<IEnumerable<TEntry>> Where(IFilterSpecification<TEntry> predicate,
                                                         CancellationToken cancellationToken = default)
        => await Where(IdentityProjectionSpecification<TEntry>.Instance, predicate, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    public virtual async Task<IEnumerable<TEntry>> Where(IFilterSpecification<TEntry> predicate,
                                                         IOrderSpecification<TEntry> orderBy = null,
                                                         IEnumerable<IncludeClause<TEntry>> includedProperties = null,
                                                         CancellationToken cancellationToken = default)
        => await Where(IdentityProjectionSpecification<TEntry>.Instance, predicate, orderBy, includedProperties, cancellationToken)
               .ConfigureAwait(false);

    /// <inheritdoc/>
    public virtual async Task<IEnumerable<TResult>> Where<TResult>(IProjectionSpecification<TEntry, TResult> selector,
                                                                   IFilterSpecification<TEntry> predicate,
                                                                   IOrderSpecification<TResult> orderBy = null,
                                                                   IEnumerable<IncludeClause<TEntry>> includedProperties = null,
                                                                   CancellationToken cancellationToken = default)
        => await Context.Set<TEntry>()
               .Where(predicate.Filter)
               .Include(includedProperties)
               .Select(selector)
               .OrderBy(orderBy)
               .ToArrayAsync(cancellationToken)
               .ConfigureAwait(false);

    /// <inheritdoc/>
    public virtual async Task<IEnumerable<TResult>> Where<TResult>(IProjectionSpecification<TEntry, TResult> selector,
                                                                   IFilterSpecification<TResult> predicate,
                                                                   IOrderSpecification<TResult> orderBy = null,
                                                                   CancellationToken cancellationToken = default)
        => await Context.Set<TEntry>()
               .Select(selector)
               .Where(predicate.Filter)
               .OrderBy(orderBy)
               .ToArrayAsync(cancellationToken)
               .ConfigureAwait(false);

    /// <inheritdoc/>
    public virtual async Task<Page<TEntry>> Where(IFilterSpecification<TEntry> predicate,
                                                  IOrderSpecification<TEntry> orderBy,
                                                  PageSize pageSize,
                                                  PageIndex page,
                                                  CancellationToken cancellationToken = default)
    {
        if (orderBy is null)
        {
            throw new ArgumentNullException(nameof(orderBy), $"{nameof(orderBy)} expression must be set");
        }
        IQueryable<TEntry> query = Context.Set<TEntry>()
            .Where(predicate.Filter)
            .OrderBy(orderBy)
            .Skip(ComputeSkipCount(page, pageSize))
            .Take(pageSize);

        IReadOnlyList<TEntry> result = await query.ToListAsync(cancellationToken).ConfigureAwait(false);
        int total = await Count(predicate, cancellationToken).ConfigureAwait(false);

        return new Page<TEntry>(result, total, pageSize);
    }

    /// <inheritdoc/>
    public virtual async Task<Page<TResult>> Where<TResult>(
        IProjectionSpecification<TEntry, TResult> selector,
        IFilterSpecification<TEntry> predicate,
        IOrderSpecification<TResult> orderBy,
        PageSize pageSize,
        PageIndex pageIndex,
        CancellationToken cancellationToken = default)
    {
        if (orderBy is null)
        {
            throw new ArgumentNullException(nameof(orderBy), $"{nameof(orderBy)} expression must be set");
        }
        IQueryable<TResult> query = Context.Set<TEntry>()
            .Where(predicate.Filter)
            .Select(selector)
            .OrderBy(orderBy);

        IQueryable<TResult> results = query.Skip(ComputeSkipCount(pageIndex, pageSize))
            .Take(pageSize);

        //we compute both values
        IReadOnlyList<TResult> result = await results.ToArrayAsync(cancellationToken).ConfigureAwait(false);
        int total = await Count(predicate, cancellationToken).ConfigureAwait(false);

        return new Page<TResult>(result, total, pageSize);
    }

    /// <inheritdoc/>
    public virtual async Task<Page<TResult>> Where<TResult>(IProjectionSpecification<TEntry, TResult> selector,
                                                            IFilterSpecification<TResult> predicate,
                                                            IOrderSpecification<TResult> orderBy,
                                                            PageSize pageSize,
                                                            PageIndex pageIndex,
                                                            CancellationToken cancellationToken = default)
    {
        if (orderBy is null)
        {
            throw new ArgumentNullException(nameof(orderBy), $"{nameof(orderBy)} expression must be set");
        }
        IContainer<TEntry> entries = Context.Set<TEntry>();
        IQueryable<TResult> query = entries.Select(selector)
            .Where(predicate.Filter)
            .OrderBy(orderBy)
            .Skip(ComputeSkipCount(pageIndex, pageSize))
            .Take(pageSize);

        // we compute both Task
        IReadOnlyList<TResult> result = await query.ToListAsync(cancellationToken)
                                            .ConfigureAwait(false);
        long total = await entries.Select(selector).LongCountAsync(predicate.Filter, cancellationToken)
                         .ConfigureAwait(false);

        return total == 0
                   ? Page<TResult>.Empty(pageSize)
                   : new Page<TResult>(result, total, pageSize);
    }

    /// <inheritdoc/>
    public virtual async Task<bool> Any(CancellationToken cancellationToken = default)
        => await Context.Set<TEntry>().AnyAsync(cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    public virtual async Task<TResult> Max<TResult>(IProjectionSpecification<TEntry, TResult> selector,
                                                    CancellationToken cancellationToken = default)
        => await Context.Set<TEntry>().MaxAsync(selector.Expression, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    public virtual async Task<TResult> Min<TResult>(IProjectionSpecification<TEntry, TResult> selector,
                                                    CancellationToken cancellationToken = default)
        => await Context.Set<TEntry>().MinAsync(selector.Expression, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    public virtual async Task<bool> Any(IFilterSpecification<TEntry> predicate,
                                        CancellationToken cancellationToken = default)
        => await Context.Set<TEntry>().AnyAsync(predicate.Filter, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    public virtual async Task<int> Count(CancellationToken cancellationToken = default)
        => await Context.Set<TEntry>().CountAsync(cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    public virtual async Task<int> Count(IFilterSpecification<TEntry> predicate,
                                         CancellationToken cancellationToken = default)
        => await Context.Set<TEntry>().CountAsync(predicate.Filter, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    public virtual async Task<TEntry> Single(CancellationToken cancellationToken = default)
        => await Context.Set<TEntry>().SingleAsync(cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    public virtual async Task<TEntry> Single(IFilterSpecification<TEntry> predicate,
                                             CancellationToken cancellationToken = default)
        => await Context.Set<TEntry>().SingleAsync(predicate.Filter, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    public virtual async Task<TEntry> Single(IFilterSpecification<TEntry> predicate,
                                             IEnumerable<IncludeClause<TEntry>> includedProperties,
                                             CancellationToken cancellationToken = default)
        => await Context.Set<TEntry>().Include(includedProperties).SingleAsync(predicate.Filter, cancellationToken)
               .ConfigureAwait(false);

    /// <inheritdoc/>
    public virtual async Task<TResult> Single<TResult>(IProjectionSpecification<TEntry, TResult> selector,
                                                       IFilterSpecification<TEntry> predicate,
                                                       CancellationToken cancellationToken = default)
        => await Context.Set<TEntry>().Where(predicate.Filter).Select(selector)
               .SingleAsync(cancellationToken)
               .ConfigureAwait(false);

    /// <inheritdoc/>
    public virtual async Task<Option<TEntry>> SingleOrDefault(CancellationToken cancellationToken = default)
        => (await Context.Set<TEntry>().SingleOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false))
            .NoneWhen(result => result is null, "No entry found");

    /// <inheritdoc/>
    public virtual async Task<Option<TEntry>> SingleOrDefault(IEnumerable<IncludeClause<TEntry>> includedProperties,
                                                              CancellationToken cancellationToken = default)
        => (await Context.Set<TEntry>()
                .Include(includedProperties)
                .SingleOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false))
            .NoneWhen(result => result is null, "No entry found");

    /// <inheritdoc/>
    public virtual async Task<Option<TEntry>> SingleOrDefault(IFilterSpecification<TEntry> predicate,
                                                              CancellationToken cancellationToken = default)
        => (await Context.Set<TEntry>().SingleOrDefaultAsync(predicate.Filter, cancellationToken)
                .ConfigureAwait(false))
            .NoneWhen(result => result is null, "No entry found");

    /// <inheritdoc/>
    public virtual async Task<Option<TEntry>> SingleOrDefault(IFilterSpecification<TEntry> predicate,
                                                              IEnumerable<IncludeClause<TEntry>> includedProperties,
                                                              CancellationToken cancellationToken = default)
        => (await Context.Set<TEntry>()
                .Include(includedProperties)
                .SingleOrDefaultAsync(predicate.Filter, cancellationToken)
                .ConfigureAwait(false))
            .NoneWhen(result => result is null, "No entry found");

    /// <inheritdoc/>
    public virtual async Task<Option<TResult>> SingleOrDefault<TResult>(IProjectionSpecification<TEntry, TResult> selector,
                                                                        IFilterSpecification<TEntry> predicate,
                                                                        CancellationToken cancellationToken = default)
        => (await Context.Set<TEntry>().Where(predicate.Filter)
                .Select(selector)
                .SingleOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false))
            .NoneWhen(result => result is null, "No entry found");

    /// <inheritdoc/>
    public virtual async Task<Option<TResult>> SingleOrDefault<TResult>(IProjectionSpecification<TEntry, TResult> selector,
                                                                        IFilterSpecification<TResult> predicate,
                                                                        CancellationToken cancellationToken = default)
        => (await Context.Set<TEntry>()
                .Select(selector)
                .SingleOrDefaultAsync(predicate.Filter, cancellationToken)
                .ConfigureAwait(false))
            .NoneWhen(result => result is null, "No entry found");

    /// <inheritdoc/>
    public virtual async Task<TEntry> First(CancellationToken cancellationToken = default)
        => await First(AlwaysTrueSpecification<TEntry>.Instance, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    public virtual async Task<TEntry> First(IFilterSpecification<TEntry> predicate,
                                            CancellationToken cancellationToken = default)
        => await First(predicate, [], cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    public virtual async Task<TEntry> First(IEnumerable<IncludeClause<TEntry>> includedProperties,
                                            CancellationToken cancellationToken = default)
        => await Context.Set<TEntry>().Include(includedProperties).FirstAsync(cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    public virtual async Task<TEntry> First(IFilterSpecification<TEntry> predicate,
                                            IEnumerable<IncludeClause<TEntry>> includedProperties,
                                            CancellationToken cancellationToken = default)
        => await Context.Set<TEntry>().Include(includedProperties)
               .FirstAsync(predicate.Filter, cancellationToken)
               .ConfigureAwait(false);
    /// <inheritdoc/>
    public virtual async Task<Option<TEntry>> FirstOrDefault(IEnumerable<IncludeClause<TEntry>> includedProperties,
                                                             CancellationToken cancellation = default)
        => (await Context.Set<TEntry>().Include(includedProperties)
                .FirstOrDefaultAsync(cancellation)
                .ConfigureAwait(false))
            .NoneWhen(result => result is null, "No entry found");

    /// <inheritdoc/>
    public virtual async Task<Option<TEntry>> FirstOrDefault(CancellationToken cancellationToken = default)
        => await FirstOrDefault([], cancellationToken).ConfigureAwait(false);
    /// <inheritdoc/>
    public virtual async Task<Option<TEntry>> FirstOrDefault(IFilterSpecification<TEntry> predicate,
                                                             CancellationToken cancellationToken = default)
        => (await Context.Set<TEntry>().FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false))
            .NoneWhen(result => result is null, "No entry found");
    /// <inheritdoc/>
    public virtual async Task<TResult> First<TResult>(IProjectionSpecification<TEntry, TResult> selector,
                                                      IFilterSpecification<TEntry> predicate,
                                                      CancellationToken cancellationToken = default)
        => await Context.Set<TEntry>()
               .Where(predicate.Filter)
               .Select(selector)
               .FirstAsync(cancellationToken)
               .ConfigureAwait(false);

    /// <inheritdoc/>
    public virtual async Task<Option<TResult>> FirstOrDefault<TResult>(IProjectionSpecification<TEntry, TResult> selector,
                                                                       IFilterSpecification<TEntry> predicate,
                                                                       CancellationToken cancellationToken = default)
        => (await Context.Set<TEntry>()
                .Where(predicate.Filter)
                .Select(selector)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false))
            .NoneWhen(result => result is null, "No entry found");

    /// <inheritdoc/>
    public virtual async Task<Option<TEntry>> FirstOrDefault(IFilterSpecification<TEntry> predicate,
                                                             IEnumerable<IncludeClause<TEntry>> includedProperties,
                                                             CancellationToken cancellationToken = default)
        => (await Context.Set<TEntry>().Include(includedProperties)
                .FirstOrDefaultAsync(predicate.Filter, cancellationToken)
                .ConfigureAwait(false))
            .NoneWhen(result => result is null, "No entry found");

    /// <inheritdoc/>
    public abstract Task<TEntry> Create(TEntry entry,
                                        CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public abstract Task Delete(IFilterSpecification<TEntry> predicate,
                                CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public abstract Task<IEnumerable<TEntry>> Create(IEnumerable<TEntry> entries,
                                                     CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public IAsyncEnumerable<TEntry> Stream(IFilterSpecification<TEntry> predicate,
                                           CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Context.Set<TEntry>()
            .Where(predicate.Filter)
            .AsAsyncEnumerable();
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<TResult> Stream<TResult>(IProjectionSpecification<TEntry, TResult> selector,
                                                     IFilterSpecification<TResult> predicate,
                                                     CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Context.Set<TEntry>()
            .Select(selector)
            .Where(predicate.Filter)
            .AsAsyncEnumerable();
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<TResult> Stream<TResult>(IProjectionSpecification<TEntry, TResult> selector,
                                                     IFilterSpecification<TResult> predicate,
                                                     IOrderSpecification<TResult> orderBy,
                                                     CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Context.Set<TEntry>()
            .Select(selector)
            .Where(predicate.Filter)
            .OrderBy(orderBy)
            .AsAsyncEnumerable();
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<TResult> Stream<TResult>(IProjectionSpecification<TEntry, TResult> selector,
                                                     IFilterSpecification<TEntry> predicate,
                                                     IOrderSpecification<TResult> orderBy,
                                                     CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Context.Set<TEntry>()
            .Where(predicate.Filter)
            .Select(selector)
            .OrderBy(orderBy)
            .AsAsyncEnumerable();
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<TResult> Stream<TResult>(IProjectionSpecification<TEntry, TResult> selector,
                                                     IFilterSpecification<TEntry> predicate,
                                                     CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Context.Set<TEntry>()
            .Where(predicate.Filter)
            .Select(selector)
            .AsAsyncEnumerable();
    }

    /// <inheritdoc/>
    public virtual async Task<bool> All(IFilterSpecification<TEntry> predicate,
                                        CancellationToken cancellationToken = default)
        => await Context.Set<TEntry>()
               .AllAsync(predicate.Filter, cancellationToken)
               .ConfigureAwait(false);

    /// <inheritdoc/>
    public virtual async Task<bool> All<TResult>(IProjectionSpecification<TEntry, TResult> selector,
                                                 IFilterSpecification<TResult> predicate,
                                                 CancellationToken cancellationToken = default)
        => await Context.Set<TEntry>()
               .Select(selector)
               .AllAsync(predicate.Filter, cancellationToken)
               .ConfigureAwait(false);
}