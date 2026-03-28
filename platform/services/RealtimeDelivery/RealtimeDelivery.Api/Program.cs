using Asp.Versioning;

using BuildingBlocks;
using BuildingBlocks.Abstractions;
using BuildingBlocks.ExceptionHandling;
using BuildingBlocks.Audit;
using BuildingBlocks.DependencyInjection;
using BuildingBlocks.OpenApi;

using Intercessor;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Primitives;

using RealtimeDelivery.Api.Hubs;
using RealtimeDelivery.Api.Infrastructure;
using RealtimeDelivery.Application.Abstractions;
using RealtimeDelivery.Application.Commands.BroadcastSessionFeed;

using RealtimePlatform.Observability;
using RealtimePlatform.Redis.DependencyInjection;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
_ = builder.AddServiceDefaults();
_ = builder.AddRealtimePlatformObservability("RealtimeDelivery.Api");

_ = builder.Services.AddDialysisPlatformC5WebApi(builder.Configuration);
_ = builder.Services.AddSingleton<IAuditEventStore, InMemoryAuditEventStore>();
_ = builder.Services.AddScoped<IAuditRecorder, FhirAuditRecorder>();
_ = builder.Services.AddRealtimePlatformRedis(builder.Configuration);
_ = builder.Services.AddSignalR();
_ = builder.Services.AddScoped<IRealtimeFeedGateway, SignalRRealtimeFeedGateway>();

_ = builder.Services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
{
    JwtBearerEvents events = options.Events ??= new JwtBearerEvents();
    events.OnMessageReceived = context =>
    {
        PathString path = context.HttpContext.Request.Path;
        if (path.StartsWithSegments("/hubs"))
        {
            StringValues accessToken = context.Request.Query["access_token"];
            if (!StringValues.IsNullOrEmpty(accessToken))
                context.Token = accessToken;
        }
        return Task.CompletedTask;
    };
});

_ = builder.Services.AddIntercessor(cfg =>
    cfg.RegisterFromAssembly(typeof(BroadcastSessionFeedCommand).Assembly));

_ = builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    })
    .AddMvc();

_ = builder.Services.AddCentralExceptionHandler(builder.Configuration);

_ = builder.Services.AddControllers();
_ = builder.Services.AddDialysisPlatformOpenApi();
_ = builder.Services
    .AddHealthChecks()
    .AddRealtimePlatformRedisHealthCheck(builder.Configuration);

WebApplication app = builder.Build();
_ = app.UseCentralExceptionHandler();
_ = app.UseDialysisPlatformC5();
_ = app.MapOpenApi().AllowAnonymous();
_ = app.MapHealthChecks("/health").AllowAnonymous();
_ = app.MapDefaultEndpoints();
_ = app.MapHub<ClinicalFeedHub>("/hubs/clinical-feed");
_ = app.MapControllers();
await app.RunAsync().ConfigureAwait(false);
