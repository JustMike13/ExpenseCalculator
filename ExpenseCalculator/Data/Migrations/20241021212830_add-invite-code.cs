using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpenseCalculator.Data.Migrations
{
    /// <inheritdoc />
    public partial class addinvitecode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InviteCode",
                table: "Trip",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InviteCode",
                table: "Trip");
        }
    }
}
