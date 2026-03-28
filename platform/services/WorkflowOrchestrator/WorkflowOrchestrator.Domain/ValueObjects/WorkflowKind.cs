namespace WorkflowOrchestrator.Domain.ValueObjects;

public sealed class WorkflowKind : IEquatable<WorkflowKind>
{
    public const int MaxLength = 64;

    public string Value { get; }

    private WorkflowKind(string value) => Value = value;

    public static WorkflowKind SessionCompletion { get; } = new("SessionCompletion");

    public static WorkflowKind CanonicalPublication { get; } = new("CanonicalPublication");

    public static WorkflowKind ParseApi(string? raw)
    {
        string t = (raw ?? string.Empty).Trim();
        return t switch
        {
            nameof(SessionCompletion) => SessionCompletion,
            nameof(CanonicalPublication) => CanonicalPublication,
            _ => throw new ArgumentException("Workflow kind must be SessionCompletion or CanonicalPublication.", nameof(raw)),
        };
    }

    public static WorkflowKind FromStored(string? stored)
    {
        string t = (stored ?? string.Empty).Trim();
        return t switch
        {
            "SessionCompletion" => SessionCompletion,
            "CanonicalPublication" => CanonicalPublication,
            _ => throw new InvalidOperationException($"Unknown workflow kind: {t}."),
        };
    }

    public bool Equals(WorkflowKind? other) => other is not null && Value == other.Value;

    public override bool Equals(object? obj) => obj is WorkflowKind k && Equals(k);

    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    public static bool operator ==(WorkflowKind? a, WorkflowKind? b) =>
        ReferenceEquals(a, b) || (a is not null && b is not null && a.Value == b.Value);

    public static bool operator !=(WorkflowKind? a, WorkflowKind? b) => !(a == b);
}
