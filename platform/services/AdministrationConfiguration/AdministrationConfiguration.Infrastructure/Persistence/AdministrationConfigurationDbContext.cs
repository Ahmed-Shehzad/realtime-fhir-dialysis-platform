using Microsoft.EntityFrameworkCore;

using BuildingBlocks.Persistence;

using AdministrationConfiguration.Domain;

namespace AdministrationConfiguration.Infrastructure.Persistence;

public sealed class AdministrationConfigurationDbContext : DbContext
{
    public AdministrationConfigurationDbContext(
        DbContextOptions<AdministrationConfigurationDbContext> options)
        : base(options)
    {
    }

    public DbSet<FacilityConfiguration> FacilityConfigurations => Set<FacilityConfiguration>();

    public DbSet<RuleSet> RuleSets => Set<RuleSet>();

    public DbSet<ThresholdProfile> ThresholdProfiles => Set<ThresholdProfile>();

    public DbSet<FeatureToggle> FeatureToggles => Set<FeatureToggle>();

    public DbSet<AdministrationConfigurationSecurityAuditLogEntry> AdministrationConfigurationSecurityAuditLogEntries =>
        Set<AdministrationConfigurationSecurityAuditLogEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.AddMassTransitTransactionalOutboxAndInbox();
        _ = modelBuilder.ApplyConfigurationsFromAssembly(typeof(AdministrationConfigurationDbContext).Assembly);
    }
}
