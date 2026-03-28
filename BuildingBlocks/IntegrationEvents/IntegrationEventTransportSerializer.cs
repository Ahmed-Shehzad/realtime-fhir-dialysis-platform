using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace BuildingBlocks.IntegrationEvents;

/// <summary>
/// Serializes <see cref="IntegrationEvent"/> instances to the catalog §1 envelope JSON (metadata + nested <c>payload</c>).
/// </summary>
public static class IntegrationEventTransportSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private static readonly HashSet<string> PayloadExcludedPropertyNames =
    [
        "eventId",
        "occurredOn",
        "correlationId",
        "eventVersion",
        "causationId",
        "workflowId",
        "sagaId",
        "facilityId",
        "sessionId",
        "patientId",
        "partitionKey",
        "routingDeviceId",
        "tenantId"
    ];

    /// <summary>
    /// Produces UTF-8 JSON bytes for the transport envelope.
    /// </summary>
    public static ReadOnlyMemory<byte> SerializeToUtf8Bytes(IntegrationEvent integrationEvent)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);
        Type runtimeType = integrationEvent.GetType();
        JsonNode? graph = JsonSerializer.SerializeToNode(integrationEvent, runtimeType, JsonOptions);
        if (graph is not JsonObject sourceObject)
            throw new InvalidOperationException($"Integration event serialized to unexpected JSON shape for {runtimeType.Name}.");

        var payloadObject = new JsonObject();
        foreach (KeyValuePair<string, JsonNode?> pair in sourceObject)
        {
            if (PayloadExcludedPropertyNames.Contains(pair.Key))
                continue;
            payloadObject[pair.Key] = pair.Value is null ? null : pair.Value.DeepClone();
        }

        string? deviceIdForEnvelope = integrationEvent.RoutingDeviceId;
        if (deviceIdForEnvelope is null
            && sourceObject.TryGetPropertyValue("deviceId", out JsonNode? deviceNode)
            && deviceNode is JsonValue dv
            && dv.TryGetValue(out string? d))
            deviceIdForEnvelope = d;

        var envelope = new JsonObject
        {
            ["eventId"] = integrationEvent.EventId.ToString(),
            ["eventType"] = runtimeType.FullName ?? runtimeType.Name,
            ["eventVersion"] = integrationEvent.EventVersion,
            ["occurredUtc"] = integrationEvent.OccurredOn,
            ["correlationId"] = integrationEvent.CorrelationId.ToString(),
            ["payload"] = payloadObject
        };

        AddUlidIfPresent(envelope, "causationId", integrationEvent.CausationId);
        AddUlidIfPresent(envelope, "workflowId", integrationEvent.WorkflowId);
        AddUlidIfPresent(envelope, "sagaId", integrationEvent.SagaId);
        AddStringIfPresent(envelope, "facilityId", integrationEvent.FacilityId);
        AddStringIfPresent(envelope, "sessionId", integrationEvent.SessionId);
        AddStringIfPresent(envelope, "patientId", integrationEvent.PatientId);
        AddStringIfPresent(envelope, "deviceId", deviceIdForEnvelope);
        AddStringIfPresent(envelope, "partitionKey", integrationEvent.PartitionKey);
        AddStringIfPresent(envelope, "tenantId", integrationEvent.TenantId);

        return JsonSerializer.SerializeToUtf8Bytes(envelope, JsonOptions);
    }

    /// <summary>
    /// Reconstructs a catalog <see cref="IntegrationEvent"/> from §1 envelope UTF-8 bytes produced by <see cref="SerializeToUtf8Bytes"/>.
    /// </summary>
    public static global::BuildingBlocks.IntegrationEvent DeserializeIntegrationEventFromCatalogEnvelope(
        ReadOnlySpan<byte> utf8EnvelopeBytes)
    {
        JsonObject? root;
        try
        {
            root = JsonNode.Parse(utf8EnvelopeBytes)?.AsObject();
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Integration event envelope is not valid JSON.", ex);
        }

        if (root is null)
            throw new InvalidOperationException("Integration event envelope is empty.");

        string? eventTypeName = root["eventType"]?.GetValue<string>();
        Type? resolvedType = ResolveIntegrationEventType(eventTypeName);
        if (resolvedType is null)
            throw new InvalidOperationException($"Unknown or disallowed integration event type: {eventTypeName}");

        JsonObject merged = MergeEnvelopeForDeserialization(root);
        string mergedJson = merged.ToJsonString(JsonOptions);
        object? deserialized = JsonSerializer.Deserialize(mergedJson, resolvedType, JsonOptions);
        if (deserialized is not global::BuildingBlocks.IntegrationEvent integrationEvent)
            throw new InvalidOperationException($"Deserialized payload is not an IntegrationEvent: {resolvedType.FullName}.");
        return integrationEvent;
    }

    private static Type? ResolveIntegrationEventType(string? fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return null;
        Type? t = Type.GetType(fullName, throwOnError: false, ignoreCase: false);
        if (t is not null && IsAllowedIntegrationEventType(t))
            return t;
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                t = assembly.GetType(fullName, throwOnError: false, ignoreCase: false);
            }
            catch
            {
                continue;
            }

            if (t is not null && IsAllowedIntegrationEventType(t))
                return t;
        }

        return null;
    }

    private static bool IsAllowedIntegrationEventType(Type type) =>
        type is { IsAbstract: false, IsInterface: false }
        && typeof(global::BuildingBlocks.IntegrationEvent).IsAssignableFrom(type)
        && type.Assembly.GetName().Name is string asm
        && !asm.StartsWith("System.", StringComparison.Ordinal)
        && !asm.StartsWith("Microsoft.", StringComparison.Ordinal);

    private static JsonObject MergeEnvelopeForDeserialization(JsonObject root)
    {
        var merged = new JsonObject();
        foreach (KeyValuePair<string, JsonNode?> pair in root)
        {
            if (pair.Key is "payload" or "eventType")
                continue;
            if (pair.Key == "occurredUtc")
            {
                merged["occurredOn"] = CloneJsonNode(pair.Value);
                continue;
            }

            merged[pair.Key] = CloneJsonNode(pair.Value);
        }

        if (root.TryGetPropertyValue("payload", out JsonNode? payloadNode) && payloadNode is JsonObject payloadObj)
            foreach (KeyValuePair<string, JsonNode?> pair in payloadObj)
                merged[pair.Key] = CloneJsonNode(pair.Value);

        if (merged.TryGetPropertyValue("deviceId", out JsonNode? deviceNode)
            && !merged.ContainsKey("routingDeviceId"))
            merged["routingDeviceId"] = CloneJsonNode(deviceNode);

        _ = merged.Remove("deviceId");
        return merged;
    }

    private static JsonNode? CloneJsonNode(JsonNode? node) =>
        node is null ? null : JsonNode.Parse(node.ToJsonString());

    private static void AddUlidIfPresent(JsonObject envelope, string name, Ulid? value)
    {
        if (value is null)
            return;
        envelope[name] = value.Value.ToString();
    }

    private static void AddStringIfPresent(JsonObject envelope, string name, string? value)
    {
        if (string.IsNullOrEmpty(value))
            return;
        envelope[name] = value;
    }
}
