using System.Reflection;
using System.Text.Json;

using BuildingBlocks;
using BuildingBlocks.IntegrationEvents;

using Shouldly;

using Xunit;

namespace RealtimePlatform.ContractTests;

/// <summary>
/// Contract §20.3: Tier 1 catalog types must serialize to the catalog transport envelope without error.
/// </summary>
public sealed class IntegrationEventCatalogTransportContractTests
{
    private static readonly Assembly CatalogAssembly =
        typeof(RealtimePlatform.IntegrationEventCatalog.MeasurementAcceptedIntegrationEvent).Assembly;

    public static TheoryData<Type> Tier1IntegrationEventTypes { get; } = BuildTheoryData();

    private static TheoryData<Type> BuildTheoryData()
    {
        var data = new TheoryData<Type>();
        foreach (Type type in CatalogAssembly.GetExportedTypes())
        {
            if (type.IsAbstract || !typeof(IntegrationEvent).IsAssignableFrom(type))
                continue;
            if (type == typeof(IntegrationEvent))
                continue;
            data.Add(type);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(Tier1IntegrationEventTypes))]
    public void Tier1_event_serializes_to_non_empty_envelope_with_payload(Type eventType)
    {
        IntegrationEvent instance = CreateInstance(eventType);
        ReadOnlyMemory<byte> utf8 = IntegrationEventTransportSerializer.SerializeToUtf8Bytes(instance);
        utf8.Length.ShouldBeGreaterThan(0);

        using JsonDocument doc = JsonDocument.Parse(utf8.ToArray());
        JsonElement root = doc.RootElement;
        root.GetProperty("eventType").GetString().ShouldNotBeNullOrWhiteSpace();
        root.GetProperty("eventId").GetString().ShouldNotBeNullOrWhiteSpace();
        root.GetProperty("correlationId").GetString().ShouldNotBeNullOrWhiteSpace();
        root.GetProperty("payload").ValueKind.ShouldBe(JsonValueKind.Object);
    }

    [Theory]
    [MemberData(nameof(Tier1IntegrationEventTypes))]
    public void Tier1_event_round_trips_catalog_envelope_serialization(Type eventType)
    {
        IntegrationEvent original = CreateInstance(eventType);
        ReadOnlyMemory<byte> utf8 = IntegrationEventTransportSerializer.SerializeToUtf8Bytes(original);
        IntegrationEvent roundTripped =
            IntegrationEventTransportSerializer.DeserializeIntegrationEventFromCatalogEnvelope(utf8.Span);
        roundTripped.GetType().ShouldBe(original.GetType());
        roundTripped.EventId.ShouldBe(original.EventId);
        roundTripped.CorrelationId.ShouldBe(original.CorrelationId);
        roundTripped.EventVersion.ShouldBe(original.EventVersion);
    }

    private static IntegrationEvent CreateInstance(Type eventType)
    {
        ConstructorInfo[] ctors = eventType
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public)
            .OrderBy(c => c.GetParameters().Length)
            .ToArray();

        foreach (ConstructorInfo ctor in ctors)
        {
            ParameterInfo[] parameters = ctor.GetParameters();
            try
            {
                object?[] args = new object?[parameters.Length];
                for (int index = 0; index < parameters.Length; index++)
                    args[index] = DummyArgument(parameters[index]);

                if (ctor.Invoke(args) is IntegrationEvent ok)
                    return ok;
            }
            catch (TargetInvocationException)
            {
                // Try next constructor (e.g. secondary ctor shapes).
            }
        }

        throw new InvalidOperationException(
            $"Could not construct a test instance of {eventType.FullName}; add a factory or relax DummyArgument.");
    }

    private static object? DummyArgument(ParameterInfo parameter)
    {
        if (parameter.HasDefaultValue && parameter.DefaultValue != DBNull.Value)
            return parameter.DefaultValue;

        Type type = parameter.ParameterType;
        Type? nullableUnderlying = Nullable.GetUnderlyingType(type);
        if (nullableUnderlying is not null)
            return NullableDummy(nullableUnderlying, type);

        if (type == typeof(Ulid))
            return Ulid.NewUlid();
        if (type == typeof(string))
            return "contract-test-value";
        if (type == typeof(int))
            return 1;
        if (type == typeof(bool))
            return false;
        if (type == typeof(DateTime))
            return DateTime.UtcNow;
        if (type == typeof(DateTimeOffset))
            return DateTimeOffset.UtcNow;

        throw new NotSupportedException($"No dummy mapping for parameter type {type.FullName ?? type.Name}.");
    }

    private static object? NullableDummy(Type underlying, Type originalNullable)
    {
        if (underlying == typeof(int))
            return (int?)1;
        if (underlying == typeof(bool))
            return (bool?)false;
        if (underlying == typeof(DateTime))
            return (DateTime?)DateTime.UtcNow;
        if (underlying == typeof(DateTimeOffset))
            return (DateTimeOffset?)DateTimeOffset.UtcNow;
        if (underlying == typeof(string))
            return null;
        throw new NotSupportedException($"No dummy mapping for nullable type {originalNullable.FullName ?? originalNullable.Name}.");
    }
}
