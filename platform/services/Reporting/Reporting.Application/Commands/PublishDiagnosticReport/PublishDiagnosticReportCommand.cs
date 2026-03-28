using Intercessor.Abstractions;

namespace Reporting.Application.Commands.PublishDiagnosticReport;

public sealed record PublishDiagnosticReportCommand(
    Ulid CorrelationId,
    Ulid ReportId,
    string? PublicationTargetHint,
    string? AuthenticatedUserId = null) : ICommand;
