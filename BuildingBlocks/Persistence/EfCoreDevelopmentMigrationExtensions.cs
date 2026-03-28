using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Persistence;

/// <summary>
/// Applies pending EF Core migrations when the host environment is Development.
/// </summary>
public static class EfCoreDevelopmentMigrationExtensions
{
    /// <summary>
    /// Ensures the relational store exists, then applies pending EF Core migrations.
    /// Outside Development, this is a no-op.
    /// </summary>
    /// <typeparam name="TDbContext">Registered <see cref="DbContext"/> type.</typeparam>
    /// <param name="app">Built web application.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async static Task ApplyPendingMigrationsInDevelopmentAsync<TDbContext>(
        this WebApplication app,
        CancellationToken cancellationToken = default)
        where TDbContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(app);
        if (!app.Environment.IsDevelopment())
            return;

        await using AsyncServiceScope scope = app.Services.CreateAsyncScope();
        TDbContext db = scope.ServiceProvider.GetRequiredService<TDbContext>();
        ILogger<TDbContext>? logger = scope.ServiceProvider.GetService<ILogger<TDbContext>>();

        IRelationalDatabaseCreator? relationalCreator = db.GetService<IRelationalDatabaseCreator>();
        if (!await relationalCreator.ExistsAsync(cancellationToken).ConfigureAwait(false))
        {
            logger?.LogInformation(
                "Creating database catalog for {DbContext} (did not exist).",
                typeof(TDbContext).Name);
            await relationalCreator.CreateAsync(cancellationToken).ConfigureAwait(false);
        }

        string[] pending = (await db.Database.GetPendingMigrationsAsync(cancellationToken).ConfigureAwait(false)).ToArray();
        if (pending.Length > 0)
            logger?.LogInformation(
                "Applying {PendingCount} pending EF Core migration(s) for {DbContext}: {MigrationIds}",
                pending.Length,
                typeof(TDbContext).Name,
                string.Join(", ", pending));
        else
            logger?.LogInformation(
                "No pending EF Core migrations for {DbContext}; database is up to date.",
                typeof(TDbContext).Name);

        await db.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
    }
}
