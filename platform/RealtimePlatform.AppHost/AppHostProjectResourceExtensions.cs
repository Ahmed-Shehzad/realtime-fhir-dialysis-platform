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
}
