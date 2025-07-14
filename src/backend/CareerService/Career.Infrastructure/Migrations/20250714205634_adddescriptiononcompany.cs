using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Career.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class adddescriptiononcompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "companies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "EmployeersNumber",
                table: "companies",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "EmployeersNumber",
                table: "companies");
        }
    }
}
