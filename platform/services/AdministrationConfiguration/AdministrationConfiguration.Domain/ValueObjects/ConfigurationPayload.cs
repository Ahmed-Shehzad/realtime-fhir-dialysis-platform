namespace AdministrationConfiguration.Domain.ValueObjects;

public readonly record struct ConfigurationPayload
{
    public string Json { get; }

    public ConfigurationPayload(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        Json = json;
    }
}
