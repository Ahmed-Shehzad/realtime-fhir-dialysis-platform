using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace MeasurementAcquisition.IntegrationTests;

/// <summary>Test host factory; relaxes DI validation so singleton EF interceptors can resolve scoped <c>IPublisher</c> like the real runtime.</summary>
public sealed class MeasurementAcquisitionApiFactory : WebApplicationFactory<Program>
{
    /// <inheritdoc />
    protected override IHost CreateHost(IHostBuilder builder)
    {
        _ = builder.UseDefaultServiceProvider((_, options) =>
        {
            options.ValidateScopes = false;
            options.ValidateOnBuild = false;
        });
        return base.CreateHost(builder);
    }
}
