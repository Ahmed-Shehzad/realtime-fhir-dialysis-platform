using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
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
    /// Outside Development, this is a no-op (logged so Aspire/local misconfiguration is obvious).
    /// </summary>
    /// <remarks>
    /// Uses a <see cref="DbContext"/> built from configuration only (no <see cref="DbContext"/> DI registration).
    /// Resolving the app-registered <typeparamref name="TDbContext"/> before <c>RunAsync</c> pulls in EF interceptors that depend on
    /// MassTransit (e.g. <c>IPublishEndpoint</c>), which can deadlock or block before the bus hosted services have started.
    /// </remarks>
    /// <typeparam name="TDbContext">Concrete <see cref="DbContext"/> type with a public constructor taking <see cref="DbContextOptions{TContext}"/>.</typeparam>
    /// <param name="app">Built web application.</param>
    /// <param name="connectionName">Configuration connection name (default <c>Default</c>).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async static Task ApplyPendingMigrationsInDevelopmentAsync<TDbContext>(
        this WebApplication app,
        string connectionName = "Default",
        CancellationToken cancellationToken = default)
        where TDbContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(app);
        if (!app.Environment.IsDevelopment())
        {
            app.Logger.LogWarning(
                "Skipping automatic EF Core migrations for {DbContext}: environment is {Environment} (not Development). " +
                "Schema must be applied manually or set ASPNETCORE_ENVIRONMENT=Development under Aspire.",
                typeof(TDbContext).FullName,
                app.Environment.EnvironmentName);
            return;
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(connectionName);

        string contextName = typeof(TDbContext).Name;
        app.Logger.LogInformation("EF Core: starting migrations for {DbContext}.", contextName);

        string? connectionString = app.Configuration.GetConnectionString(connectionName);
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                $"ConnectionStrings:{connectionName} is required for migrations (see Aspire WithReference or appsettings).");

        var optionsBuilder = new DbContextOptionsBuilder<TDbContext>();
        _ = optionsBuilder.UseNpgsql(connectionString);

        if (Activator.CreateInstance(typeof(TDbContext), optionsBuilder.Options) is not TDbContext db)
            throw new InvalidOperationException(
                $"Cannot create {typeof(TDbContext).Name} for migrations. Add a public constructor accepting DbContextOptions<{typeof(TDbContext).Name}>.");

        await using (db)
        {
            app.Logger.LogInformation(
                "EF Core: using no-interceptor {DbContext} instance for migrate (avoid MassTransit before host start).",
                contextName);

            IRelationalDatabaseCreator? relationalCreator = db.GetService<IRelationalDatabaseCreator>();
            if (relationalCreator is not null
                && !await relationalCreator.ExistsAsync(cancellationToken).ConfigureAwait(false))
            {
                app.Logger.LogInformation(
                    "EF Core: creating database catalog for {DbContext} (catalog did not exist).",
                    contextName);
                await relationalCreator.CreateAsync(cancellationToken).ConfigureAwait(false);
            }

            string[] pending =
                (await db.Database.GetPendingMigrationsAsync(cancellationToken).ConfigureAwait(false)).ToArray();
            if (pending.Length > 0)
                app.Logger.LogInformation(
                    "EF Core: applying {PendingCount} pending migration(s) for {DbContext}: {MigrationIds}",
                    pending.Length,
                    contextName,
                    string.Join(", ", pending));
            else
                app.Logger.LogInformation(
                    "EF Core: no pending migrations for {DbContext}; database schema is up to date.",
                    contextName);

            await db.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);

            string[] applied =
                (await db.Database.GetAppliedMigrationsAsync(cancellationToken).ConfigureAwait(false)).ToArray();
            app.Logger.LogInformation(
                "EF Core: migrate completed for {DbContext}; {AppliedCount} migration(s) recorded in history.",
                contextName,
                applied.Length);
        }
    }
}
