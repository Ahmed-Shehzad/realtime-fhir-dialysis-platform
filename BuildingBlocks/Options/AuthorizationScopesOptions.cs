namespace BuildingBlocks.Options;

/// <summary>
/// OAuth2 scopes exposed by the API app registration; used for policy names under <see cref="Authorization.PlatformAuthorizationPolicies"/>.
/// </summary>
public sealed class AuthorizationScopesOptions
{
    public const string SectionName = "Authorization:Scopes";

    public string DevicesRead { get; set; } = "Dialysis.Devices.Read";

    public string DevicesWrite { get; set; } = "Dialysis.Devices.Write";

    public string MeasurementsRead { get; set; } = "Dialysis.Measurements.Read";

    public string MeasurementsWrite { get; set; } = "Dialysis.Measurements.Write";

    public string SessionsRead { get; set; } = "Dialysis.Sessions.Read";

    public string SessionsWrite { get; set; } = "Dialysis.Sessions.Write";

    public string AuditRead { get; set; } = "Dialysis.Audit.Read";

    public string AuditWrite { get; set; } = "Dialysis.Audit.Write";

    public string ValidationRead { get; set; } = "Dialysis.Validation.Read";

    public string ValidationWrite { get; set; } = "Dialysis.Validation.Write";

    public string ConditioningRead { get; set; } = "Dialysis.Conditioning.Read";

    public string ConditioningWrite { get; set; } = "Dialysis.Conditioning.Write";

    public string TerminologyRead { get; set; } = "Dialysis.Terminology.Read";

    public string TerminologyWrite { get; set; } = "Dialysis.Terminology.Write";

    public string InteroperabilityRead { get; set; } = "Dialysis.Interoperability.Read";

    public string InteroperabilityWrite { get; set; } = "Dialysis.Interoperability.Write";

    public string ReadModelRead { get; set; } = "Dialysis.ReadModel.Read";

    public string ReadModelWrite { get; set; } = "Dialysis.ReadModel.Write";

    public string SurveillanceRead { get; set; } = "Dialysis.Surveillance.Read";

    public string SurveillanceWrite { get; set; } = "Dialysis.Surveillance.Write";

    public string DeliveryRead { get; set; } = "Dialysis.Delivery.Read";

    public string DeliveryWrite { get; set; } = "Dialysis.Delivery.Write";

    public string AnalyticsRead { get; set; } = "Dialysis.Analytics.Read";

    public string AnalyticsWrite { get; set; } = "Dialysis.Analytics.Write";

    public string ReportingRead { get; set; } = "Dialysis.Reporting.Read";

    public string ReportingWrite { get; set; } = "Dialysis.Reporting.Write";

    public string WorkflowRead { get; set; } = "Dialysis.Workflow.Read";

    public string WorkflowWrite { get; set; } = "Dialysis.Workflow.Write";

    public string ReplayRead { get; set; } = "Dialysis.Replay.Read";

    public string ReplayWrite { get; set; } = "Dialysis.Replay.Write";

    public string ConfigurationRead { get; set; } = "Dialysis.Configuration.Read";

    public string ConfigurationWrite { get; set; } = "Dialysis.Configuration.Write";

    public string FinancialRead { get; set; } = "Dialysis.Financial.Read";

    public string FinancialWrite { get; set; } = "Dialysis.Financial.Write";
}
