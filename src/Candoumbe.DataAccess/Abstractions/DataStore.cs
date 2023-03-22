namespace Candoumbe.DataAccess.Abstractions
{
    using Candoumbe.DataAccess.Abstractions.Entities;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.ChangeTracking;
    using Microsoft.EntityFrameworkCore.Metadata;

    using NodaTime;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Base class for creating a datastore specialized class.
    /// </summary>
    /// <typeparam name="TContext">Type of the datastore that will be created.</typeparam>
    public abstract class DataStore<TContext> : DbContext, IDbContext where TContext : DbContext
    {
        /// <summary>
        /// Usual size for the "normal" text
        /// </summary>
        public const int NormalTextLength = 255;

        /// <summary>
        /// Usual size for "short" text
        /// </summary>
        public const int ShortTextLength = 50;
        private readonly IClock _clock;

        /// <summary>
        /// Builds a new <see cref="DataStore{TContext}"/> instance.
        /// </summary>
        /// <param name="options">Options used by the datastore</param>
        /// <param name="clock"><see cref="IClock"/> instance used to access current time.</param>
        protected DataStore(DbContextOptions<TContext> options, IClock clock) : base(options)
        {
            _clock = clock;
        }

        ///<inheritdoc/>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            foreach (IMutableEntityType entity in builder.Model.GetEntityTypes())
            {
                if (typeof(IAuditableEntity).IsAssignableFrom(entity.ClrType))
                {
                    builder.Entity(entity.Name).Property(typeof(string), nameof(IAuditableEntity.CreatedBy))
                        .HasMaxLength(NormalTextLength);

                    builder.Entity(entity.Name).Property(typeof(string), nameof(IAuditableEntity.UpdatedBy))
                        .HasMaxLength(NormalTextLength);

                    builder.Entity(entity.Name).Property(typeof(Instant?), nameof(IAuditableEntity.UpdatedDate))
                           .IsConcurrencyToken();
                }

                if (entity.ClrType.IsAssignableToGenericType(typeof(IEntity<>)))
                {
                    builder.Entity(entity.Name)
                                .HasKey(nameof(IEntity<object>.Id));
                }
            }
        }

        private IEnumerable<EntityEntry> GetModifiedEntities() => ChangeTracker.Entries()
            .AsParallel()
            .Where(x => (x.Entity is IAuditableEntity)
            && (x.State == EntityState.Added
                || x.State == EntityState.Modified))
#if DEBUG
            .ToArray()
#endif
            ;

        private Action<EntityEntry> UpdateModifiedEntry
            => x =>
            {
                IAuditableEntity auditableEntity = (IAuditableEntity)x.Entity;
                Instant now = _clock.GetCurrentInstant();
                if (x.State == EntityState.Added)
                {
                    auditableEntity.CreatedDate = now;
                    auditableEntity.UpdatedDate = now;
                }
                else if (x.State == EntityState.Modified)
                {
                    auditableEntity.UpdatedDate = now;
                }
            };

        ///<inheritdoc/>
        public override int SaveChanges() => SaveChanges(true);

        ///<inheritdoc/>
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            IEnumerable<EntityEntry> entities = GetModifiedEntities();
            entities
                .AsParallel()
                .ForEach(UpdateModifiedEntry);
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        ///<inheritdoc/>
        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken ct = default)
        {
            IEnumerable<EntityEntry> entities = GetModifiedEntities();

            entities.ForEach(UpdateModifiedEntry);

            return await base.SaveChangesAsync(true, ct)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// <see cref="DbContext.SaveChangesAsync(CancellationToken)"/>
        /// </summary>
        public override async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
            await SaveChangesAsync(true, ct)
                .ConfigureAwait(false);
    }
}