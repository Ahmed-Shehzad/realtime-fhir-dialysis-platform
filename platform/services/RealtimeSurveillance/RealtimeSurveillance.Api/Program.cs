using Asp.Versioning;

using BuildingBlocks;
using BuildingBlocks.Abstractions;
using BuildingBlocks.ExceptionHandling;
using BuildingBlocks.Audit;
using BuildingBlocks.DependencyInjection;
using BuildingBlocks.Interceptors;
using BuildingBlocks.OpenApi;
using BuildingBlocks.Persistence;

using Intercessor;

using RealtimeSurveillance.Application.Commands.RaiseSurveillanceAlert;
using RealtimeSurveillance.Domain.Abstractions;
using RealtimeSurveillance.Infrastructure.Persistence;

using RealtimePlatform.Observability;
using RealtimePlatform.Redis.DependencyInjection;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
_ = builder.AddServiceDefaults();
_ = builder.AddRealtimePlatformObservability("RealtimeSurveillance.Api");

_ = builder.Services.AddDialysisPlatformC5WebApi(builder.Configuration);
_ = builder.Services.AddScoped<IAuditEventStore, EfRealtimeSurveillanceAuditEventStore>();
_ = builder.Services.AddScoped<IAuditRecorder, FhirAuditRecorder>();
_ = builder.Services.AddRealtimePlatformMessagingPersistence(builder.Configuration);
_ = builder.Services.AddRealtimePlatformRedis(builder.Configuration);
_ = builder.Services.AddDialysisPlatformMassTransit<RealtimeSurveillanceDbContext>(builder.Configuration);
_ = builder.Services.AddScoped<DomainEventDispatcherInterceptor>();
_ = builder.Services.AddScoped<IntegrationEventDispatcherInterceptor>();

string connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("ConnectionStrings:Default is required.");

_ = builder.Services.AddNpgsqlBoundedContext<RealtimeSurveillanceDbContext>(connectionString);

_ = builder.Services.AddScoped<ISurveillanceAlertRepository, SurveillanceAlertRepository>();
_ = builder.Services.AddScoped<ISessionRiskSnapshotRepository, SessionRiskSnapshotRepository>();
_ = builder.Services.AddIntercessor(cfg =>
    cfg.RegisterFromAssembly(typeof(RaiseSurveillanceAlertCommand).Assembly));

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
    .AddNpgsqlDefaultConnectionHealthCheck()
    .AddRealtimePlatformRedisHealthCheck(builder.Configuration);

WebApplication app = builder.Build();
await app.ApplyPendingMigrationsInDevelopmentAsync<RealtimeSurveillanceDbContext>().ConfigureAwait(false);
_ = app.UseCentralExceptionHandler();
_ = app.UseDialysisPlatformC5();
_ = app.MapOpenApi().AllowAnonymous();
_ = app.MapHealthChecks("/health").AllowAnonymous();
_ = app.MapDefaultEndpoints();
_ = app.MapControllers();
await app.RunAsync().ConfigureAwait(false);
