using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Claims.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddClaimEditLock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "locked_at",
                schema: "claims",
                table: "claims",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "locked_by_user_id",
                schema: "claims",
                table: "claims",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "locked_by_user_name",
                schema: "claims",
                table: "claims",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "locked_at",
                schema: "claims",
                table: "claims");

            migrationBuilder.DropColumn(
                name: "locked_by_user_id",
                schema: "claims",
                table: "claims");

            migrationBuilder.DropColumn(
                name: "locked_by_user_name",
                schema: "claims",
                table: "claims");
        }
    }
}
