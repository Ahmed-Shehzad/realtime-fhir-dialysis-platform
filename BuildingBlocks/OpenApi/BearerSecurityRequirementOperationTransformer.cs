using System.Reflection;

using BuildingBlocks.Authorization;
using BuildingBlocks.Options;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;

namespace BuildingBlocks.OpenApi;

/// <summary>
/// Sets <see cref="OpenApiOperation.Security"/> to the <c>Bearer</c> scheme when the endpoint has authorization metadata
/// and is not marked <see cref="IAllowAnonymous"/> (aligns OpenAPI with C5 JWT policies).
/// When <see cref="AuthorizeAttribute.Policy"/> matches <see cref="PlatformAuthorizationPolicies"/>, the requirement lists the
/// configured Entra scope value(s) from <see cref="AuthorizationScopesOptions"/> for client tooling.
/// </summary>
public sealed class BearerSecurityRequirementOperationTransformer : IOpenApiOperationTransformer
{
    private readonly AuthorizationScopesOptions _scopeOptions;

    /// <summary>
    /// Creates the transformer.
    /// </summary>
    public BearerSecurityRequirementOperationTransformer(IOptions<AuthorizationScopesOptions> scopeOptions) =>
        _scopeOptions = scopeOptions?.Value ?? throw new ArgumentNullException(nameof(scopeOptions));

    /// <inheritdoc />
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(context);
        if (context.Document is null)
            return Task.CompletedTask;
        if (context.Description?.ActionDescriptor is not ActionDescriptor descriptor)
            return Task.CompletedTask;
        if (!AuthorizedMvcActionRequiresBearer(descriptor))
            return Task.CompletedTask;

        List<string> scopes = ResolveScopeStrings(descriptor, _scopeOptions);
        var schemeReference = new OpenApiSecuritySchemeReference("Bearer", context.Document, string.Empty);
        var requirement = new OpenApiSecurityRequirement();
        requirement.Add(schemeReference, scopes);
        operation.Security ??= new List<OpenApiSecurityRequirement>();
        operation.Security.Add(requirement);
        return Task.CompletedTask;
    }

    private static List<string> ResolveScopeStrings(
        ActionDescriptor descriptor,
        AuthorizationScopesOptions scopes)
    {
        var collected = new HashSet<string>(StringComparer.Ordinal);
        if (descriptor is ControllerActionDescriptor cad)
        {
            foreach (AuthorizeAttribute attribute in cad.MethodInfo.GetCustomAttributes<AuthorizeAttribute>(inherit: true))
                AddScopesFromAuthorize(attribute, scopes, collected);
            Type? declaring = cad.MethodInfo.DeclaringType;
            if (declaring is not null)
                foreach (AuthorizeAttribute attribute in declaring.GetCustomAttributes<AuthorizeAttribute>(inherit: true))
                    AddScopesFromAuthorize(attribute, scopes, collected);
        }
        else
            foreach (object item in descriptor.EndpointMetadata)
                switch (item)
                {
                    case AuthorizeAttribute authorizeAttribute:
                        AddScopesFromAuthorize(authorizeAttribute, scopes, collected);
                        break;
                    case IAuthorizeData { Policy: { } policy } when !string.IsNullOrEmpty(policy):
                        AddScopeForPolicyName(policy, scopes, collected);
                        break;
                }

        return collected.Count > 0 ? collected.OrderBy(s => s, StringComparer.Ordinal).ToList() : new List<string>();
    }

    private static void AddScopesFromAuthorize(
        AuthorizeAttribute attribute,
        AuthorizationScopesOptions scopes,
        HashSet<string> collected)
    {
        if (!string.IsNullOrEmpty(attribute.Policy))
            AddScopeForPolicyName(attribute.Policy, scopes, collected);
    }

    private static void AddScopeForPolicyName(
        string policy,
        AuthorizationScopesOptions scopesOptions,
        HashSet<string> collected)
    {
        string? scope = policy switch
        {
            PlatformAuthorizationPolicies.DevicesRead => scopesOptions.DevicesRead,
            PlatformAuthorizationPolicies.DevicesWrite => scopesOptions.DevicesWrite,
            PlatformAuthorizationPolicies.MeasurementsRead => scopesOptions.MeasurementsRead,
            PlatformAuthorizationPolicies.MeasurementsWrite => scopesOptions.MeasurementsWrite,
            PlatformAuthorizationPolicies.SessionsRead => scopesOptions.SessionsRead,
            PlatformAuthorizationPolicies.SessionsWrite => scopesOptions.SessionsWrite,
            PlatformAuthorizationPolicies.AuditRead => scopesOptions.AuditRead,
            PlatformAuthorizationPolicies.AuditWrite => scopesOptions.AuditWrite,
            PlatformAuthorizationPolicies.ValidationRead => scopesOptions.ValidationRead,
            PlatformAuthorizationPolicies.ValidationWrite => scopesOptions.ValidationWrite,
            PlatformAuthorizationPolicies.ConditioningRead => scopesOptions.ConditioningRead,
            PlatformAuthorizationPolicies.ConditioningWrite => scopesOptions.ConditioningWrite,
            PlatformAuthorizationPolicies.TerminologyRead => scopesOptions.TerminologyRead,
            PlatformAuthorizationPolicies.TerminologyWrite => scopesOptions.TerminologyWrite,
            PlatformAuthorizationPolicies.InteroperabilityRead => scopesOptions.InteroperabilityRead,
            PlatformAuthorizationPolicies.InteroperabilityWrite => scopesOptions.InteroperabilityWrite,
            PlatformAuthorizationPolicies.ReadModelRead => scopesOptions.ReadModelRead,
            PlatformAuthorizationPolicies.ReadModelWrite => scopesOptions.ReadModelWrite,
            PlatformAuthorizationPolicies.SurveillanceRead => scopesOptions.SurveillanceRead,
            PlatformAuthorizationPolicies.SurveillanceWrite => scopesOptions.SurveillanceWrite,
            PlatformAuthorizationPolicies.DeliveryRead => scopesOptions.DeliveryRead,
            PlatformAuthorizationPolicies.DeliveryWrite => scopesOptions.DeliveryWrite,
            PlatformAuthorizationPolicies.AnalyticsRead => scopesOptions.AnalyticsRead,
            PlatformAuthorizationPolicies.AnalyticsWrite => scopesOptions.AnalyticsWrite,
            PlatformAuthorizationPolicies.ReportingRead => scopesOptions.ReportingRead,
            PlatformAuthorizationPolicies.ReportingWrite => scopesOptions.ReportingWrite,
            PlatformAuthorizationPolicies.WorkflowRead => scopesOptions.WorkflowRead,
            PlatformAuthorizationPolicies.WorkflowWrite => scopesOptions.WorkflowWrite,
            PlatformAuthorizationPolicies.ReplayRead => scopesOptions.ReplayRead,
            PlatformAuthorizationPolicies.ReplayWrite => scopesOptions.ReplayWrite,
            PlatformAuthorizationPolicies.ConfigurationRead => scopesOptions.ConfigurationRead,
            PlatformAuthorizationPolicies.ConfigurationWrite => scopesOptions.ConfigurationWrite,
            _ => null
        };
        if (!string.IsNullOrWhiteSpace(scope))
            _ = collected.Add(scope.Trim());
    }

    private static bool AuthorizedMvcActionRequiresBearer(ActionDescriptor descriptor)
    {
        if (descriptor is ControllerActionDescriptor cad)
        {
            if (cad.MethodInfo.GetCustomAttribute<AllowAnonymousAttribute>(inherit: true) is not null
                || ContainsAllowAnonymousOnDeclaringType(cad))
                return false;
            return cad.MethodInfo.GetCustomAttributes<AuthorizeAttribute>(inherit: true).Any()
                || cad.MethodInfo.DeclaringType?.GetCustomAttributes<AuthorizeAttribute>(inherit: true).Any() == true;
        }

        IList<object> metadata = descriptor.EndpointMetadata;
        if (metadata.Count == 0)
            return false;
        if (metadata.OfType<IAllowAnonymous>().Any())
            return false;
        return metadata.OfType<IAuthorizeData>().Any();
    }

    private static bool ContainsAllowAnonymousOnDeclaringType(ControllerActionDescriptor cad)
    {
        Type? t = cad.MethodInfo.DeclaringType;
        while (t is not null)
        {
            if (t.GetCustomAttribute<AllowAnonymousAttribute>(inherit: false) is not null)
                return true;
            t = t.DeclaringType;
        }

        return false;
    }
}
