using BuildingBlocks;

using RealtimePlatform.IntegrationEventCatalog;

using ReplayRecovery.Domain.ValueObjects;

namespace ReplayRecovery.Domain;

public sealed class ReplayJob : AggregateRoot
{
    private ReplayJob()
    {
    }

    public ReplayMode Mode { get; private set; } = null!;

    public ReplayJobState State { get; private set; } = null!;

    public ProjectionSetName ProjectionSet { get; private set; } = null!;

    public int CheckpointSequence { get; private set; }

    public static ReplayJob Start(
        Ulid correlationId,
        ReplayMode mode,
        ProjectionSetName projectionSet,
        string? tenantId)
    {
        ArgumentNullException.ThrowIfNull(mode);
        ArgumentNullException.ThrowIfNull(projectionSet);
        var job = new ReplayJob
        {
            Mode = mode,
            State = ReplayJobState.Running,
            ProjectionSet = projectionSet,
            CheckpointSequence = 0,
        };
        job.ApplyCreatedDateTime();
        string jobId = job.Id.ToString();
        job.ApplyEvent(
            new ReplayStartedIntegrationEvent(correlationId, jobId, mode.Value, projectionSet.Value)
            {
                TenantId = tenantId,
            });
        return job;
    }

    public void AdvanceCheckpoint()
    {
        if (State != ReplayJobState.Running)
            throw new InvalidOperationException("Checkpoints can advance only while running.");
        CheckpointSequence = checked(CheckpointSequence + 1);
        ApplyUpdateDateTime();
    }

    public void Pause()
    {
        if (State != ReplayJobState.Running)
            throw new InvalidOperationException("Only running jobs can pause.");
        State = ReplayJobState.Paused;
        ApplyUpdateDateTime();
    }

    public void Resume()
    {
        if (State != ReplayJobState.Paused)
            throw new InvalidOperationException("Only paused jobs can resume.");
        State = ReplayJobState.Running;
        ApplyUpdateDateTime();
    }

    public void Cancel()
    {
        if (State != ReplayJobState.Running && State != ReplayJobState.Paused)
            throw new InvalidOperationException("Only running or paused jobs can cancel.");
        State = ReplayJobState.Cancelled;
        ApplyUpdateDateTime();
    }

    public void Complete(Ulid correlationId, string? tenantId)
    {
        if (State != ReplayJobState.Running)
            throw new InvalidOperationException("Only running jobs can complete.");
        State = ReplayJobState.Completed;
        ApplyUpdateDateTime();
        ApplyEvent(
            new ReplayCompletedIntegrationEvent(
                correlationId,
                Id.ToString(),
                ProjectionSet.Value,
                CheckpointSequence)
            {
                TenantId = tenantId,
            });
    }

    public void Fail(Ulid correlationId, string reason, string? tenantId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        if (State != ReplayJobState.Running && State != ReplayJobState.Paused)
            throw new InvalidOperationException("Only running or paused jobs can fail.");
        State = ReplayJobState.Failed;
        ApplyUpdateDateTime();
        ApplyEvent(
            new ReplayFailedIntegrationEvent(correlationId, Id.ToString(), reason.Trim())
            {
                TenantId = tenantId,
            });
    }
}
