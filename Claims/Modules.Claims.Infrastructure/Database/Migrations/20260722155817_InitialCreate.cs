using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Claims.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "claims");

            migrationBuilder.CreateTable(
                name: "customers",
                schema: "claims",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    akio_number = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_customers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "suppliers",
                schema: "claims",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    supplier_akio_number = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_suppliers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "claims",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "text", nullable: false),
                    last_name = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    role = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "bookings",
                schema: "claims",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    booking_number = table.Column<string>(type: "text", nullable: false),
                    sales_channel = table.Column<int>(type: "integer", nullable: true),
                    language = table.Column<int>(type: "integer", nullable: true),
                    season_label = table.Column<string>(type: "text", nullable: false),
                    season_value = table.Column<string>(type: "text", nullable: false),
                    service = table.Column<int>(type: "integer", nullable: true),
                    skissim = table.Column<bool>(type: "boolean", nullable: true),
                    skissim_type = table.Column<int>(type: "integer", nullable: true),
                    product = table.Column<string>(type: "text", nullable: true),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    supplier_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bookings", x => x.id);
                    table.ForeignKey(
                        name: "fk_bookings_customers_customer_id",
                        column: x => x.customer_id,
                        principalSchema: "claims",
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_bookings_suppliers_supplier_id",
                        column: x => x.supplier_id,
                        principalSchema: "claims",
                        principalTable: "suppliers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "claims",
                schema: "claims",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    state = table.Column<int>(type: "integer", nullable: false),
                    followed_by = table.Column<string>(type: "text", nullable: true),
                    reason = table.Column<string>(type: "text", nullable: true),
                    claim_summary = table.Column<string>(type: "text", nullable: true),
                    solution = table.Column<int>(type: "integer", nullable: true),
                    purpose_of_solution = table.Column<string>(type: "text", nullable: true),
                    update_reason = table.Column<string>(type: "text", nullable: true),
                    customer_supp_info = table.Column<string>(type: "text", nullable: true),
                    supplier_supp_info = table.Column<string>(type: "text", nullable: true),
                    booking_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_claims", x => x.id);
                    table.ForeignKey(
                        name: "fk_claims_bookings_booking_id",
                        column: x => x.booking_id,
                        principalSchema: "claims",
                        principalTable: "bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "claim_dates",
                schema: "claims",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_of_received_claim = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    date_of_start_follow_up = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    date_last_update = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    date_of_departure = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    date_end_of_follow_up = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    date_of_arrival = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    claim_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_claim_dates", x => x.id);
                    table.ForeignKey(
                        name: "fk_claim_dates_claims_claim_id",
                        column: x => x.claim_id,
                        principalSchema: "claims",
                        principalTable: "claims",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compensations",
                schema: "claims",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_voucher = table.Column<float>(type: "real", nullable: true),
                    customer_used_voucher = table.Column<float>(type: "real", nullable: true),
                    supplier_refund = table.Column<float>(type: "real", nullable: true),
                    claim_refund = table.Column<float>(type: "real", nullable: true),
                    refund_state = table.Column<int>(type: "integer", nullable: true),
                    claim_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_compensations", x => x.id);
                    table.ForeignKey(
                        name: "fk_compensations_claims_claim_id",
                        column: x => x.claim_id,
                        principalSchema: "claims",
                        principalTable: "claims",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_bookings_customer_id",
                schema: "claims",
                table: "bookings",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_bookings_supplier_id",
                schema: "claims",
                table: "bookings",
                column: "supplier_id");

            migrationBuilder.CreateIndex(
                name: "ix_claim_dates_claim_id",
                schema: "claims",
                table: "claim_dates",
                column: "claim_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_claim_dates_date_of_received_claim",
                schema: "claims",
                table: "claim_dates",
                column: "date_of_received_claim");

            migrationBuilder.CreateIndex(
                name: "ix_claims_booking_id",
                schema: "claims",
                table: "claims",
                column: "booking_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_compensations_claim_id",
                schema: "claims",
                table: "compensations",
                column: "claim_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                schema: "claims",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "claim_dates",
                schema: "claims");

            migrationBuilder.DropTable(
                name: "compensations",
                schema: "claims");

            migrationBuilder.DropTable(
                name: "users",
                schema: "claims");

            migrationBuilder.DropTable(
                name: "claims",
                schema: "claims");

            migrationBuilder.DropTable(
                name: "bookings",
                schema: "claims");

            migrationBuilder.DropTable(
                name: "customers",
                schema: "claims");

            migrationBuilder.DropTable(
                name: "suppliers",
                schema: "claims");
        }
    }
}
