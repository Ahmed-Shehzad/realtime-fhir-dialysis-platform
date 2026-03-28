namespace WorkflowOrchestrator.Domain.ValueObjects;

public sealed class WorkflowLifecycleState : IEquatable<WorkflowLifecycleState>
{
    public const int MaxLength = 40;

    public string Value { get; }

    private WorkflowLifecycleState(string value) => Value = value;

    public static WorkflowLifecycleState Running { get; } = new("Running");

    public static WorkflowLifecycleState Completed { get; } = new("Completed");

    public static WorkflowLifecycleState Failed { get; } = new("Failed");

    public static WorkflowLifecycleState Compensating { get; } = new("Compensating");

    public static WorkflowLifecycleState ManualInterventionRequired { get; } = new("ManualInterventionRequired");

    public static WorkflowLifecycleState TimedOut { get; } = new("TimedOut");

    public static WorkflowLifecycleState FromStored(string? stored)
    {
        string t = (stored ?? string.Empty).Trim();
        return t switch
        {
            "Running" => Running,
            "Completed" => Completed,
            "Failed" => Failed,
            "Compensating" => Compensating,
            "ManualInterventionRequired" => ManualInterventionRequired,
            "TimedOut" => TimedOut,
            _ => throw new InvalidOperationException($"Unknown workflow lifecycle state: {t}."),
        };
    }

    public bool Equals(WorkflowLifecycleState? other) => other is not null && Value == other.Value;

    public override bool Equals(object? obj) => obj is WorkflowLifecycleState s && Equals(s);

    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    public static bool operator ==(WorkflowLifecycleState? a, WorkflowLifecycleState? b) =>
        ReferenceEquals(a, b) || (a is not null && b is not null && a.Value == b.Value);

    public static bool operator !=(WorkflowLifecycleState? a, WorkflowLifecycleState? b) => !(a == b);
}
