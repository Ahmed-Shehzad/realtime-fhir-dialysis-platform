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

using MeasurementValidation.Application.Commands.ValidateMeasurement;
using MeasurementValidation.Domain.Abstractions;
using MeasurementValidation.Infrastructure.Persistence;

using RealtimePlatform.Observability;
using RealtimePlatform.Redis.DependencyInjection;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
_ = builder.AddServiceDefaults();
_ = builder.AddRealtimePlatformObservability("MeasurementValidation.Api");

_ = builder.Services.AddDialysisPlatformC5WebApi(builder.Configuration);
_ = builder.Services.AddScoped<IAuditEventStore, EfMeasurementValidationAuditEventStore>();
_ = builder.Services.AddScoped<IAuditRecorder, FhirAuditRecorder>();
_ = builder.Services.AddRealtimePlatformMessagingPersistence(builder.Configuration);
_ = builder.Services.AddRealtimePlatformRedis(builder.Configuration);
_ = builder.Services.AddDialysisPlatformMassTransit<MeasurementValidationDbContext>(builder.Configuration);
_ = builder.Services.AddScoped<DomainEventDispatcherInterceptor>();
_ = builder.Services.AddScoped<IntegrationEventDispatcherInterceptor>();

string connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("ConnectionStrings:Default is required.");

_ = builder.Services.AddNpgsqlBoundedContext<MeasurementValidationDbContext>(connectionString);

_ = builder.Services.AddScoped<IMeasurementValidationRepository, MeasurementValidationRepository>();

_ = builder.Services.AddIntercessor(cfg =>
    cfg.RegisterFromAssembly(typeof(ValidateMeasurementCommand).Assembly));

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
await app.ApplyPendingMigrationsInDevelopmentAsync<MeasurementValidationDbContext>().ConfigureAwait(false);
_ = app.UseCentralExceptionHandler();
_ = app.UseDialysisPlatformC5();
_ = app.MapOpenApi().AllowAnonymous();
_ = app.MapHealthChecks("/health").AllowAnonymous();
_ = app.MapDefaultEndpoints();
_ = app.MapControllers();
await app.RunAsync().ConfigureAwait(false);
