using Microsoft.EntityFrameworkCore;

namespace ClientManager.Shared.Data;

public class ReadOnlyAppDbContext : AppDbContext
{
    public ReadOnlyAppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public override int SaveChanges() => throw new InvalidOperationException("This context is read-only.");

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => throw new InvalidOperationException("This context is read-only.");
}
