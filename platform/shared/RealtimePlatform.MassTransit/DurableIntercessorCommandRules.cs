namespace RealtimePlatform.MassTransit;

internal static class DurableIntercessorCommandRules
{
    public static bool IsCommandTypeAllowed(Type type, MassTransitAzureServiceBusOptions options)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(options);
        string? asmName = type.Assembly.GetName().Name;
        if (string.IsNullOrEmpty(asmName))
            return false;
        if (asmName.StartsWith("System.", StringComparison.Ordinal)
            || asmName.StartsWith("Microsoft.", StringComparison.Ordinal))
            return false;
        IReadOnlyList<string> prefixes = options.AllowedCommandAssemblyNamePrefixes;
        if (prefixes.Count == 0)
            return true;
        return prefixes.Any(p => asmName.StartsWith(p, StringComparison.OrdinalIgnoreCase));
    }
}
