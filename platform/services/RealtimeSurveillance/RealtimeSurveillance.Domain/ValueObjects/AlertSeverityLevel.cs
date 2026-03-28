namespace RealtimeSurveillance.Domain.ValueObjects;

public readonly record struct AlertSeverityLevel
{
    public const int MaxLength = 32;

    public string Value { get; }

    public AlertSeverityLevel(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        string t = value.Trim();
        if (t.Length == 0 || t.Length > MaxLength)
            throw new ArgumentException("Alert severity is invalid.", nameof(value));
        Value = t;
    }
}
