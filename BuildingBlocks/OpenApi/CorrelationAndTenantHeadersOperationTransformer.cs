using BuildingBlocks.Correlation;
using BuildingBlocks.Tenancy;

using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace BuildingBlocks.OpenApi;

/// <summary>
/// Adds optional <see cref="CorrelationIdConstants.HeaderName"/> and <see cref="TenantContext.TenantIdHeader"/> to each operation for client discoverability (C5 traceability and multi-tenancy).
/// </summary>
public sealed class CorrelationAndTenantHeadersOperationTransformer : IOpenApiOperationTransformer
{
    /// <inheritdoc />
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(operation);
        AddOptionalHeader(
            operation,
            CorrelationIdConstants.HeaderName,
            "Request correlation (ULID). If omitted, the server generates a value and echoes it on the response.");
        AddOptionalHeader(
            operation,
            TenantContext.TenantIdHeader,
            "Tenant for isolation; defaults to `default` when omitted.");
        return Task.CompletedTask;
    }

    private static void AddOptionalHeader(OpenApiOperation operation, string name, string description)
    {
        if (HasHeader(operation.Parameters, name))
            return;

        operation.Parameters ??= new List<IOpenApiParameter>();
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = name,
            In = ParameterLocation.Header,
            Required = false,
            Description = description,
            Schema = new OpenApiSchema { Type = JsonSchemaType.String },
        });
    }

    private static bool HasHeader(IList<IOpenApiParameter>? parameters, string name)
    {
        if (parameters is null || parameters.Count == 0)
            return false;
        foreach (IOpenApiParameter p in parameters)
            if (p is OpenApiParameter op
                && op.In == ParameterLocation.Header
                && string.Equals(op.Name, name, StringComparison.OrdinalIgnoreCase))
                return true;

        return false;
    }
}
