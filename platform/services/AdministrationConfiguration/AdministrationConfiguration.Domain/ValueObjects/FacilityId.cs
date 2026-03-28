namespace AdministrationConfiguration.Domain.ValueObjects;

public sealed class FacilityId : IEquatable<FacilityId>
{
    public const int MaxLength = 128;

    public string Value { get; }

    public FacilityId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        string trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"FacilityId exceeds max length {MaxLength}.", nameof(value));
        Value = trimmed;
    }

    private FacilityId()
    {
        Value = null!;
    }

    public override string ToString() => Value;

    public bool Equals(FacilityId? other) => other is not null && Value == other.Value;

    public override bool Equals(object? obj) => obj is FacilityId other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    public static implicit operator string(FacilityId id) => id.Value;
}
