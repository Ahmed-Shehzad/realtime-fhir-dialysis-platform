namespace RealtimePlatform.ApiGateway;

internal static class SpaCorsServiceCollectionExtensions
{
    public static IServiceCollection AddDashboardSpaCors(
        this IServiceCollection services,
        IHostEnvironment environment,
        IConfiguration configuration)
    {
        bool isDevelopment = environment.IsDevelopment();
        return services.AddCors(options =>
            options.AddPolicy(
                "spa",
                policy =>
                {
                    string[] origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                        ?? Array.Empty<string>();
                    if (origins.Length == 0)
                    {
                        _ = policy.SetIsOriginAllowed(_ => false).AllowAnyHeader().AllowAnyMethod();
                        return;
                    }

                    if (isDevelopment)
                    {
                        HashSet<string> configured = new(origins, StringComparer.Ordinal);
                        _ = policy
                            .SetIsOriginAllowed(origin => AllowsConfiguredOrLocalhostOrigin(origin, configured))
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    }
                    else _ = policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
                }));
    }

    private static bool AllowsConfiguredOrLocalhostOrigin(string origin, HashSet<string> configuredOrigins)
    {
        if (configuredOrigins.Contains(origin)) return true;

        if (!Uri.TryCreate(origin, UriKind.Absolute, out Uri? uri)) return false;

        return uri.Scheme is "http" or "https"
               && (string.Equals(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(uri.Host, "127.0.0.1", StringComparison.Ordinal));
    }
}
