using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdministrationConfiguration.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialAdministrationConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "administration_configuration_audit_log",
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
                    table.PrimaryKey("PK_administration_configuration_audit_log", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "facility_configurations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    FacilityId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ConfigurationJson = table.Column<string>(type: "text", nullable: false),
                    ConfigurationRevision = table.Column<int>(type: "integer", nullable: false),
                    EffectiveFromUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EffectiveToUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_facility_configurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "feature_toggles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    FeatureKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_feature_toggles", x => x.Id);
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
                name: "rule_sets",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Version = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RulesDocument = table.Column<string>(type: "text", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    PublishedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rule_sets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "threshold_profiles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ProfileCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PayloadJson = table.Column<string>(type: "text", nullable: false),
                    ProfileRevision = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_threshold_profiles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_administration_configuration_audit_log_OccurredAtUtc",
                table: "administration_configuration_audit_log",
                column: "OccurredAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_facility_configurations_FacilityId",
                table: "facility_configurations",
                column: "FacilityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_feature_toggles_FeatureKey",
                table: "feature_toggles",
                column: "FeatureKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "administration_configuration_audit_log");

            migrationBuilder.DropTable(
                name: "facility_configurations");

            migrationBuilder.DropTable(
                name: "feature_toggles");

            migrationBuilder.DropTable(
                name: "inbox_states",
                schema: "public");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "public");

            migrationBuilder.DropTable(
                name: "rule_sets");

            migrationBuilder.DropTable(
                name: "threshold_profiles");
        }
    }
}
