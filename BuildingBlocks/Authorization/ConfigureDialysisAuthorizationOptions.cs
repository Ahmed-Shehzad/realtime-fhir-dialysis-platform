using BuildingBlocks.Options;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Authorization;

internal sealed class ConfigureDialysisAuthorizationOptions : IConfigureOptions<AuthorizationOptions>
{
    private readonly AuthorizationScopesOptions _scopes;

    public ConfigureDialysisAuthorizationOptions(IOptions<AuthorizationScopesOptions> scopes)
    {
        _scopes = scopes?.Value ?? throw new ArgumentNullException(nameof(scopes));
    }

    /// <inheritdoc />
    public void Configure(AuthorizationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.AddPolicy(
            PlatformAuthorizationPolicies.DevicesRead,
            p => p.Requirements.Add(new ScopeOrBypassRequirement(_scopes.DevicesRead)));
        options.AddPolicy(
            PlatformAuthorizationPolicies.DevicesWrite,
            p => p.Requirements.Add(new ScopeOrBypassRequirement(_scopes.DevicesWrite)));
        options.AddPolicy(
            PlatformAuthorizationPolicies.MeasurementsRead,
            p => p.Requirements.Add(new ScopeOrBypassRequirement(_scopes.MeasurementsRead)));
        options.AddPolicy(
            PlatformAuthorizationPolicies.MeasurementsWrite,
            p => p.Requirements.Add(new ScopeOrBypassRequirement(_scopes.MeasurementsWrite)));
        options.AddPolicy(
            PlatformAuthorizationPolicies.SessionsRead,
            p => p.Requirements.Add(new ScopeOrBypassRequirement(_scopes.SessionsRead)));
        options.AddPolicy(
            PlatformAuthorizationPolicies.SessionsWrite,
            p => p.Requirements.Add(new ScopeOrBypassRequirement(_scopes.SessionsWrite)));
        options.AddPolicy(
            PlatformAuthorizationPolicies.AuditRead,
            p => p.Requirements.Add(new ScopeOrBypassRequirement(_scopes.AuditRead)));
        options.AddPolicy(
            PlatformAuthorizationPolicies.AuditWrite,
            p => p.Requirements.Add(new ScopeOrBypassRequirement(_scopes.AuditWrite)));
        options.AddPolicy(
            PlatformAuthorizationPolicies.ValidationRead,
            p => p.Requirements.Add(new ScopeOrBypassRequirement(_scopes.ValidationRead)));
        options.AddPolicy(
            PlatformAuthorizationPolicies.ValidationWrite,
            p => p.Requirements.Add(new ScopeOrBypassRequirement(_scopes.ValidationWrite)));
        options.AddPolicy(
            PlatformAuthorizationPolicies.ConditioningRead,
            p => p.Requirements.Add(new ScopeOrBypassRequirement(_scopes.ConditioningRead)));
        options.AddPolicy(
            PlatformAuthorizationPolicies.ConditioningWrite,
            p => p.Requirements.Add(new ScopeOrBypassRequirement(_scopes.ConditioningWrite)));
        options.AddPolicy(
            PlatformAuthorizationPolicies.TerminologyRead,
            p => p.Requirements.Add(new ScopeOrBypassRequirement(_scopes.TerminologyRead)));
        options.AddPolicy(
            PlatformAuthorizationPolicies.TerminologyWrite,
            p => p.Requirements.Add(new ScopeOrBypassRequirement(_scopes.TerminologyWrite)));
        options.AddPolicy(
            PlatformAuthorizationPolicies.InteroperabilityRead,
            p => p.Requirements.Add(new ScopeOrBypassRequirement(_scopes.InteroperabilityRead)));
        options.AddPolicy(
            PlatformAuthorizationPolicies.InteroperabilityWrite,
            p => p.Requirements.Add(new ScopeOrBypassRequirement(_scopes.InteroperabilityWrite)));
        options.AddPolicy(
            PlatformAuthorizationPolicies.ReadModelRead,
            p => p.Requirements.Add(new ScopeOrBypassRequirement(_scopes.ReadModelRead)));
        options.AddPolicy(
            PlatformAuthorizationPolicies.ReadModelWrite,
            p => p.Requirements.Add(new ScopeOrBypassRequirement(_scopes.ReadModelWrite)));
        options.AddPolicy(
            PlatformAuthorizationPolicies.SurveillanceRead,
            p => p.Requirements.Add(new ScopeOrBypassRequirement(_scopes.SurveillanceRead)));
        options.AddPolicy(
            PlatformAuthorizationPolicies.SurveillanceWrite,
            p => p.Requirements.Add(new ScopeOrBypassRequirement(_scopes.SurveillanceWrite)));
        options.AddPolicy(
            PlatformAuthorizationPolicies.DeliveryRead,
            p => p.Requirements.Add(new ScopeOrBypassRequirement(_scopes.DeliveryRead)));
        options.AddPolicy(
            PlatformAuthorizationPolicies.DeliveryWrite,
            p => p.Requirements.Add(new ScopeOrBypassRequirement(_scopes.DeliveryWrite)));
        options.AddPolicy(
            PlatformAuthorizationPolicies.AnalyticsRead,
            p => p.Requirements.Add(new ScopeOrBypassRequirement(_scopes.AnalyticsRead)));
        options.AddPolicy(
            PlatformAuthorizationPolicies.AnalyticsWrite,
            p => p.Requirements.Add(new ScopeOrBypassRequirement(_scopes.AnalyticsWrite)));
        options.AddPolicy(
            PlatformAuthorizationPolicies.ReportingRead,
            p => p.Requirements.Add(new ScopeOrBypassRequirement(_scopes.ReportingRead)));
        options.AddPolicy(
            PlatformAuthorizationPolicies.ReportingWrite,
            p => p.Requirements.Add(new ScopeOrBypassRequirement(_scopes.ReportingWrite)));
        options.AddPolicy(
            PlatformAuthorizationPolicies.WorkflowRead,
            p => p.Requirements.Add(new ScopeOrBypassRequirement(_scopes.WorkflowRead)));
        options.AddPolicy(
            PlatformAuthorizationPolicies.WorkflowWrite,
            p => p.Requirements.Add(new ScopeOrBypassRequirement(_scopes.WorkflowWrite)));
        options.AddPolicy(
            PlatformAuthorizationPolicies.ReplayRead,
            p => p.Requirements.Add(new ScopeOrBypassRequirement(_scopes.ReplayRead)));
        options.AddPolicy(
            PlatformAuthorizationPolicies.ReplayWrite,
            p => p.Requirements.Add(new ScopeOrBypassRequirement(_scopes.ReplayWrite)));
        options.AddPolicy(
            PlatformAuthorizationPolicies.ConfigurationRead,
            p => p.Requirements.Add(new ScopeOrBypassRequirement(_scopes.ConfigurationRead)));
        options.AddPolicy(
            PlatformAuthorizationPolicies.ConfigurationWrite,
            p => p.Requirements.Add(new ScopeOrBypassRequirement(_scopes.ConfigurationWrite)));
        options.AddPolicy(
            PlatformAuthorizationPolicies.FinancialRead,
            p => p.Requirements.Add(new ScopeOrBypassRequirement(_scopes.FinancialRead)));
        options.AddPolicy(
            PlatformAuthorizationPolicies.FinancialWrite,
            p => p.Requirements.Add(new ScopeOrBypassRequirement(_scopes.FinancialWrite)));
    }
}
