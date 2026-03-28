using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealtimePlatform.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class OutboxPublishQuarantine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LastPublishError",
                schema: "public",
                table: "outbox_messages",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PublishAttemptCount",
                schema: "public",
                table: "outbox_messages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "QuarantinedAt",
                schema: "public",
                table: "outbox_messages",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastPublishError",
                schema: "public",
                table: "outbox_messages");

            migrationBuilder.DropColumn(
                name: "PublishAttemptCount",
                schema: "public",
                table: "outbox_messages");

            migrationBuilder.DropColumn(
                name: "QuarantinedAt",
                schema: "public",
                table: "outbox_messages");
        }
    }
}
