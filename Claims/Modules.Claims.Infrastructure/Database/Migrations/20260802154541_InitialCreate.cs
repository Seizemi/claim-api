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
                name: "compensation_reasons",
                schema: "claims",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    label = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_compensation_reasons", x => x.id);
                });

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
                name: "followed_bies",
                schema: "claims",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    label = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_followed_bies", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "reasons",
                schema: "claims",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    label = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reasons", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "refund_states",
                schema: "claims",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    label = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_refund_states", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sales_channels",
                schema: "claims",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    label = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: false),
                    language = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sales_channels", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "services",
                schema: "claims",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    label = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_services", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "skissim_types",
                schema: "claims",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    label = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_skissim_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "solutions",
                schema: "claims",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    label = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_solutions", x => x.id);
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
                name: "suppliers",
                schema: "claims",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    label = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: false),
                    supplier_akio_number = table.Column<int>(type: "integer", nullable: false),
                    service_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_suppliers", x => x.id);
                    table.ForeignKey(
                        name: "fk_suppliers_services_service_id",
                        column: x => x.service_id,
                        principalSchema: "claims",
                        principalTable: "services",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "bookings",
                schema: "claims",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    booking_number = table.Column<string>(type: "text", nullable: false),
                    sales_channel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    skissim_type_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                        name: "fk_bookings_sales_channels_sales_channel_id",
                        column: x => x.sales_channel_id,
                        principalSchema: "claims",
                        principalTable: "sales_channels",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_bookings_skissim_types_skissim_type_id",
                        column: x => x.skissim_type_id,
                        principalSchema: "claims",
                        principalTable: "skissim_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
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
                    followed_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reason_id = table.Column<Guid>(type: "uuid", nullable: false),
                    claim_summary = table.Column<string>(type: "text", nullable: true),
                    solution_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.ForeignKey(
                        name: "fk_claims_followed_bies_followed_by_id",
                        column: x => x.followed_by_id,
                        principalSchema: "claims",
                        principalTable: "followed_bies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_claims_reasons_reason_id",
                        column: x => x.reason_id,
                        principalSchema: "claims",
                        principalTable: "reasons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_claims_solutions_solution_id",
                        column: x => x.solution_id,
                        principalSchema: "claims",
                        principalTable: "solutions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "claim_dates",
                schema: "claims",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_of_received_claim = table.Column<DateOnly>(type: "date", nullable: true),
                    date_of_start_follow_up = table.Column<DateOnly>(type: "date", nullable: true),
                    date_last_update = table.Column<DateOnly>(type: "date", nullable: true),
                    date_of_departure = table.Column<DateOnly>(type: "date", nullable: true),
                    date_end_of_follow_up = table.Column<DateOnly>(type: "date", nullable: true),
                    date_of_arrival = table.Column<DateOnly>(type: "date", nullable: true),
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
                    refund_state_id = table.Column<Guid>(type: "uuid", nullable: false),
                    compensation_reason_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.ForeignKey(
                        name: "fk_compensations_compensation_reasons_compensation_reason_id",
                        column: x => x.compensation_reason_id,
                        principalSchema: "claims",
                        principalTable: "compensation_reasons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_compensations_refund_states_refund_state_id",
                        column: x => x.refund_state_id,
                        principalSchema: "claims",
                        principalTable: "refund_states",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_bookings_customer_id",
                schema: "claims",
                table: "bookings",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_bookings_sales_channel_id",
                schema: "claims",
                table: "bookings",
                column: "sales_channel_id");

            migrationBuilder.CreateIndex(
                name: "ix_bookings_skissim_type_id",
                schema: "claims",
                table: "bookings",
                column: "skissim_type_id");

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
                name: "ix_claims_followed_by_id",
                schema: "claims",
                table: "claims",
                column: "followed_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_claims_reason_id",
                schema: "claims",
                table: "claims",
                column: "reason_id");

            migrationBuilder.CreateIndex(
                name: "ix_claims_solution_id",
                schema: "claims",
                table: "claims",
                column: "solution_id");

            migrationBuilder.CreateIndex(
                name: "ix_compensations_claim_id",
                schema: "claims",
                table: "compensations",
                column: "claim_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_compensations_compensation_reason_id",
                schema: "claims",
                table: "compensations",
                column: "compensation_reason_id");

            migrationBuilder.CreateIndex(
                name: "ix_compensations_refund_state_id",
                schema: "claims",
                table: "compensations",
                column: "refund_state_id");

            migrationBuilder.CreateIndex(
                name: "ix_suppliers_service_id",
                schema: "claims",
                table: "suppliers",
                column: "service_id");

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
                name: "compensation_reasons",
                schema: "claims");

            migrationBuilder.DropTable(
                name: "refund_states",
                schema: "claims");

            migrationBuilder.DropTable(
                name: "bookings",
                schema: "claims");

            migrationBuilder.DropTable(
                name: "followed_bies",
                schema: "claims");

            migrationBuilder.DropTable(
                name: "reasons",
                schema: "claims");

            migrationBuilder.DropTable(
                name: "solutions",
                schema: "claims");

            migrationBuilder.DropTable(
                name: "customers",
                schema: "claims");

            migrationBuilder.DropTable(
                name: "sales_channels",
                schema: "claims");

            migrationBuilder.DropTable(
                name: "skissim_types",
                schema: "claims");

            migrationBuilder.DropTable(
                name: "suppliers",
                schema: "claims");

            migrationBuilder.DropTable(
                name: "services",
                schema: "claims");
        }
    }
}
