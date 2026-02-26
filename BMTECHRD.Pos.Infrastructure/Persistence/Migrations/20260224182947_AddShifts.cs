using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BMTECHRD.Pos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddShifts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ShiftId",
                table: "payments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "shifts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OpenedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OpeningCash = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ClosingCash = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shifts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_payments_ShiftId",
                table: "payments",
                column: "ShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_shifts_BusinessId",
                table: "shifts",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_shifts_UserId_Status",
                table: "shifts",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.AddForeignKey(
                name: "FK_payments_shifts_ShiftId",
                table: "payments",
                column: "ShiftId",
                principalTable: "shifts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_payments_shifts_ShiftId",
                table: "payments");

            migrationBuilder.DropTable(
                name: "shifts");

            migrationBuilder.DropIndex(
                name: "IX_payments_ShiftId",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "ShiftId",
                table: "payments");
        }
    }
}
