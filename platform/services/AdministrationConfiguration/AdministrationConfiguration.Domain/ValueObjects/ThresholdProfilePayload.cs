namespace AdministrationConfiguration.Domain.ValueObjects;

public readonly record struct ThresholdProfilePayload
{
    public string Json { get; }

    public ThresholdProfilePayload(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        string trimmed = json.Trim();
        if (trimmed.Length == 0)
            throw new ArgumentException("Threshold profile payload must not be empty.", nameof(json));
        Json = trimmed;
    }
}
