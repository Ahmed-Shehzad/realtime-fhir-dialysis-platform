using Intercessor.Abstractions;

namespace Reporting.Application.Commands.GenerateSessionReport;

public sealed record SessionReportEvidenceDto(string Kind, string Locator);

public sealed record GenerateSessionReportCommand(
    Ulid CorrelationId,
    string TreatmentSessionId,
    string NarrativeVersion,
    IReadOnlyList<SessionReportEvidenceDto>? Evidence,
    string? AuthenticatedUserId = null) : ICommand<Ulid>;
