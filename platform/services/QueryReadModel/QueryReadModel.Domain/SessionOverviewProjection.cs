using BuildingBlocks;

namespace QueryReadModel.Domain;

public sealed class SessionOverviewProjection : BaseEntity
{
    public const int MaxTreatmentSessionIdLength = 256;

    public const int MaxSessionStateLength = 64;

    public const int MaxPatientDisplayLength = 512;

    public const int MaxDeviceIdLength = 256;

    private SessionOverviewProjection()
    {
    }

    public string TreatmentSessionId { get; private set; } = null!;

    public string SessionState { get; private set; } = null!;

    public string? PatientDisplayLabel { get; private set; }

    public string? LinkedDeviceId { get; private set; }

    public DateTimeOffset SessionStartedAtUtc { get; private set; }

    public DateTimeOffset ProjectionUpdatedAtUtc { get; private set; }

    public static SessionOverviewProjection Create(
        string treatmentSessionId,
        string sessionState,
        string? patientDisplayLabel,
        string? linkedDeviceId,
        DateTimeOffset sessionStartedAtUtc)
    {
        string tid = (treatmentSessionId ?? string.Empty).Trim();
        if (tid.Length == 0 || tid.Length > MaxTreatmentSessionIdLength)
            throw new ArgumentException("TreatmentSessionId is invalid.", nameof(treatmentSessionId));

        string state = (sessionState ?? string.Empty).Trim();
        if (state.Length == 0 || state.Length > MaxSessionStateLength)
            throw new ArgumentException("SessionState is invalid.", nameof(sessionState));

        var row = new SessionOverviewProjection
        {
            TreatmentSessionId = tid,
            SessionState = state,
            PatientDisplayLabel = Truncate(patientDisplayLabel, MaxPatientDisplayLength),
            LinkedDeviceId = Truncate(linkedDeviceId, MaxDeviceIdLength),
            SessionStartedAtUtc = sessionStartedAtUtc,
            ProjectionUpdatedAtUtc = DateTimeOffset.UtcNow,
        };
        row.ApplyCreatedDateTime();
        return row;
    }

    public void UpdateOverview(
        string sessionState,
        string? patientDisplayLabel,
        string? linkedDeviceId,
        DateTimeOffset sessionStartedAtUtc)
    {
        string state = (sessionState ?? string.Empty).Trim();
        if (state.Length == 0 || state.Length > MaxSessionStateLength)
            throw new ArgumentException("SessionState is invalid.", nameof(sessionState));

        SessionState = state;
        PatientDisplayLabel = Truncate(patientDisplayLabel, MaxPatientDisplayLength);
        LinkedDeviceId = Truncate(linkedDeviceId, MaxDeviceIdLength);
        SessionStartedAtUtc = sessionStartedAtUtc;
        ProjectionUpdatedAtUtc = DateTimeOffset.UtcNow;
        ApplyUpdateDateTime();
    }

    private static string? Truncate(string? value, int max)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        string t = value.Trim();
        return t.Length <= max ? t : t[..max];
    }
}
