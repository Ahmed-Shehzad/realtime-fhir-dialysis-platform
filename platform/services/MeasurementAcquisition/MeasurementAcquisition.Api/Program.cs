using Asp.Versioning;

using BuildingBlocks;
using BuildingBlocks.Abstractions;
using BuildingBlocks.ExceptionHandling;
using BuildingBlocks.Audit;
using BuildingBlocks.DependencyInjection;
using BuildingBlocks.OpenApi;
using BuildingBlocks.Interceptors;
using BuildingBlocks.Persistence;

using MeasurementAcquisition.Application.Commands.IngestMeasurement;
using MeasurementAcquisition.Domain.Abstractions;
using MeasurementAcquisition.Infrastructure.Persistence;

using Intercessor;

using RealtimePlatform.Observability;
using RealtimePlatform.Redis.DependencyInjection;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
_ = builder.AddServiceDefaults();
_ = builder.AddRealtimePlatformObservability("MeasurementAcquisition.Api");

_ = builder.Services.AddDialysisPlatformC5WebApi(builder.Configuration);
_ = builder.Services.AddScoped<IAuditEventStore, EfMeasurementAcquisitionAuditEventStore>();
_ = builder.Services.AddScoped<IAuditRecorder, FhirAuditRecorder>();
_ = builder.Services.AddRealtimePlatformMessagingPersistence(builder.Configuration);
_ = builder.Services.AddRealtimePlatformRedis(builder.Configuration);
_ = builder.Services.AddDialysisPlatformMassTransit<MeasurementAcquisitionDbContext>(builder.Configuration);
_ = builder.Services.AddScoped<DomainEventDispatcherInterceptor>();
_ = builder.Services.AddScoped<IntegrationEventDispatcherInterceptor>();

string connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("ConnectionStrings:Default is required.");

_ = builder.Services.AddNpgsqlBoundedContext<MeasurementAcquisitionDbContext>(connectionString);

_ = builder.Services.AddScoped<IAcquisitionRepository, AcquisitionRepository>();
_ = builder.Services.AddIntercessor(cfg =>
{
    cfg.RegisterFromAssembly(typeof(IngestMeasurementPayloadCommand).Assembly);
});

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
await app.ApplyPendingMigrationsInDevelopmentAsync<MeasurementAcquisitionDbContext>().ConfigureAwait(false);
_ = app.UseCentralExceptionHandler();
_ = app.UseDialysisPlatformC5();
_ = app.MapOpenApi().AllowAnonymous();
_ = app.MapHealthChecks("/health").AllowAnonymous();
_ = app.MapDefaultEndpoints();
_ = app.MapControllers();
await app.RunAsync().ConfigureAwait(false);
