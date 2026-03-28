using System.Reflection;

using NetArchTest.Rules;

using Shouldly;

using Xunit;

namespace RealtimePlatform.ArchitectureTests;

/// <summary>
/// Clean Architecture dependency rules for Iteration 2–17 services.
/// </summary>
public sealed class ServiceLayeringRulesTests
{
    private static readonly Assembly DeviceRegistryDomain = typeof(DeviceRegistry.Domain.Device).Assembly;
    private static readonly Assembly DeviceRegistryApplication = typeof(DeviceRegistry.Application.Commands.RegisterDevice.RegisterDeviceCommand).Assembly;
    private static readonly Assembly MeasurementAcquisitionDomain = typeof(MeasurementAcquisition.Domain.RawMeasurementEnvelope).Assembly;
    private static readonly Assembly MeasurementAcquisitionApplication =
        typeof(MeasurementAcquisition.Application.Commands.IngestMeasurement.IngestMeasurementPayloadCommand).Assembly;
    private static readonly Assembly TreatmentSessionDomain = typeof(TreatmentSession.Domain.DialysisSession).Assembly;
    private static readonly Assembly TreatmentSessionApplication =
        typeof(TreatmentSession.Application.Commands.CreateSession.CreateDialysisSessionCommand).Assembly;
    private static readonly Assembly AuditProvenanceDomain = typeof(AuditProvenance.Domain.PlatformAuditFact).Assembly;
    private static readonly Assembly AuditProvenanceApplication =
        typeof(AuditProvenance.Application.Commands.RecordPlatformAuditFact.RecordPlatformAuditFactCommand).Assembly;
    private static readonly Assembly MeasurementValidationDomain = typeof(MeasurementValidation.Domain.ValidatedMeasurement).Assembly;
    private static readonly Assembly MeasurementValidationApplication =
        typeof(MeasurementValidation.Application.Commands.ValidateMeasurement.ValidateMeasurementCommand).Assembly;
    private static readonly Assembly SignalConditioningDomain = typeof(SignalConditioning.Domain.ConditioningResult).Assembly;
    private static readonly Assembly SignalConditioningApplication =
        typeof(SignalConditioning.Application.Commands.ConditionSignal.ConditionSignalCommand).Assembly;
    private static readonly Assembly TerminologyConformanceDomain = typeof(TerminologyConformance.Domain.ConformanceAssessment).Assembly;
    private static readonly Assembly TerminologyConformanceApplication =
        typeof(TerminologyConformance.Application.Commands.ValidateSemanticConformance.ValidateSemanticConformanceCommand).Assembly;
    private static readonly Assembly ClinicalInteroperabilityDomain =
        typeof(ClinicalInteroperability.Domain.CanonicalObservationPublication).Assembly;
    private static readonly Assembly ClinicalInteroperabilityApplication =
        typeof(ClinicalInteroperability.Application.Commands.PublishCanonicalObservation.PublishCanonicalObservationCommand).Assembly;
    private static readonly Assembly FinancialInteroperabilityDomain =
        typeof(FinancialInteroperability.Domain.PatientCoverageRegistration).Assembly;
    private static readonly Assembly FinancialInteroperabilityApplication =
        typeof(FinancialInteroperability.Application.Commands.RecordPatientCoverage.RecordPatientCoverageCommand).Assembly;
    private static readonly Assembly QueryReadModelDomain = typeof(QueryReadModel.Domain.SessionOverviewProjection).Assembly;
    private static readonly Assembly QueryReadModelApplication =
        typeof(QueryReadModel.Application.Commands.RebuildReadModelProjections.RebuildReadModelProjectionsCommand).Assembly;
    private static readonly Assembly RealtimeSurveillanceDomain = typeof(RealtimeSurveillance.Domain.SurveillanceAlert).Assembly;
    private static readonly Assembly RealtimeSurveillanceApplication =
        typeof(RealtimeSurveillance.Application.Commands.RaiseSurveillanceAlert.RaiseSurveillanceAlertCommand).Assembly;
    private static readonly Assembly RealtimeDeliveryDomain =
        typeof(RealtimeDelivery.Domain.Contracts.SessionFeedPayload).Assembly;
    private static readonly Assembly RealtimeDeliveryApplication =
        typeof(RealtimeDelivery.Application.Commands.BroadcastSessionFeed.BroadcastSessionFeedCommand).Assembly;
    private static readonly Assembly ClinicalAnalyticsDomain = typeof(ClinicalAnalytics.Domain.SessionAnalysis).Assembly;
    private static readonly Assembly ClinicalAnalyticsApplication =
        typeof(ClinicalAnalytics.Application.Commands.RunSessionAnalysis.RunSessionAnalysisCommand).Assembly;
    private static readonly Assembly ReportingDomain = typeof(Reporting.Domain.SessionReport).Assembly;
    private static readonly Assembly ReportingApplication =
        typeof(Reporting.Application.Commands.GenerateSessionReport.GenerateSessionReportCommand).Assembly;
    private static readonly Assembly WorkflowOrchestratorDomain = typeof(WorkflowOrchestrator.Domain.WorkflowInstance).Assembly;
    private static readonly Assembly WorkflowOrchestratorApplication =
        typeof(WorkflowOrchestrator.Application.Commands.StartWorkflowInstance.StartWorkflowInstanceCommand).Assembly;
    private static readonly Assembly ReplayRecoveryDomain = typeof(ReplayRecovery.Domain.ReplayJob).Assembly;
    private static readonly Assembly ReplayRecoveryApplication =
        typeof(ReplayRecovery.Application.Commands.StartReplayJob.StartReplayJobCommand).Assembly;
    private static readonly Assembly AdministrationConfigurationDomain =
        typeof(AdministrationConfiguration.Domain.FacilityConfiguration).Assembly;
    private static readonly Assembly AdministrationConfigurationApplication =
        typeof(AdministrationConfiguration.Application.Commands.UpsertFacilityConfiguration.UpsertFacilityConfigurationCommand).Assembly;
    private static readonly Assembly IntegrationEventCatalog =
        typeof(RealtimePlatform.IntegrationEventCatalog.MeasurementAcceptedIntegrationEvent).Assembly;

    private static string FormatFailures(TestResult result)
    {
        if (result.IsSuccessful) return string.Empty;
        return result.FailingTypes is null
            ? "Rule failed (no type list returned)."
            : string.Join(Environment.NewLine, result.FailingTypes.Select(t => t.FullName));
    }

    [Fact]
    public void DeviceRegistry_Domain_must_not_reference_Infrastructure_or_Api()
    {
        TestResult infra = Types.InAssembly(DeviceRegistryDomain)
            .Should()
            .NotHaveDependencyOn("DeviceRegistry.Infrastructure")
            .GetResult();

        TestResult api = Types.InAssembly(DeviceRegistryDomain)
            .Should()
            .NotHaveDependencyOn("DeviceRegistry.Api")
            .GetResult();

        infra.IsSuccessful.ShouldBeTrue(FormatFailures(infra));
        api.IsSuccessful.ShouldBeTrue(FormatFailures(api));
    }

    [Fact]
    public void DeviceRegistry_Application_must_not_reference_Infrastructure_or_Api()
    {
        TestResult infra = Types.InAssembly(DeviceRegistryApplication)
            .Should()
            .NotHaveDependencyOn("DeviceRegistry.Infrastructure")
            .GetResult();

        TestResult api = Types.InAssembly(DeviceRegistryApplication)
            .Should()
            .NotHaveDependencyOn("DeviceRegistry.Api")
            .GetResult();

        infra.IsSuccessful.ShouldBeTrue(FormatFailures(infra));
        api.IsSuccessful.ShouldBeTrue(FormatFailures(api));
    }

    [Fact]
    public void MeasurementAcquisition_Domain_must_not_reference_Infrastructure_or_Api()
    {
        TestResult infra = Types.InAssembly(MeasurementAcquisitionDomain)
            .Should()
            .NotHaveDependencyOn("MeasurementAcquisition.Infrastructure")
            .GetResult();

        TestResult api = Types.InAssembly(MeasurementAcquisitionDomain)
            .Should()
            .NotHaveDependencyOn("MeasurementAcquisition.Api")
            .GetResult();

        infra.IsSuccessful.ShouldBeTrue(FormatFailures(infra));
        api.IsSuccessful.ShouldBeTrue(FormatFailures(api));
    }

    [Fact]
    public void MeasurementAcquisition_Application_must_not_reference_Infrastructure_or_Api()
    {
        TestResult infra = Types.InAssembly(MeasurementAcquisitionApplication)
            .Should()
            .NotHaveDependencyOn("MeasurementAcquisition.Infrastructure")
            .GetResult();

        TestResult api = Types.InAssembly(MeasurementAcquisitionApplication)
            .Should()
            .NotHaveDependencyOn("MeasurementAcquisition.Api")
            .GetResult();

        infra.IsSuccessful.ShouldBeTrue(FormatFailures(infra));
        api.IsSuccessful.ShouldBeTrue(FormatFailures(api));
    }

    [Fact]
    public void TreatmentSession_Domain_must_not_reference_Infrastructure_or_Api()
    {
        TestResult infra = Types.InAssembly(TreatmentSessionDomain)
            .Should()
            .NotHaveDependencyOn("TreatmentSession.Infrastructure")
            .GetResult();

        TestResult api = Types.InAssembly(TreatmentSessionDomain)
            .Should()
            .NotHaveDependencyOn("TreatmentSession.Api")
            .GetResult();

        infra.IsSuccessful.ShouldBeTrue(FormatFailures(infra));
        api.IsSuccessful.ShouldBeTrue(FormatFailures(api));
    }

    [Fact]
    public void TreatmentSession_Application_must_not_reference_Infrastructure_or_Api()
    {
        TestResult infra = Types.InAssembly(TreatmentSessionApplication)
            .Should()
            .NotHaveDependencyOn("TreatmentSession.Infrastructure")
            .GetResult();

        TestResult api = Types.InAssembly(TreatmentSessionApplication)
            .Should()
            .NotHaveDependencyOn("TreatmentSession.Api")
            .GetResult();

        infra.IsSuccessful.ShouldBeTrue(FormatFailures(infra));
        api.IsSuccessful.ShouldBeTrue(FormatFailures(api));
    }

    [Fact]
    public void AuditProvenance_Domain_must_not_reference_Infrastructure_or_Api()
    {
        TestResult infra = Types.InAssembly(AuditProvenanceDomain)
            .Should()
            .NotHaveDependencyOn("AuditProvenance.Infrastructure")
            .GetResult();

        TestResult api = Types.InAssembly(AuditProvenanceDomain)
            .Should()
            .NotHaveDependencyOn("AuditProvenance.Api")
            .GetResult();

        infra.IsSuccessful.ShouldBeTrue(FormatFailures(infra));
        api.IsSuccessful.ShouldBeTrue(FormatFailures(api));
    }

    [Fact]
    public void AuditProvenance_Application_must_not_reference_Infrastructure_or_Api()
    {
        TestResult infra = Types.InAssembly(AuditProvenanceApplication)
            .Should()
            .NotHaveDependencyOn("AuditProvenance.Infrastructure")
            .GetResult();

        TestResult api = Types.InAssembly(AuditProvenanceApplication)
            .Should()
            .NotHaveDependencyOn("AuditProvenance.Api")
            .GetResult();

        infra.IsSuccessful.ShouldBeTrue(FormatFailures(infra));
        api.IsSuccessful.ShouldBeTrue(FormatFailures(api));
    }

    [Fact]
    public void MeasurementValidation_Domain_must_not_reference_Infrastructure_or_Api()
    {
        TestResult infra = Types.InAssembly(MeasurementValidationDomain)
            .Should()
            .NotHaveDependencyOn("MeasurementValidation.Infrastructure")
            .GetResult();

        TestResult api = Types.InAssembly(MeasurementValidationDomain)
            .Should()
            .NotHaveDependencyOn("MeasurementValidation.Api")
            .GetResult();

        infra.IsSuccessful.ShouldBeTrue(FormatFailures(infra));
        api.IsSuccessful.ShouldBeTrue(FormatFailures(api));
    }

    [Fact]
    public void MeasurementValidation_Application_must_not_reference_Infrastructure_or_Api()
    {
        TestResult infra = Types.InAssembly(MeasurementValidationApplication)
            .Should()
            .NotHaveDependencyOn("MeasurementValidation.Infrastructure")
            .GetResult();

        TestResult api = Types.InAssembly(MeasurementValidationApplication)
            .Should()
            .NotHaveDependencyOn("MeasurementValidation.Api")
            .GetResult();

        infra.IsSuccessful.ShouldBeTrue(FormatFailures(infra));
        api.IsSuccessful.ShouldBeTrue(FormatFailures(api));
    }

    [Fact]
    public void SignalConditioning_Domain_must_not_reference_Infrastructure_or_Api()
    {
        TestResult infra = Types.InAssembly(SignalConditioningDomain)
            .Should()
            .NotHaveDependencyOn("SignalConditioning.Infrastructure")
            .GetResult();

        TestResult api = Types.InAssembly(SignalConditioningDomain)
            .Should()
            .NotHaveDependencyOn("SignalConditioning.Api")
            .GetResult();

        infra.IsSuccessful.ShouldBeTrue(FormatFailures(infra));
        api.IsSuccessful.ShouldBeTrue(FormatFailures(api));
    }

    [Fact]
    public void SignalConditioning_Application_must_not_reference_Infrastructure_or_Api()
    {
        TestResult infra = Types.InAssembly(SignalConditioningApplication)
            .Should()
            .NotHaveDependencyOn("SignalConditioning.Infrastructure")
            .GetResult();

        TestResult api = Types.InAssembly(SignalConditioningApplication)
            .Should()
            .NotHaveDependencyOn("SignalConditioning.Api")
            .GetResult();

        infra.IsSuccessful.ShouldBeTrue(FormatFailures(infra));
        api.IsSuccessful.ShouldBeTrue(FormatFailures(api));
    }

    [Fact]
    public void TerminologyConformance_Domain_must_not_reference_Infrastructure_or_Api()
    {
        TestResult infra = Types.InAssembly(TerminologyConformanceDomain)
            .Should()
            .NotHaveDependencyOn("TerminologyConformance.Infrastructure")
            .GetResult();

        TestResult api = Types.InAssembly(TerminologyConformanceDomain)
            .Should()
            .NotHaveDependencyOn("TerminologyConformance.Api")
            .GetResult();

        infra.IsSuccessful.ShouldBeTrue(FormatFailures(infra));
        api.IsSuccessful.ShouldBeTrue(FormatFailures(api));
    }

    [Fact]
    public void TerminologyConformance_Application_must_not_reference_Infrastructure_or_Api()
    {
        TestResult infra = Types.InAssembly(TerminologyConformanceApplication)
            .Should()
            .NotHaveDependencyOn("TerminologyConformance.Infrastructure")
            .GetResult();

        TestResult api = Types.InAssembly(TerminologyConformanceApplication)
            .Should()
            .NotHaveDependencyOn("TerminologyConformance.Api")
            .GetResult();

        infra.IsSuccessful.ShouldBeTrue(FormatFailures(infra));
        api.IsSuccessful.ShouldBeTrue(FormatFailures(api));
    }

    [Fact]
    public void ClinicalInteroperability_Domain_must_not_reference_Infrastructure_or_Api()
    {
        TestResult infra = Types.InAssembly(ClinicalInteroperabilityDomain)
            .Should()
            .NotHaveDependencyOn("ClinicalInteroperability.Infrastructure")
            .GetResult();

        TestResult api = Types.InAssembly(ClinicalInteroperabilityDomain)
            .Should()
            .NotHaveDependencyOn("ClinicalInteroperability.Api")
            .GetResult();

        infra.IsSuccessful.ShouldBeTrue(FormatFailures(infra));
        api.IsSuccessful.ShouldBeTrue(FormatFailures(api));
    }

    [Fact]
    public void ClinicalInteroperability_Application_must_not_reference_Infrastructure_or_Api()
    {
        TestResult infra = Types.InAssembly(ClinicalInteroperabilityApplication)
            .Should()
            .NotHaveDependencyOn("ClinicalInteroperability.Infrastructure")
            .GetResult();

        TestResult api = Types.InAssembly(ClinicalInteroperabilityApplication)
            .Should()
            .NotHaveDependencyOn("ClinicalInteroperability.Api")
            .GetResult();

        infra.IsSuccessful.ShouldBeTrue(FormatFailures(infra));
        api.IsSuccessful.ShouldBeTrue(FormatFailures(api));
    }

    [Fact]
    public void FinancialInteroperability_Domain_must_not_reference_Infrastructure_or_Api()
    {
        TestResult infra = Types.InAssembly(FinancialInteroperabilityDomain)
            .Should()
            .NotHaveDependencyOn("FinancialInteroperability.Infrastructure")
            .GetResult();

        TestResult api = Types.InAssembly(FinancialInteroperabilityDomain)
            .Should()
            .NotHaveDependencyOn("FinancialInteroperability.Api")
            .GetResult();

        infra.IsSuccessful.ShouldBeTrue(FormatFailures(infra));
        api.IsSuccessful.ShouldBeTrue(FormatFailures(api));
    }

    [Fact]
    public void FinancialInteroperability_Application_must_not_reference_Infrastructure_or_Api()
    {
        TestResult infra = Types.InAssembly(FinancialInteroperabilityApplication)
            .Should()
            .NotHaveDependencyOn("FinancialInteroperability.Infrastructure")
            .GetResult();

        TestResult api = Types.InAssembly(FinancialInteroperabilityApplication)
            .Should()
            .NotHaveDependencyOn("FinancialInteroperability.Api")
            .GetResult();

        infra.IsSuccessful.ShouldBeTrue(FormatFailures(infra));
        api.IsSuccessful.ShouldBeTrue(FormatFailures(api));
    }

    [Fact]
    public void QueryReadModel_Domain_must_not_reference_Infrastructure_or_Api()
    {
        TestResult infra = Types.InAssembly(QueryReadModelDomain)
            .Should()
            .NotHaveDependencyOn("QueryReadModel.Infrastructure")
            .GetResult();

        TestResult api = Types.InAssembly(QueryReadModelDomain)
            .Should()
            .NotHaveDependencyOn("QueryReadModel.Api")
            .GetResult();

        infra.IsSuccessful.ShouldBeTrue(FormatFailures(infra));
        api.IsSuccessful.ShouldBeTrue(FormatFailures(api));
    }

    [Fact]
    public void QueryReadModel_Application_must_not_reference_Infrastructure_or_Api()
    {
        TestResult infra = Types.InAssembly(QueryReadModelApplication)
            .Should()
            .NotHaveDependencyOn("QueryReadModel.Infrastructure")
            .GetResult();

        TestResult api = Types.InAssembly(QueryReadModelApplication)
            .Should()
            .NotHaveDependencyOn("QueryReadModel.Api")
            .GetResult();

        infra.IsSuccessful.ShouldBeTrue(FormatFailures(infra));
        api.IsSuccessful.ShouldBeTrue(FormatFailures(api));
    }

    [Fact]
    public void RealtimeSurveillance_Domain_must_not_reference_Infrastructure_or_Api()
    {
        TestResult infra = Types.InAssembly(RealtimeSurveillanceDomain)
            .Should()
            .NotHaveDependencyOn("RealtimeSurveillance.Infrastructure")
            .GetResult();

        TestResult api = Types.InAssembly(RealtimeSurveillanceDomain)
            .Should()
            .NotHaveDependencyOn("RealtimeSurveillance.Api")
            .GetResult();

        infra.IsSuccessful.ShouldBeTrue(FormatFailures(infra));
        api.IsSuccessful.ShouldBeTrue(FormatFailures(api));
    }

    [Fact]
    public void RealtimeSurveillance_Application_must_not_reference_Infrastructure_or_Api()
    {
        TestResult infra = Types.InAssembly(RealtimeSurveillanceApplication)
            .Should()
            .NotHaveDependencyOn("RealtimeSurveillance.Infrastructure")
            .GetResult();

        TestResult api = Types.InAssembly(RealtimeSurveillanceApplication)
            .Should()
            .NotHaveDependencyOn("RealtimeSurveillance.Api")
            .GetResult();

        infra.IsSuccessful.ShouldBeTrue(FormatFailures(infra));
        api.IsSuccessful.ShouldBeTrue(FormatFailures(api));
    }

    [Fact]
    public void RealtimeDelivery_Domain_must_not_reference_Infrastructure_or_Api()
    {
        TestResult infra = Types.InAssembly(RealtimeDeliveryDomain)
            .Should()
            .NotHaveDependencyOn("RealtimeDelivery.Infrastructure")
            .GetResult();

        TestResult api = Types.InAssembly(RealtimeDeliveryDomain)
            .Should()
            .NotHaveDependencyOn("RealtimeDelivery.Api")
            .GetResult();

        infra.IsSuccessful.ShouldBeTrue(FormatFailures(infra));
        api.IsSuccessful.ShouldBeTrue(FormatFailures(api));
    }

    [Fact]
    public void RealtimeDelivery_Application_must_not_reference_Infrastructure_or_Api()
    {
        TestResult infra = Types.InAssembly(RealtimeDeliveryApplication)
            .Should()
            .NotHaveDependencyOn("RealtimeDelivery.Infrastructure")
            .GetResult();

        TestResult api = Types.InAssembly(RealtimeDeliveryApplication)
            .Should()
            .NotHaveDependencyOn("RealtimeDelivery.Api")
            .GetResult();

        infra.IsSuccessful.ShouldBeTrue(FormatFailures(infra));
        api.IsSuccessful.ShouldBeTrue(FormatFailures(api));
    }

    [Fact]
    public void ClinicalAnalytics_Domain_must_not_reference_Infrastructure_or_Api()
    {
        TestResult infra = Types.InAssembly(ClinicalAnalyticsDomain)
            .Should()
            .NotHaveDependencyOn("ClinicalAnalytics.Infrastructure")
            .GetResult();

        TestResult api = Types.InAssembly(ClinicalAnalyticsDomain)
            .Should()
            .NotHaveDependencyOn("ClinicalAnalytics.Api")
            .GetResult();

        infra.IsSuccessful.ShouldBeTrue(FormatFailures(infra));
        api.IsSuccessful.ShouldBeTrue(FormatFailures(api));
    }

    [Fact]
    public void ClinicalAnalytics_Application_must_not_reference_Infrastructure_or_Api()
    {
        TestResult infra = Types.InAssembly(ClinicalAnalyticsApplication)
            .Should()
            .NotHaveDependencyOn("ClinicalAnalytics.Infrastructure")
            .GetResult();

        TestResult api = Types.InAssembly(ClinicalAnalyticsApplication)
            .Should()
            .NotHaveDependencyOn("ClinicalAnalytics.Api")
            .GetResult();

        infra.IsSuccessful.ShouldBeTrue(FormatFailures(infra));
        api.IsSuccessful.ShouldBeTrue(FormatFailures(api));
    }

    [Fact]
    public void Reporting_Domain_must_not_reference_Infrastructure_or_Api()
    {
        TestResult infra = Types.InAssembly(ReportingDomain)
            .Should()
            .NotHaveDependencyOn("Reporting.Infrastructure")
            .GetResult();

        TestResult api = Types.InAssembly(ReportingDomain)
            .Should()
            .NotHaveDependencyOn("Reporting.Api")
            .GetResult();

        infra.IsSuccessful.ShouldBeTrue(FormatFailures(infra));
        api.IsSuccessful.ShouldBeTrue(FormatFailures(api));
    }

    [Fact]
    public void Reporting_Application_must_not_reference_Infrastructure_or_Api()
    {
        TestResult infra = Types.InAssembly(ReportingApplication)
            .Should()
            .NotHaveDependencyOn("Reporting.Infrastructure")
            .GetResult();

        TestResult api = Types.InAssembly(ReportingApplication)
            .Should()
            .NotHaveDependencyOn("Reporting.Api")
            .GetResult();

        infra.IsSuccessful.ShouldBeTrue(FormatFailures(infra));
        api.IsSuccessful.ShouldBeTrue(FormatFailures(api));
    }

    [Fact]
    public void WorkflowOrchestrator_Domain_must_not_reference_Infrastructure_or_Api()
    {
        TestResult infra = Types.InAssembly(WorkflowOrchestratorDomain)
            .Should()
            .NotHaveDependencyOn("WorkflowOrchestrator.Infrastructure")
            .GetResult();

        TestResult api = Types.InAssembly(WorkflowOrchestratorDomain)
            .Should()
            .NotHaveDependencyOn("WorkflowOrchestrator.Api")
            .GetResult();

        infra.IsSuccessful.ShouldBeTrue(FormatFailures(infra));
        api.IsSuccessful.ShouldBeTrue(FormatFailures(api));
    }

    [Fact]
    public void WorkflowOrchestrator_Application_must_not_reference_Infrastructure_or_Api()
    {
        TestResult infra = Types.InAssembly(WorkflowOrchestratorApplication)
            .Should()
            .NotHaveDependencyOn("WorkflowOrchestrator.Infrastructure")
            .GetResult();

        TestResult api = Types.InAssembly(WorkflowOrchestratorApplication)
            .Should()
            .NotHaveDependencyOn("WorkflowOrchestrator.Api")
            .GetResult();

        infra.IsSuccessful.ShouldBeTrue(FormatFailures(infra));
        api.IsSuccessful.ShouldBeTrue(FormatFailures(api));
    }

    [Fact]
    public void ReplayRecovery_Domain_must_not_reference_Infrastructure_or_Api()
    {
        TestResult infra = Types.InAssembly(ReplayRecoveryDomain)
            .Should()
            .NotHaveDependencyOn("ReplayRecovery.Infrastructure")
            .GetResult();

        TestResult api = Types.InAssembly(ReplayRecoveryDomain)
            .Should()
            .NotHaveDependencyOn("ReplayRecovery.Api")
            .GetResult();

        infra.IsSuccessful.ShouldBeTrue(FormatFailures(infra));
        api.IsSuccessful.ShouldBeTrue(FormatFailures(api));
    }

    [Fact]
    public void ReplayRecovery_Application_must_not_reference_Infrastructure_or_Api()
    {
        TestResult infra = Types.InAssembly(ReplayRecoveryApplication)
            .Should()
            .NotHaveDependencyOn("ReplayRecovery.Infrastructure")
            .GetResult();

        TestResult api = Types.InAssembly(ReplayRecoveryApplication)
            .Should()
            .NotHaveDependencyOn("ReplayRecovery.Api")
            .GetResult();

        infra.IsSuccessful.ShouldBeTrue(FormatFailures(infra));
        api.IsSuccessful.ShouldBeTrue(FormatFailures(api));
    }

    [Fact]
    public void AdministrationConfiguration_Domain_must_not_reference_Infrastructure_or_Api()
    {
        TestResult infra = Types.InAssembly(AdministrationConfigurationDomain)
            .Should()
            .NotHaveDependencyOn("AdministrationConfiguration.Infrastructure")
            .GetResult();

        TestResult api = Types.InAssembly(AdministrationConfigurationDomain)
            .Should()
            .NotHaveDependencyOn("AdministrationConfiguration.Api")
            .GetResult();

        infra.IsSuccessful.ShouldBeTrue(FormatFailures(infra));
        api.IsSuccessful.ShouldBeTrue(FormatFailures(api));
    }

    [Fact]
    public void AdministrationConfiguration_Application_must_not_reference_Infrastructure_or_Api()
    {
        TestResult infra = Types.InAssembly(AdministrationConfigurationApplication)
            .Should()
            .NotHaveDependencyOn("AdministrationConfiguration.Infrastructure")
            .GetResult();

        TestResult api = Types.InAssembly(AdministrationConfigurationApplication)
            .Should()
            .NotHaveDependencyOn("AdministrationConfiguration.Api")
            .GetResult();

        infra.IsSuccessful.ShouldBeTrue(FormatFailures(infra));
        api.IsSuccessful.ShouldBeTrue(FormatFailures(api));
    }

    [Fact]
    public void IntegrationEventCatalog_must_not_reference_EntityFrameworkCore()
    {
        TestResult ef = Types.InAssembly(IntegrationEventCatalog)
            .Should()
            .NotHaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        ef.IsSuccessful.ShouldBeTrue(FormatFailures(ef));
    }
}
