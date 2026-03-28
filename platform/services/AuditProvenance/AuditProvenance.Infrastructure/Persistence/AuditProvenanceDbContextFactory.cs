using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AuditProvenance.Infrastructure.Persistence;

public sealed class AuditProvenanceDbContextFactory : IDesignTimeDbContextFactory<AuditProvenanceDbContext>
{
    public AuditProvenanceDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AuditProvenanceDbContext>();
        string connectionString = Environment.GetEnvironmentVariable("REALTIME_PLATFORM_EF_CONNECTION")
            ?? "Host=127.0.0.1;Port=5432;Database=audit_provenance_dev;Username=postgres;Password=postgres";
        _ = optionsBuilder.UseNpgsql(connectionString);
        return new AuditProvenanceDbContext(optionsBuilder.Options);
    }
}
