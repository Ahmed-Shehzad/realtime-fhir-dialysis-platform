using System.Text.Json.Serialization;

namespace DeviceRegistry.Api.Controllers;

/// <summary>HTTP request body for device registration.</summary>
public sealed record RegisterDeviceRequest(
    [property: JsonPropertyName("deviceIdentifier")] string DeviceIdentifier,
    [property: JsonPropertyName("manufacturer")] string? Manufacturer,
    [property: JsonPropertyName("initialTrustState")] string InitialTrustState);

/// <summary>HTTP response for successful registration.</summary>
public sealed record RegisterDeviceResponse(Ulid AggregateId, string DeviceId, string TrustState);

/// <summary>Trust probe response.</summary>
public sealed record DeviceTrustResponse(string DeviceId, bool Trusted);
