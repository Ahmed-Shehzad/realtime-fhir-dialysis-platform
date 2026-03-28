namespace ReplayRecovery.Domain.ValueObjects;

public sealed class ProjectionSetName : IEquatable<ProjectionSetName>
{
    public const int MaxLength = 128;

    public string Value { get; }

    public ProjectionSetName(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        string t = value.Trim();
        if (t.Length > MaxLength)
            throw new ArgumentException($"Projection set name exceeds {MaxLength} characters.", nameof(value));
        Value = t;
    }

    public bool Equals(ProjectionSetName? other) => other is not null && Value == other.Value;

    public override bool Equals(object? obj) => obj is ProjectionSetName n && Equals(n);

    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    public static bool operator ==(ProjectionSetName? a, ProjectionSetName? b) =>
        ReferenceEquals(a, b) || (a is not null && b is not null && a.Value == b.Value);

    public static bool operator !=(ProjectionSetName? a, ProjectionSetName? b) => !(a == b);
}
