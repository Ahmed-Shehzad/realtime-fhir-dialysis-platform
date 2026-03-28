namespace BuildingBlocks.Authorization;

/// <summary>
/// Authorization policy names for platform APIs (map to Entra-exposed scopes via <see cref="Options.AuthorizationScopesOptions"/>).
/// </summary>
public static class PlatformAuthorizationPolicies
{
    public const string DevicesRead = "DevicesRead";

    public const string DevicesWrite = "DevicesWrite";

    public const string MeasurementsRead = "MeasurementsRead";

    public const string MeasurementsWrite = "MeasurementsWrite";

    public const string SessionsRead = "SessionsRead";

    public const string SessionsWrite = "SessionsWrite";

    public const string AuditRead = "AuditRead";

    public const string AuditWrite = "AuditWrite";

    public const string ValidationRead = "ValidationRead";

    public const string ValidationWrite = "ValidationWrite";

    public const string ConditioningRead = "ConditioningRead";

    public const string ConditioningWrite = "ConditioningWrite";

    public const string TerminologyRead = "TerminologyRead";

    public const string TerminologyWrite = "TerminologyWrite";

    public const string InteroperabilityRead = "InteroperabilityRead";

    public const string InteroperabilityWrite = "InteroperabilityWrite";

    public const string ReadModelRead = "ReadModelRead";

    public const string ReadModelWrite = "ReadModelWrite";

    public const string SurveillanceRead = "SurveillanceRead";

    public const string SurveillanceWrite = "SurveillanceWrite";

    public const string DeliveryRead = "DeliveryRead";

    public const string DeliveryWrite = "DeliveryWrite";

    public const string AnalyticsRead = "AnalyticsRead";

    public const string AnalyticsWrite = "AnalyticsWrite";

    public const string ReportingRead = "ReportingRead";

    public const string ReportingWrite = "ReportingWrite";

    public const string WorkflowRead = "WorkflowRead";

    public const string WorkflowWrite = "WorkflowWrite";

    public const string ReplayRead = "ReplayRead";

    public const string ReplayWrite = "ReplayWrite";

    public const string ConfigurationRead = "ConfigurationRead";

    public const string ConfigurationWrite = "ConfigurationWrite";

    public const string FinancialRead = "FinancialRead";

    public const string FinancialWrite = "FinancialWrite";
}
