using BuildingBlocks;
using BuildingBlocks.DependencyInjection;
using BuildingBlocks.Options;

using Microsoft.AspNetCore.HttpOverrides;

using RealtimePlatform.ApiGateway;
using RealtimePlatform.ApiGateway.Authorization;
using RealtimePlatform.Observability;

using Microsoft.AspNetCore.Authorization;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
_ = builder.AddServiceDefaults();
_ = builder.AddRealtimePlatformObservability("RealtimePlatform.ApiGateway");

_ = builder.Services.AddDialysisPlatformC5WebApi(builder.Configuration);
_ = builder.Services.AddSingleton<IAuthorizationHandler, GatewayIngressAuthorizationHandler>();
_ = builder.Services.Configure<AuthorizationOptions>(static options =>
{
    options.AddPolicy(
        GatewayAuthorizationPolicies.Ingress,
        static policy => policy.Requirements.Add(new GatewayIngressRequirement()));
});

builder.Services.AddOptions<ForwardedHeadersOptions>()
    .Configure<IConfiguration>((options, configuration) =>
    {
        IConfigurationSection section = configuration.GetSection("ForwardedHeaders");
        if (!section.GetValue("Enabled", false)) return;

        options.ForwardedHeaders =
            ForwardedHeaders.XForwardedFor
            | ForwardedHeaders.XForwardedProto
            | ForwardedHeaders.XForwardedHost;
        options.ForwardLimit = section.GetValue("ForwardLimit", 2);
        options.RequireHeaderSymmetry = section.GetValue("RequireHeaderSymmetry", false);

        if (section.GetValue("TrustIngressNetwork", false))
        {
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        }
    });

_ = builder.Services.AddDashboardSpaCors(builder.Environment, builder.Configuration);

_ = builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddServiceDiscoveryDestinationResolver();

WebApplication app = builder.Build();

bool jwtBypassConfigured = app.Configuration.GetValue<bool>(
    $"{JwtBearerStartupOptions.SectionName}:{nameof(JwtBearerStartupOptions.DevelopmentBypass)}");
if (jwtBypassConfigured && !app.Environment.IsDevelopment())
    app.Logger.LogWarning(
        "{BypassPath} is true, but the host environment is {Environment}. YARP ingress still requires an authenticated user (bypass applies only in Development).",
        $"{JwtBearerStartupOptions.SectionName}:{nameof(JwtBearerStartupOptions.DevelopmentBypass)}",
        app.Environment.EnvironmentName);

if (app.Environment.IsDevelopment() && !jwtBypassConfigured)
    app.Logger.LogWarning(
        "Development environment but {BypassPath} is false. YARP ingress requires a valid Bearer token for proxied APIs, or set DevelopmentBypass to true for local tooling.",
        $"{JwtBearerStartupOptions.SectionName}:{nameof(JwtBearerStartupOptions.DevelopmentBypass)}");

_ = app.UseForwardedHeaders();
_ = app.UseCors("spa");
_ = app.UseDialysisPlatformC5();

_ = app.MapGet(
        "/health",
        (IHostEnvironment env) => Results.Json(new
        {
            status = "Healthy",
            service = "RealtimePlatform.ApiGateway",
            environment = env.EnvironmentName,
        }))
    .AllowAnonymous();

_ = app.MapDefaultEndpoints();

_ = app.UseWebSockets();

IEndpointConventionBuilder proxyPipeline = app.MapReverseProxy(static proxyApp =>
{
    _ = proxyApp.UseMiddleware<ReverseProxyForwarderDevelopmentErrorMiddleware>();
    _ = proxyApp.UseSessionAffinity();
    _ = proxyApp.UseLoadBalancing();
    _ = proxyApp.UsePassiveHealthChecks();
});
_ = proxyPipeline.RequireAuthorization(GatewayAuthorizationPolicies.Ingress);

await app.RunAsync().ConfigureAwait(false);
