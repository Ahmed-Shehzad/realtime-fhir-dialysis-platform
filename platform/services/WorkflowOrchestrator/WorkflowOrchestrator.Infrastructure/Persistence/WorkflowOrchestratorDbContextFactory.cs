using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WorkflowOrchestrator.Infrastructure.Persistence;

public sealed class WorkflowOrchestratorDbContextFactory : IDesignTimeDbContextFactory<WorkflowOrchestratorDbContext>
{
    public WorkflowOrchestratorDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<WorkflowOrchestratorDbContext>();
        string connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default")
            ?? Environment.GetEnvironmentVariable("REALTIME_PLATFORM_EF_CONNECTION")
            ?? "Host=127.0.0.1;Port=5432;Database=workflow_orchestrator_dev;Username=postgres;Password=postgres";
        _ = optionsBuilder.UseNpgsql(connectionString);
        return new WorkflowOrchestratorDbContext(optionsBuilder.Options);
    }
}
