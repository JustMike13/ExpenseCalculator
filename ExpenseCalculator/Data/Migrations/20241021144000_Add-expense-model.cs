using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpenseCalculator.Data.Migrations
{
    /// <inheritdoc />
    public partial class Addexpensemodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Expense",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PayedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalAmmount = table.Column<float>(type: "real", nullable: false),
                    EquallyDivided = table.Column<bool>(type: "bit", nullable: false),
                    OwnContribution = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expense", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Expense");
        }
    }
}
