namespace RealtimePlatform.Workflow;

/// <summary>
/// Identifies a durable workflow or saga instance.
/// </summary>
public readonly record struct WorkflowInstanceId(Ulid Value)
{
    /// <summary>
    /// Creates a new identifier.
    /// </summary>
    public static WorkflowInstanceId New() => new(Ulid.NewUlid());
}
