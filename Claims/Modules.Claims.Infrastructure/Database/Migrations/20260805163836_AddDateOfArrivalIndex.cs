using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Claims.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddDateOfArrivalIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_claim_dates_date_of_arrival",
                schema: "claims",
                table: "claim_dates",
                column: "date_of_arrival");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_claim_dates_date_of_arrival",
                schema: "claims",
                table: "claim_dates");
        }
    }
}
