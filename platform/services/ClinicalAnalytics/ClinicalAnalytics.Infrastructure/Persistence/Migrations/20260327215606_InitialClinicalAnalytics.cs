using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicalAnalytics.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialClinicalAnalytics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "clinical_analytics_audit_log",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Action = table.Column<int>(type: "integer", nullable: false),
                    ResourceType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ResourceId = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    UserId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Outcome = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    TenantId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CorrelationId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clinical_analytics_audit_log", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "inbox_states",
                schema: "public",
                columns: table => new
                {
                    MessageId = table.Column<string>(type: "text", nullable: false),
                    ConsumerId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ReceivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inbox_states", x => new { x.MessageId, x.ConsumerId });
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "public",
                columns: table => new
                {
                    MessageId = table.Column<string>(type: "text", nullable: false),
                    CorrelationId = table.Column<string>(type: "text", nullable: true),
                    MessageType = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Payload = table.Column<byte[]>(type: "bytea", nullable: false),
                    SourceAddress = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    EnqueuedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SentAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PublishAttemptCount = table.Column<int>(type: "integer", nullable: false),
                    LastPublishError = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    QuarantinedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_messages", x => x.MessageId);
                });

            migrationBuilder.CreateTable(
                name: "session_analyses",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    TreatmentSessionId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    AnalyticalModelVersion = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    OverallConfidence = table.Column<int>(type: "integer", nullable: false),
                    Interpretation = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TrendSummary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_session_analyses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "session_analysis_derived_metrics",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    SessionAnalysisId = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Value = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Unit = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_session_analysis_derived_metrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_session_analysis_derived_metrics_session_analyses_SessionAn~",
                        column: x => x.SessionAnalysisId,
                        principalTable: "session_analyses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_clinical_analytics_audit_log_OccurredAtUtc",
                table: "clinical_analytics_audit_log",
                column: "OccurredAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_session_analyses_TreatmentSessionId",
                table: "session_analyses",
                column: "TreatmentSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_session_analysis_derived_metrics_SessionAnalysisId",
                table: "session_analysis_derived_metrics",
                column: "SessionAnalysisId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "clinical_analytics_audit_log");

            migrationBuilder.DropTable(
                name: "inbox_states",
                schema: "public");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "public");

            migrationBuilder.DropTable(
                name: "session_analysis_derived_metrics");

            migrationBuilder.DropTable(
                name: "session_analyses");
        }
    }
}
