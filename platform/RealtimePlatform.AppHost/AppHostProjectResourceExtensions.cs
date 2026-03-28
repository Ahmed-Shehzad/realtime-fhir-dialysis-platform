namespace RealtimePlatform.AppHost;

internal static class AppHostProjectResourceExtensions
{
    /// <summary>
    /// When the launch profile is excluded, Aspire no longer applies profile env vars; set Development so
    /// <c>appsettings.Development.json</c> and JWT startup validation match local orchestration.
    /// </summary>
    public static IResourceBuilder<ProjectResource> WithAspNetCoreDevelopmentEnvironment(
        this IResourceBuilder<ProjectResource> builder) =>
        builder
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
            .WithEnvironment("DOTNET_ENVIRONMENT", "Development");

    /// <summary>
    /// Aspire's default HTTP probe is "/"; these apps have no root route.
    /// Probe <c>/alive</c> (liveness-only) so process startup succeeds once Kestrel listens; dependency readiness is enforced via <c>WaitFor</c> on Postgres database resources, not by duplicating DB checks in each HTTP probe.
    /// </summary>
    public static IResourceBuilder<ProjectResource> WithAspireReadinessProbe(
        this IResourceBuilder<ProjectResource> builder) =>
        builder.WithHttpHealthCheck("/alive");
}
