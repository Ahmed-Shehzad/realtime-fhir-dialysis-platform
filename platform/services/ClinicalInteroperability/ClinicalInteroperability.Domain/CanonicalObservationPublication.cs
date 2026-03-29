using BuildingBlocks;

using RealtimePlatform.IntegrationEventCatalog;

namespace ClinicalInteroperability.Domain;

/// <summary>Stub canonical FHIR Observation publication with MVP retry from <see cref="CanonicalPublicationState.Failed"/>.</summary>
public sealed class CanonicalObservationPublication : AggregateRoot
{
    public const int MaxMeasurementIdLength = 256;

    public const int MaxObservationIdLength = 128;

    public const int MaxFhirResourceReferenceLength = 2048;

    public const int MaxFailureReasonLength = 2000;

    public const int MaxFhirProfileUrlLength = 512;

    private const string StubFhirServerBase = "https://fhir.example.org/R4";

    private CanonicalObservationPublication()
    {
    }

    public string MeasurementId { get; private set; } = null!;

    public string ObservationId { get; private set; } = null!;

    public string? FhirProfileUrl { get; private set; }

    public CanonicalPublicationState State { get; private set; }

    public string? FhirResourceReference { get; private set; }

    public string? LastFailureReason { get; private set; }

    public int AttemptCount { get; private set; }

    public DateTimeOffset LastAttemptAtUtc { get; private set; }

    public static CanonicalObservationPublication StartPublication(
        Ulid correlationId,
        string measurementId,
        string? fhirProfileUrl,
        string? tenantId)
    {
        string mid = (measurementId ?? string.Empty).Trim();
        if (mid.Length == 0 || mid.Length > MaxMeasurementIdLength)
            throw new ArgumentException("MeasurementId is invalid.", nameof(measurementId));

        string observationId = Ulid.NewUlid().ToString();
        if (observationId.Length > MaxObservationIdLength)
            observationId = observationId[..MaxObservationIdLength];

        var publication = new CanonicalObservationPublication
        {
            MeasurementId = mid,
            ObservationId = observationId,
            FhirProfileUrl = NormalizeProfileUrl(fhirProfileUrl),
            AttemptCount = 1,
        };
        publication.ApplyCreatedDateTime();
        publication.ApplyStubOutcome(correlationId, tenantId);
        return publication;
    }

    public void RetryPublication(Ulid correlationId, string? tenantId)
    {
        if (State != CanonicalPublicationState.Failed)
            throw new InvalidOperationException("Retry is only allowed after a failed publication attempt.");

        AttemptCount++;
        ApplyUpdateDateTime();
        ApplyStubOutcome(correlationId, tenantId);
    }

    private void ApplyStubOutcome(Ulid correlationId, string? tenantId)
    {
        LastAttemptAtUtc = DateTimeOffset.UtcNow;
        ApplyUpdateDateTime();

        if (StubShouldFail(MeasurementId, FhirProfileUrl, AttemptCount))
        {
            State = CanonicalPublicationState.Failed;
            FhirResourceReference = null;
            LastFailureReason = TruncateReason(
                "Stub FHIR server rejected the conditional create (use measurement id containing 'perm-fhir-fail', or 'transient-once' for a one-shot failure).");
            ApplyEvent(
                new CanonicalObservationPublicationFailedIntegrationEvent(
                    correlationId,
                    ObservationId,
                    MeasurementId,
                    LastFailureReason)
                {
                    TenantId = tenantId,
                });
            return;
        }

        State = CanonicalPublicationState.Published;
        LastFailureReason = null;
        FhirResourceReference = $"{StubFhirServerBase}/Observation/{ObservationId}";
        if (FhirResourceReference.Length > MaxFhirResourceReferenceLength)
            FhirResourceReference = FhirResourceReference[..MaxFhirResourceReferenceLength];

        ApplyEvent(
            new CanonicalObservationPublishedIntegrationEvent(correlationId, ObservationId, FhirResourceReference)
            {
                TenantId = tenantId,
            });
    }

    private static bool StubShouldFail(string measurementId, string? fhirProfileUrl, int attemptCount)
    {
        if (measurementId.Contains("perm-fhir-fail", StringComparison.OrdinalIgnoreCase))
            return true;
        bool transientMarker =
            measurementId.Contains("transient-once", StringComparison.OrdinalIgnoreCase)
            || (!string.IsNullOrEmpty(fhirProfileUrl)
                && fhirProfileUrl.Contains("transient-once", StringComparison.OrdinalIgnoreCase));
        return transientMarker && attemptCount == 1;
    }

    private static string? NormalizeProfileUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        string t = value.Trim();
        return t.Length <= MaxFhirProfileUrlLength ? t : t[..MaxFhirProfileUrlLength];
    }

    private static string TruncateReason(string reason) =>
        reason.Length <= MaxFailureReasonLength ? reason : reason[..MaxFailureReasonLength];
}
