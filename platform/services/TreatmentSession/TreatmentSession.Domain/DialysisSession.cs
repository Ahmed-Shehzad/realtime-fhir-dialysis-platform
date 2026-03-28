using BuildingBlocks;

using BuildingBlocks.ValueObjects;

using RealtimePlatform.IntegrationEventCatalog;

using TreatmentSession.Domain.Events;

namespace TreatmentSession.Domain;

/// <summary>
/// Treatment session aggregate: identity, patient/device correlation, MVP lifecycle (Created → Active → Completed).
/// </summary>
public sealed class DialysisSession : AggregateRoot
{
    private DialysisSession()
    {
    }

    public DialysisSessionLifecycleState State { get; private set; }

    /// <summary>Assigned patient MRN when set.</summary>
    public MedicalRecordNumber? AssignedPatientMrn { get; private set; }

    public DeviceId? LinkedDeviceId { get; private set; }

    /// <summary>
    /// Creates a session in <see cref="DialysisSessionLifecycleState.Created"/>.
    /// </summary>
    public static DialysisSession Create(Ulid correlationId, string? tenantId)
    {
        var session = new DialysisSession
        {
            State = DialysisSessionLifecycleState.Created,
            LinkedDeviceId = null
        };
        session.ApplyEvent(new DialysisSessionCreatedDomainEvent(session.Id));
        session.ApplyEvent(
            new DialysisSessionCreatedIntegrationEvent(correlationId, session.Id.ToString())
            {
                SessionId = session.Id.ToString(),
                TenantId = tenantId
            });
        return session;
    }

    /// <summary>
    /// Assigns the patient (MRN) while the session is created.
    /// </summary>
    public void AssignPatient(Ulid correlationId, MedicalRecordNumber mrn, string? tenantId)
    {
        EnsureCreated(nameof(AssignPatient));
        if (AssignedPatientMrn is not null)
            throw new InvalidOperationException("Patient is already assigned to this session.");

        AssignedPatientMrn = mrn;
        ApplyUpdateDateTime();
        ApplyEvent(new PatientAssignedToSessionDomainEvent(Id, mrn.Value));
        ApplyEvent(
            new PatientAssignedToSessionIntegrationEvent(correlationId, Id.ToString(), mrn.Value)
            {
                SessionId = Id.ToString(),
                PatientId = mrn.Value,
                TenantId = tenantId
            });
    }

    /// <summary>
    /// Links a device while the session is created.
    /// </summary>
    public void LinkDevice(DeviceId deviceId)
    {
        EnsureCreated(nameof(LinkDevice));
        if (LinkedDeviceId is not null)
            throw new InvalidOperationException("Device is already linked to this session.");

        LinkedDeviceId = deviceId;
        ApplyUpdateDateTime();
    }

    /// <summary>
    /// Starts treatment: Created → Active; requires patient and device.
    /// </summary>
    public void Start(Ulid correlationId, string? tenantId)
    {
        EnsureCreated(nameof(Start));
        MedicalRecordNumber mrn = AssignedPatientMrn
            ?? throw new InvalidOperationException("Assign patient before starting the session.");
        DeviceId device = LinkedDeviceId
            ?? throw new InvalidOperationException("Link device before starting the session.");

        State = DialysisSessionLifecycleState.Active;
        ApplyUpdateDateTime();
        ApplyEvent(new DialysisSessionStartedDomainEvent(Id));
        ApplyEvent(
            new DialysisSessionStartedIntegrationEvent(correlationId, Id.ToString(), mrn.Value)
            {
                SessionId = Id.ToString(),
                PatientId = mrn.Value,
                RoutingDeviceId = device.Value,
                TenantId = tenantId
            });
    }

    /// <summary>
    /// Completes an active session.
    /// </summary>
    public void Complete(Ulid correlationId, string? tenantId)
    {
        if (State != DialysisSessionLifecycleState.Active)
            throw new InvalidOperationException("Session must be active to complete.");

        State = DialysisSessionLifecycleState.Completed;
        ApplyUpdateDateTime();
        ApplyEvent(new DialysisSessionCompletedDomainEvent(Id));
        string? patient = AssignedPatientMrn?.Value;
        string? routingDevice = LinkedDeviceId?.Value;
        ApplyEvent(
            new DialysisSessionCompletedIntegrationEvent(correlationId, Id.ToString())
            {
                SessionId = Id.ToString(),
                PatientId = patient,
                RoutingDeviceId = routingDevice,
                TenantId = tenantId
            });
    }

    /// <summary>
    /// Records that measurement context was resolved for this active session.
    /// </summary>
    public void ResolveMeasurementContext(Ulid correlationId, string measurementId, string? tenantId)
    {
        EnsureActive(nameof(ResolveMeasurementContext));
        ArgumentException.ThrowIfNullOrWhiteSpace(measurementId);
        string trimmed = measurementId.Trim();
        ApplyEvent(new MeasurementContextResolvedDomainEvent(Id, trimmed));
        ApplyEvent(
            new MeasurementContextResolvedIntegrationEvent(correlationId, Id.ToString(), trimmed)
            {
                SessionId = Id.ToString(),
                PatientId = AssignedPatientMrn?.Value,
                RoutingDeviceId = LinkedDeviceId?.Value,
                TenantId = tenantId
            });
        ApplyUpdateDateTime();
    }

    /// <summary>
    /// Records unresolved measurement context (quarantine signal) for this active session.
    /// </summary>
    public void MarkMeasurementContextUnresolved(
        Ulid correlationId,
        string measurementId,
        string reason,
        string? tenantId)
    {
        EnsureActive(nameof(MarkMeasurementContextUnresolved));
        ArgumentException.ThrowIfNullOrWhiteSpace(measurementId);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        string mid = measurementId.Trim();
        string r = reason.Trim();
        ApplyEvent(new MeasurementContextUnresolvedDomainEvent(Id, mid, r));
        ApplyEvent(
            new MeasurementContextUnresolvedIntegrationEvent(correlationId, Id.ToString(), mid, r)
            {
                SessionId = Id.ToString(),
                PatientId = AssignedPatientMrn?.Value,
                RoutingDeviceId = LinkedDeviceId?.Value,
                TenantId = tenantId
            });
        ApplyUpdateDateTime();
    }

    private void EnsureCreated(string operation)
    {
        if (State != DialysisSessionLifecycleState.Created)
            throw new InvalidOperationException($"{operation} requires session to be in Created state.");
    }

    private void EnsureActive(string operation)
    {
        if (State != DialysisSessionLifecycleState.Active)
            throw new InvalidOperationException($"{operation} requires session to be active.");
    }
}
