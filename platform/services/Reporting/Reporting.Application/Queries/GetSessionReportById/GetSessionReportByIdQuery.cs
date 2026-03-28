using Intercessor.Abstractions;

namespace Reporting.Application.Queries.GetSessionReportById;

public sealed record GetSessionReportByIdQuery(Ulid ReportId) : IQuery<SessionReportReadDto?>;
