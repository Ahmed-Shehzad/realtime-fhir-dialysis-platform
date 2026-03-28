namespace RealtimeSurveillance.Domain.ValueObjects;

public readonly record struct AlertTypeCode
{
    public const int MaxLength = 128;

    public string Value { get; }

    public AlertTypeCode(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        string t = value.Trim();
        if (t.Length == 0 || t.Length > MaxLength)
            throw new ArgumentException("Alert type is invalid.", nameof(value));
        Value = t;
    }
}
