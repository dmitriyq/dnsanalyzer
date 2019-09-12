using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Dns.DAL
{
	public class DnsReadOnlyDbContext : DnsDbContext
	{
		private const string READ_ONLY_ERROR = "This context is read-only.";

		public DnsReadOnlyDbContext()
		{
		}

		public DnsReadOnlyDbContext(DbContextOptions<DnsDbContext> options) : base(options)
		{
			ChangeTracker.AutoDetectChangesEnabled = false;
			ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
		}

		public override int SaveChanges()
			=> throw new InvalidOperationException(READ_ONLY_ERROR);

		public override int SaveChanges(bool acceptAllChangesOnSuccess)
			=> throw new InvalidOperationException(READ_ONLY_ERROR);

		public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
			=> throw new InvalidOperationException(READ_ONLY_ERROR);

		public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
			=> throw new InvalidOperationException(READ_ONLY_ERROR);

		public override EntityEntry Add(object entity)
			=> throw new InvalidOperationException(READ_ONLY_ERROR);

		public override EntityEntry<TEntity> Add<TEntity>(TEntity entity)
			=> throw new InvalidOperationException(READ_ONLY_ERROR);

		public override ValueTask<EntityEntry> AddAsync(object entity, CancellationToken cancellationToken = default)
			=> throw new InvalidOperationException(READ_ONLY_ERROR);

		public override ValueTask<EntityEntry<TEntity>> AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
			=> throw new InvalidOperationException(READ_ONLY_ERROR);

		public override void AddRange(IEnumerable<object> entities)
			=> throw new InvalidOperationException(READ_ONLY_ERROR);

		public override void AddRange(params object[] entities)
			=> throw new InvalidOperationException(READ_ONLY_ERROR);

		public override Task AddRangeAsync(IEnumerable<object> entities, CancellationToken cancellationToken = default)
			=> throw new InvalidOperationException(READ_ONLY_ERROR);

		public override Task AddRangeAsync(params object[] entities)
			=> throw new InvalidOperationException(READ_ONLY_ERROR);

		public override EntityEntry Attach(object entity)
			=> throw new InvalidOperationException(READ_ONLY_ERROR);

		public override EntityEntry<TEntity> Attach<TEntity>(TEntity entity)
			=> throw new InvalidOperationException(READ_ONLY_ERROR);

		public override void AttachRange(IEnumerable<object> entities)
			=> throw new InvalidOperationException(READ_ONLY_ERROR);

		public override void AttachRange(params object[] entities)
			=> throw new InvalidOperationException(READ_ONLY_ERROR);

		public override EntityEntry Remove(object entity)
			=> throw new InvalidOperationException(READ_ONLY_ERROR);

		public override EntityEntry<TEntity> Remove<TEntity>(TEntity entity)
			=> throw new InvalidOperationException(READ_ONLY_ERROR);

		public override void RemoveRange(IEnumerable<object> entities)
			=> throw new InvalidOperationException(READ_ONLY_ERROR);

		public override void RemoveRange(params object[] entities)
			=> throw new InvalidOperationException(READ_ONLY_ERROR);

		public override EntityEntry Update(object entity)
			=> throw new InvalidOperationException(READ_ONLY_ERROR);

		public override EntityEntry<TEntity> Update<TEntity>(TEntity entity)
			=> throw new InvalidOperationException(READ_ONLY_ERROR);

		public override void UpdateRange(IEnumerable<object> entities)
			=> throw new InvalidOperationException(READ_ONLY_ERROR);

		public override void UpdateRange(params object[] entities)
			=> throw new InvalidOperationException(READ_ONLY_ERROR);
	}
}