using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Career.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addCompaniesMod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmployeersNumber",
                table: "companies");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EmployeersNumber",
                table: "companies",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
