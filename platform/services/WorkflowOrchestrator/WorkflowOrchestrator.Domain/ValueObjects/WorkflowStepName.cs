namespace WorkflowOrchestrator.Domain.ValueObjects;

public sealed class WorkflowStepName : IEquatable<WorkflowStepName>
{
    public const int MaxLength = 256;

    public string Value { get; }

    public WorkflowStepName(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        string t = value.Trim();
        if (t.Length > MaxLength)
            throw new ArgumentException($"Workflow step name exceeds {MaxLength} characters.", nameof(value));
        Value = t;
    }

    public bool Equals(WorkflowStepName? other) => other is not null && Value == other.Value;

    public override bool Equals(object? obj) => obj is WorkflowStepName n && Equals(n);

    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    public static bool operator ==(WorkflowStepName? a, WorkflowStepName? b) =>
        ReferenceEquals(a, b) || (a is not null && b is not null && a.Value == b.Value);

    public static bool operator !=(WorkflowStepName? a, WorkflowStepName? b) => !(a == b);
}
