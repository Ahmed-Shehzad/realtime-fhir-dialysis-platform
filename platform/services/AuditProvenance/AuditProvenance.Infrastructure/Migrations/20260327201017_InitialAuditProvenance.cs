using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuditProvenance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialAuditProvenance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

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
                name: "platform_audit_facts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EventType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    DetailJson = table.Column<string>(type: "text", nullable: true),
                    CorrelationId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CausationId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TenantId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ActorId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    SourceSystem = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    RelatedResourceType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    RelatedResourceId = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    SessionId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    PatientId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_audit_facts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "security_audit_log",
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
                    table.PrimaryKey("PK_security_audit_log", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "provenance_links",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    FromPlatformAuditFactId = table.Column<string>(type: "text", nullable: false),
                    ToPlatformAuditFactId = table.Column<string>(type: "text", nullable: false),
                    RelationType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_provenance_links", x => x.Id);
                    table.ForeignKey(
                        name: "FK_provenance_links_platform_audit_facts_FromPlatformAuditFact~",
                        column: x => x.FromPlatformAuditFactId,
                        principalTable: "platform_audit_facts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_provenance_links_platform_audit_facts_ToPlatformAuditFactId",
                        column: x => x.ToPlatformAuditFactId,
                        principalTable: "platform_audit_facts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_platform_audit_facts_OccurredAtUtc",
                table: "platform_audit_facts",
                column: "OccurredAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_provenance_links_FromPlatformAuditFactId",
                table: "provenance_links",
                column: "FromPlatformAuditFactId");

            migrationBuilder.CreateIndex(
                name: "IX_provenance_links_ToPlatformAuditFactId",
                table: "provenance_links",
                column: "ToPlatformAuditFactId");

            migrationBuilder.CreateIndex(
                name: "IX_security_audit_log_OccurredAtUtc",
                table: "security_audit_log",
                column: "OccurredAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inbox_states",
                schema: "public");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "public");

            migrationBuilder.DropTable(
                name: "provenance_links");

            migrationBuilder.DropTable(
                name: "security_audit_log");

            migrationBuilder.DropTable(
                name: "platform_audit_facts");
        }
    }
}
