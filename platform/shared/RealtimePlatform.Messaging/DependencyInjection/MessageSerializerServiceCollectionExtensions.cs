using Microsoft.Extensions.DependencyInjection;

namespace RealtimePlatform.Messaging.DependencyInjection;

/// <summary>
/// Registration helpers for messaging primitives.
/// </summary>
public static class MessageSerializerServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="SystemTextJsonMessageSerializer"/> as <see cref="IMessageSerializer"/> (singleton).
    /// </summary>
    public static IServiceCollection AddRealtimePlatformJsonSerialization(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        _ = services.AddSingleton<IMessageSerializer, SystemTextJsonMessageSerializer>();
        return services;
    }
}
