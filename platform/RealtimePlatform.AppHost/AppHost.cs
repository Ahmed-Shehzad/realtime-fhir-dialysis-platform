using RealtimePlatform.AppHost;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

// In-cluster Host=DevPostgresInClusterHost (Aspire DNS). From the host OS use Host=localhost;Port=DevPostgresPublishedPort.
const string devPostgresInClusterHost = "postgres";
const string devPostgresUser = "postgres";
const string devPostgresPassword = "postgres";
const int devPostgresPublishedPort = 5432;

IResourceBuilder<ParameterResource> postgresUser = builder.AddParameter("postgres-user", devPostgresUser);
IResourceBuilder<ParameterResource> postgresPassword = builder.AddParameter(
    "postgres-password", devPostgresPassword, secret: true);

// PGDATA is initialized once per Docker volume; POSTGRES_PASSWORD is not reapplied on later runs.
// If "password authentication failed for user postgres", remove the volume or bump its name after credential changes.
IResourceBuilder<PostgresServerResource> postgres = builder
    .AddPostgres(devPostgresInClusterHost, postgresUser, postgresPassword, devPostgresPublishedPort)
    .WithDataVolume(name: "realtime-fhir-dialysis-postgres-data");

static IResourceBuilder<PostgresDatabaseResource> DevDb(
    IResourceBuilder<PostgresServerResource> pg,
    string resourceName,
    string postgresDatabaseName) =>
    pg.AddDatabase(resourceName, postgresDatabaseName);

static void ConfigureProjectForExplicitAspireHttp(ProjectResourceOptions options)
{
    options.ExcludeLaunchProfile = true;
    options.ExcludeKestrelEndpoints = true;
}

IResourceBuilder<PostgresDatabaseResource> dbDeviceRegistry =
    DevDb(postgres, "device-registry-dev", "device_registry_dev");
IResourceBuilder<PostgresDatabaseResource> dbMeasurementAcquisition =
    DevDb(postgres, "measurement-acquisition-dev", "measurement_acquisition_dev");
IResourceBuilder<PostgresDatabaseResource> dbTreatmentSession =
    DevDb(postgres, "treatment-session-dev", "treatment_session_dev");
IResourceBuilder<PostgresDatabaseResource> dbAuditProvenance =
    DevDb(postgres, "audit-provenance-dev", "audit_provenance_dev");
IResourceBuilder<PostgresDatabaseResource> dbMeasurementValidation =
    DevDb(postgres, "measurement-validation-dev", "measurement_validation_dev");
IResourceBuilder<PostgresDatabaseResource> dbSignalConditioning =
    DevDb(postgres, "signal-conditioning-dev", "signal_conditioning_dev");
IResourceBuilder<PostgresDatabaseResource> dbTerminologyConformance =
    DevDb(postgres, "terminology-conformance-dev", "terminology_conformance_dev");
IResourceBuilder<PostgresDatabaseResource> dbClinicalInteroperability =
    DevDb(postgres, "clinical-interoperability-dev", "clinical_interoperability_dev");
IResourceBuilder<PostgresDatabaseResource> dbFinancialInteroperability =
    DevDb(postgres, "financial-interoperability-dev", "financial_interoperability_dev");
IResourceBuilder<PostgresDatabaseResource> dbQueryReadModel =
    DevDb(postgres, "query-read-model-dev", "query_read_model_dev");
IResourceBuilder<PostgresDatabaseResource> dbRealtimeSurveillance =
    DevDb(postgres, "realtime-surveillance-dev", "realtime_surveillance_dev");
IResourceBuilder<PostgresDatabaseResource> dbClinicalAnalytics =
    DevDb(postgres, "clinical-analytics-dev", "clinical_analytics_dev");
IResourceBuilder<PostgresDatabaseResource> dbReporting =
    DevDb(postgres, "reporting-dev", "reporting_dev");
IResourceBuilder<PostgresDatabaseResource> dbWorkflowOrchestrator =
    DevDb(postgres, "workflow-orchestrator-dev", "workflow_orchestrator_dev");
IResourceBuilder<PostgresDatabaseResource> dbReplayRecovery =
    DevDb(postgres, "replay-recovery-dev", "replay_recovery_dev");
IResourceBuilder<PostgresDatabaseResource> dbAdministrationConfiguration =
    DevDb(postgres, "administration-configuration-dev", "administration_configuration_dev");

const string defaultConnection = "Default";

IResourceBuilder<ProjectResource> deviceRegistry =
    builder.AddProject<Projects.DeviceRegistry_Api>(
            "device-registry",
            ConfigureProjectForExplicitAspireHttp)
        .WithHttpEndpoint(port: 5001, targetPort: 5001, isProxied: false)
        .WithAspNetCoreDevelopmentEnvironment()
        .WithReference(dbDeviceRegistry, defaultConnection)
        .WaitFor(dbDeviceRegistry)
        .WithAspireReadinessProbe();
IResourceBuilder<ProjectResource> measurementAcquisition =
    builder.AddProject<Projects.MeasurementAcquisition_Api>(
            "measurement-acquisition",
            ConfigureProjectForExplicitAspireHttp)
        .WithHttpEndpoint(port: 5002, targetPort: 5002, isProxied: false)
        .WithAspNetCoreDevelopmentEnvironment()
        .WithReference(dbMeasurementAcquisition, defaultConnection)
        .WaitFor(dbMeasurementAcquisition)
        .WithAspireReadinessProbe();
IResourceBuilder<ProjectResource> treatmentSession =
    builder.AddProject<Projects.TreatmentSession_Api>(
            "treatment-session",
            ConfigureProjectForExplicitAspireHttp)
        .WithHttpEndpoint(port: 5003, targetPort: 5003, isProxied: false)
        .WithAspNetCoreDevelopmentEnvironment()
        .WithReference(dbTreatmentSession, defaultConnection)
        .WaitFor(dbTreatmentSession)
        .WithAspireReadinessProbe();
IResourceBuilder<ProjectResource> auditProvenance =
    builder.AddProject<Projects.AuditProvenance_Api>(
            "audit-provenance",
            ConfigureProjectForExplicitAspireHttp)
        .WithHttpEndpoint(port: 5004, targetPort: 5004, isProxied: false)
        .WithAspNetCoreDevelopmentEnvironment()
        .WithReference(dbAuditProvenance, defaultConnection)
        .WaitFor(dbAuditProvenance)
        .WithAspireReadinessProbe();
IResourceBuilder<ProjectResource> measurementValidation =
    builder.AddProject<Projects.MeasurementValidation_Api>(
            "measurement-validation",
            ConfigureProjectForExplicitAspireHttp)
        .WithHttpEndpoint(port: 5005, targetPort: 5005, isProxied: false)
        .WithAspNetCoreDevelopmentEnvironment()
        .WithReference(dbMeasurementValidation, defaultConnection)
        .WaitFor(dbMeasurementValidation)
        .WithAspireReadinessProbe();
IResourceBuilder<ProjectResource> signalConditioning =
    builder.AddProject<Projects.SignalConditioning_Api>(
            "signal-conditioning",
            ConfigureProjectForExplicitAspireHttp)
        .WithHttpEndpoint(port: 5006, targetPort: 5006, isProxied: false)
        .WithAspNetCoreDevelopmentEnvironment()
        .WithReference(dbSignalConditioning, defaultConnection)
        .WaitFor(dbSignalConditioning)
        .WithAspireReadinessProbe();
IResourceBuilder<ProjectResource> terminologyConformance =
    builder.AddProject<Projects.TerminologyConformance_Api>(
            "terminology-conformance",
            ConfigureProjectForExplicitAspireHttp)
        .WithHttpEndpoint(port: 5007, targetPort: 5007, isProxied: false)
        .WithAspNetCoreDevelopmentEnvironment()
        .WithReference(dbTerminologyConformance, defaultConnection)
        .WaitFor(dbTerminologyConformance)
        .WithAspireReadinessProbe();
IResourceBuilder<ProjectResource> clinicalInteroperability =
    builder.AddProject<Projects.ClinicalInteroperability_Api>(
            "clinical-interoperability",
            ConfigureProjectForExplicitAspireHttp)
        .WithHttpEndpoint(port: 5008, targetPort: 5008, isProxied: false)
        .WithAspNetCoreDevelopmentEnvironment()
        .WithReference(dbClinicalInteroperability, defaultConnection)
        .WaitFor(dbClinicalInteroperability)
        .WithAspireReadinessProbe();
IResourceBuilder<ProjectResource> financialInteroperability =
    builder.AddProject<Projects.FinancialInteroperability_Api>(
            "financial-interoperability",
            ConfigureProjectForExplicitAspireHttp)
        .WithHttpEndpoint(port: 5017, targetPort: 5017, isProxied: false)
        .WithAspNetCoreDevelopmentEnvironment()
        .WithReference(dbFinancialInteroperability, defaultConnection)
        .WaitFor(dbFinancialInteroperability)
        .WithAspireReadinessProbe();
IResourceBuilder<ProjectResource> queryReadModel =
    builder.AddProject<Projects.QueryReadModel_Api>(
            "query-read-model",
            ConfigureProjectForExplicitAspireHttp)
        .WithHttpEndpoint(port: 5009, targetPort: 5009, isProxied: false)
        .WithAspNetCoreDevelopmentEnvironment()
        .WithReference(dbQueryReadModel, defaultConnection)
        .WaitFor(dbQueryReadModel)
        .WithAspireReadinessProbe();
IResourceBuilder<ProjectResource> realtimeSurveillance =
    builder.AddProject<Projects.RealtimeSurveillance_Api>(
            "realtime-surveillance",
            ConfigureProjectForExplicitAspireHttp)
        .WithHttpEndpoint(port: 5010, targetPort: 5010, isProxied: false)
        .WithAspNetCoreDevelopmentEnvironment()
        .WithReference(dbRealtimeSurveillance, defaultConnection)
        .WaitFor(dbRealtimeSurveillance)
        .WithAspireReadinessProbe();
IResourceBuilder<ProjectResource> realtimeDelivery =
    builder.AddProject<Projects.RealtimeDelivery_Api>(
            "realtime-delivery",
            ConfigureProjectForExplicitAspireHttp)
        .WithHttpEndpoint(port: 5011, targetPort: 5011, isProxied: false)
        .WithAspNetCoreDevelopmentEnvironment()
        .WithAspireReadinessProbe();
IResourceBuilder<ProjectResource> clinicalAnalytics =
    builder.AddProject<Projects.ClinicalAnalytics_Api>(
            "clinical-analytics",
            ConfigureProjectForExplicitAspireHttp)
        .WithHttpEndpoint(port: 5012, targetPort: 5012, isProxied: false)
        .WithAspNetCoreDevelopmentEnvironment()
        .WithReference(dbClinicalAnalytics, defaultConnection)
        .WaitFor(dbClinicalAnalytics)
        .WithAspireReadinessProbe();
IResourceBuilder<ProjectResource> reporting =
    builder.AddProject<Projects.Reporting_Api>(
            "reporting",
            ConfigureProjectForExplicitAspireHttp)
        .WithHttpEndpoint(port: 5013, targetPort: 5013, isProxied: false)
        .WithAspNetCoreDevelopmentEnvironment()
        .WithReference(dbReporting, defaultConnection)
        .WaitFor(dbReporting)
        .WithAspireReadinessProbe();
IResourceBuilder<ProjectResource> workflowOrchestrator =
    builder.AddProject<Projects.WorkflowOrchestrator_Api>(
            "workflow-orchestrator",
            ConfigureProjectForExplicitAspireHttp)
        .WithHttpEndpoint(port: 5014, targetPort: 5014, isProxied: false)
        .WithAspNetCoreDevelopmentEnvironment()
        .WithReference(dbWorkflowOrchestrator, defaultConnection)
        .WaitFor(dbWorkflowOrchestrator)
        .WithAspireReadinessProbe();
IResourceBuilder<ProjectResource> replayRecovery =
    builder.AddProject<Projects.ReplayRecovery_Api>(
            "replay-recovery",
            ConfigureProjectForExplicitAspireHttp)
        .WithHttpEndpoint(port: 5015, targetPort: 5015, isProxied: false)
        .WithAspNetCoreDevelopmentEnvironment()
        .WithReference(dbReplayRecovery, defaultConnection)
        .WaitFor(dbReplayRecovery)
        .WithAspireReadinessProbe();
IResourceBuilder<ProjectResource> administrationConfiguration =
    builder.AddProject<Projects.AdministrationConfiguration_Api>(
            "administration-configuration",
            ConfigureProjectForExplicitAspireHttp)
        .WithHttpEndpoint(port: 5016, targetPort: 5016, isProxied: false)
        .WithAspNetCoreDevelopmentEnvironment()
        .WithReference(dbAdministrationConfiguration, defaultConnection)
        .WaitFor(dbAdministrationConfiguration)
        .WithAspireReadinessProbe();

// Environment variables override appsettings.Development.json localhost YARP targets so logical
// host names resolve via Microsoft.Extensions.ServiceDiscovery.Yarp under the AppHost.
_ = builder.AddProject<Projects.RealtimePlatform_ApiGateway>(
        "api-gateway",
        ConfigureProjectForExplicitAspireHttp)
    .WithReference(deviceRegistry)
    .WithReference(measurementAcquisition)
    .WithReference(treatmentSession)
    .WithReference(auditProvenance)
    .WithReference(measurementValidation)
    .WithReference(signalConditioning)
    .WithReference(terminologyConformance)
    .WithReference(clinicalInteroperability)
    .WithReference(financialInteroperability)
    .WithReference(queryReadModel)
    .WithReference(realtimeSurveillance)
    .WithReference(realtimeDelivery)
    .WithReference(clinicalAnalytics)
    .WithReference(reporting)
    .WithReference(workflowOrchestrator)
    .WithReference(replayRecovery)
    .WithReference(administrationConfiguration)
    .WithEnvironment(
        "ReverseProxy__Clusters__device-registry__Destinations__primary__Address",
        "http://device-registry/")
    .WithEnvironment(
        "ReverseProxy__Clusters__measurement-acquisition__Destinations__primary__Address",
        "http://measurement-acquisition/")
    .WithEnvironment(
        "ReverseProxy__Clusters__treatment-session__Destinations__primary__Address",
        "http://treatment-session/")
    .WithEnvironment(
        "ReverseProxy__Clusters__audit-provenance__Destinations__primary__Address",
        "http://audit-provenance/")
    .WithEnvironment(
        "ReverseProxy__Clusters__measurement-validation__Destinations__primary__Address",
        "http://measurement-validation/")
    .WithEnvironment(
        "ReverseProxy__Clusters__signal-conditioning__Destinations__primary__Address",
        "http://signal-conditioning/")
    .WithEnvironment(
        "ReverseProxy__Clusters__terminology-conformance__Destinations__primary__Address",
        "http://terminology-conformance/")
    .WithEnvironment(
        "ReverseProxy__Clusters__clinical-interoperability__Destinations__primary__Address",
        "http://clinical-interoperability/")
    .WithEnvironment(
        "ReverseProxy__Clusters__query-read-model__Destinations__primary__Address",
        "http://query-read-model/")
    .WithEnvironment(
        "ReverseProxy__Clusters__realtime-surveillance__Destinations__primary__Address",
        "http://realtime-surveillance/")
    .WithEnvironment(
        "ReverseProxy__Clusters__realtime-delivery__Destinations__primary__Address",
        "http://realtime-delivery/")
    .WithEnvironment(
        "ReverseProxy__Clusters__clinical-analytics__Destinations__primary__Address",
        "http://clinical-analytics/")
    .WithEnvironment(
        "ReverseProxy__Clusters__reporting__Destinations__primary__Address",
        "http://reporting/")
    .WithEnvironment(
        "ReverseProxy__Clusters__workflow-orchestrator__Destinations__primary__Address",
        "http://workflow-orchestrator/")
    .WithEnvironment(
        "ReverseProxy__Clusters__replay-recovery__Destinations__primary__Address",
        "http://replay-recovery/")
    .WithEnvironment(
        "ReverseProxy__Clusters__administration-configuration__Destinations__primary__Address",
        "http://administration-configuration/")
    .WithEnvironment(
        "ReverseProxy__Clusters__financial-interoperability__Destinations__primary__Address",
        "http://financial-interoperability/")
    .WithHttpEndpoint(port: 5100, targetPort: 5100, isProxied: false)
    .WithAspNetCoreDevelopmentEnvironment()
    .WaitFor(deviceRegistry)
    .WaitFor(measurementAcquisition)
    .WaitFor(treatmentSession)
    .WaitFor(auditProvenance)
    .WaitFor(measurementValidation)
    .WaitFor(signalConditioning)
    .WaitFor(terminologyConformance)
    .WaitFor(clinicalInteroperability)
    .WaitFor(financialInteroperability)
    .WaitFor(queryReadModel)
    .WaitFor(realtimeSurveillance)
    .WaitFor(realtimeDelivery)
    .WaitFor(clinicalAnalytics)
    .WaitFor(reporting)
    .WaitFor(workflowOrchestrator)
    .WaitFor(replayRecovery)
    .WaitFor(administrationConfiguration)
    .WithAspireReadinessProbe();

await builder.Build().RunAsync().ConfigureAwait(false);
