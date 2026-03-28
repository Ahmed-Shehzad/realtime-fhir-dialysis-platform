using System.Text.Json;

namespace AdministrationConfiguration.Domain.ValueObjects;

public readonly record struct RulesDocumentPayload
{
    public string Raw { get; }

    public RulesDocumentPayload(string raw)
    {
        ArgumentNullException.ThrowIfNull(raw);
        Raw = raw;
    }

    public void EnsurePublishable()
    {
        string trimmed = Raw.Trim();
        if (trimmed.Length == 0)
            throw new InvalidOperationException("Rule set document must not be empty for publication.");
        try
        {
            using JsonDocument doc = JsonDocument.Parse(trimmed);
            JsonElement root = doc.RootElement;
            if (root.ValueKind is not (JsonValueKind.Object or JsonValueKind.Array))
                throw new InvalidOperationException("Rule set document must be a JSON object or array.");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Rule set document must be valid JSON before publication.", ex);
        }
    }
}
