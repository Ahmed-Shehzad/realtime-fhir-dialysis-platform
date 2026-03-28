using Intercessor.Abstractions;

namespace Reporting.Application.Commands.FinalizeSessionReport;

public sealed record FinalizeSessionReportCommand(
    Ulid CorrelationId,
    Ulid ReportId,
    string? AuthenticatedUserId = null) : ICommand;
