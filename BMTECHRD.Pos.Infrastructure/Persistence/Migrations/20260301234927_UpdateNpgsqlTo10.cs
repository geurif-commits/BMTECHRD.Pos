using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BMTECHRD.Pos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNpgsqlTo10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "order_items",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "order_items");
        }
    }
}
