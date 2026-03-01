using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BMTECHRD.Pos.Infrastructure.Persistence.Migrations
{
    public partial class AddBusinessFiscalAndFiscalDocuments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EnableElectronicInvoice",
                table: "businesses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableFiscalReceipt",
                table: "businesses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableItbis",
                table: "businesses",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "EnableTip",
                table: "businesses",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ItbisRate",
                table: "businesses",
                type: "numeric(6,4)",
                precision: 6,
                scale: 4,
                nullable: false,
                defaultValue: 0.18m);

            migrationBuilder.AddColumn<decimal>(
                name: "TipRate",
                table: "businesses",
                type: "numeric(6,4)",
                precision: 6,
                scale: 4,
                nullable: false,
                defaultValue: 0.10m);

            migrationBuilder.CreateTable(
                name: "fiscal_documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uuid", nullable: false),
                    TableId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Type = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Number = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Subtotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Tax = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Tip = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fiscal_documents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_fiscal_documents_BusinessId_Number",
                table: "fiscal_documents",
                columns: new[] { "BusinessId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_fiscal_documents_TableId",
                table: "fiscal_documents",
                column: "TableId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fiscal_documents");

            migrationBuilder.DropColumn(name: "EnableElectronicInvoice", table: "businesses");
            migrationBuilder.DropColumn(name: "EnableFiscalReceipt", table: "businesses");
            migrationBuilder.DropColumn(name: "EnableItbis", table: "businesses");
            migrationBuilder.DropColumn(name: "EnableTip", table: "businesses");
            migrationBuilder.DropColumn(name: "ItbisRate", table: "businesses");
            migrationBuilder.DropColumn(name: "TipRate", table: "businesses");
        }
    }
}
