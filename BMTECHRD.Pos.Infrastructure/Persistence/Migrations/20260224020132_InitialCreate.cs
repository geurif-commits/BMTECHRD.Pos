using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BMTECHRD.Pos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "businesses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(140)", maxLength: 140, nullable: false),
                    LogoPath = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    CurrencyCode = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_businesses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "licenses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uuid", nullable: false),
                    Plan = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ActivationKey = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    ActivatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_licenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_licenses_businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    PinHash = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    BusinessId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_users_businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tables",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    OpenedByWaiterId = table.Column<Guid>(type: "uuid", nullable: true),
                    OpenedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PosX = table.Column<double>(type: "double precision", nullable: false),
                    PosY = table.Column<double>(type: "double precision", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tables_businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tables_users_OpenedByWaiterId",
                        column: x => x.OpenedByWaiterId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_licenses_ActivationKey",
                table: "licenses",
                column: "ActivationKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_licenses_BusinessId",
                table: "licenses",
                column: "BusinessId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tables_BusinessId_Number",
                table: "tables",
                columns: new[] { "BusinessId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tables_OpenedByWaiterId",
                table: "tables",
                column: "OpenedByWaiterId");

            migrationBuilder.CreateIndex(
                name: "IX_users_BusinessId_Username",
                table: "users",
                columns: new[] { "BusinessId", "Username" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "licenses");

            migrationBuilder.DropTable(
                name: "tables");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "businesses");
        }
    }
}
