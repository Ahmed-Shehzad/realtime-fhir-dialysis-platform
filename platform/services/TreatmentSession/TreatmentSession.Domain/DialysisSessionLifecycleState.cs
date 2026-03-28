namespace TreatmentSession.Domain;

/// <summary>
/// Persisted lifecycle for <see cref="DialysisSession"/> (MVP: create → assign → start → complete).
/// </summary>
public enum DialysisSessionLifecycleState
{
    Created = 0,
    Active = 1,
    Completed = 2
}
