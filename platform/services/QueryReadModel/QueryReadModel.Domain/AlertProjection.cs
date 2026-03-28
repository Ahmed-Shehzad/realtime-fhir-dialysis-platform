using BuildingBlocks;

namespace QueryReadModel.Domain;

public sealed class AlertProjection : BaseEntity
{
    public const int MaxAlertIdLength = 256;

    public const int MaxAlertTypeLength = 128;

    public const int MaxSeverityLength = 32;

    public const int MaxAlertStateLength = 64;

    public const int MaxTreatmentSessionIdLength = 256;

    private AlertProjection()
    {
    }

    public string AlertRowKey { get; private set; } = null!;

    public string AlertType { get; private set; } = null!;

    public string Severity { get; private set; } = null!;

    public string AlertState { get; private set; } = null!;

    public string? TreatmentSessionId { get; private set; }

    public DateTimeOffset RaisedAtUtc { get; private set; }

    public DateTimeOffset ProjectionUpdatedAtUtc { get; private set; }

    public static AlertProjection Create(
        string alertRowKey,
        string alertType,
        string severity,
        string alertState,
        string? treatmentSessionId,
        DateTimeOffset raisedAtUtc)
    {
        string key = (alertRowKey ?? string.Empty).Trim();
        if (key.Length == 0 || key.Length > MaxAlertIdLength)
            throw new ArgumentException("AlertRowKey is invalid.", nameof(alertRowKey));

        ValidateAndTrim(alertType, MaxAlertTypeLength, nameof(alertType), out string type);
        ValidateAndTrim(severity, MaxSeverityLength, nameof(severity), out string sev);
        ValidateAndTrim(alertState, MaxAlertStateLength, nameof(alertState), out string state);

        string? sid = TruncateOptional(treatmentSessionId, MaxTreatmentSessionIdLength);

        var row = new AlertProjection
        {
            AlertRowKey = key,
            AlertType = type,
            Severity = sev,
            AlertState = state,
            TreatmentSessionId = sid,
            RaisedAtUtc = raisedAtUtc,
            ProjectionUpdatedAtUtc = DateTimeOffset.UtcNow,
        };
        row.ApplyCreatedDateTime();
        return row;
    }

    public void UpdateAlert(string alertType, string severity, string alertState, string? treatmentSessionId)
    {
        ValidateAndTrim(alertType, MaxAlertTypeLength, nameof(alertType), out string type);
        ValidateAndTrim(severity, MaxSeverityLength, nameof(severity), out string sev);
        ValidateAndTrim(alertState, MaxAlertStateLength, nameof(alertState), out string state);

        AlertType = type;
        Severity = sev;
        AlertState = state;
        TreatmentSessionId = TruncateOptional(treatmentSessionId, MaxTreatmentSessionIdLength);
        ProjectionUpdatedAtUtc = DateTimeOffset.UtcNow;
        ApplyUpdateDateTime();
    }

    private static void ValidateAndTrim(string? value, int max, string paramName, out string result)
    {
        string t = (value ?? string.Empty).Trim();
        if (t.Length == 0 || t.Length > max)
            throw new ArgumentException($"{paramName} is invalid.", paramName);
        result = t;
    }

    private static string? TruncateOptional(string? value, int max)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        string t = value.Trim();
        return t.Length <= max ? t : t[..max];
    }
}
