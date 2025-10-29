using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ClientManager.Shared.Data;

public class ReadOnlyAppDbContext : AppDbContext
{
    public ReadOnlyAppDbContext(DbContextOptions<ReadOnlyAppDbContext> options)
        : base(options) { }

    public override int SaveChanges() => throw new InvalidOperationException("This context is read-only.");

    public override int SaveChanges(bool acceptAllChangesOnSuccess) => throw new InvalidOperationException("This context is read-only.");

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => throw new InvalidOperationException("This context is read-only.");

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("This context is read-only.");

    public override EntityEntry<TEntity> Add<TEntity>(TEntity entity) => throw new InvalidOperationException("Cannot call 'Update' — this DbContext is read-only.");

    public override ValueTask<EntityEntry<TEntity>> AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("Cannot call 'Update' — this DbContext is read-only.");

    public override EntityEntry<TEntity> Update<TEntity>(TEntity entity) => throw new InvalidOperationException("Cannot call 'Update' — this DbContext is read-only.");

    public override EntityEntry<TEntity> Remove<TEntity>(TEntity entity) => throw new InvalidOperationException("Cannot call 'Update' — this DbContext is read-only.");
}
