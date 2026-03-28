using Microsoft.EntityFrameworkCore;

using BuildingBlocks.Persistence;

using FinancialInteroperability.Domain;

namespace FinancialInteroperability.Infrastructure.Persistence;

public sealed class FinancialInteroperabilityDbContext : DbContext
{
    public FinancialInteroperabilityDbContext(
        DbContextOptions<FinancialInteroperabilityDbContext> options)
        : base(options)
    {
    }

    public DbSet<PatientCoverageRegistration> PatientCoverageRegistrations => Set<PatientCoverageRegistration>();

    public DbSet<CoverageEligibilityInquiry> CoverageEligibilityInquiries => Set<CoverageEligibilityInquiry>();

    public DbSet<DialysisFinancialClaim> DialysisFinancialClaims => Set<DialysisFinancialClaim>();

    public DbSet<ExplanationOfBenefitRecord> ExplanationOfBenefitRecords => Set<ExplanationOfBenefitRecord>();

    public DbSet<SecurityAuditLogEntry> SecurityAuditLogEntries => Set<SecurityAuditLogEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.AddMassTransitTransactionalOutboxAndInbox();
        _ = modelBuilder.ApplyConfigurationsFromAssembly(typeof(FinancialInteroperabilityDbContext).Assembly);
    }
}
