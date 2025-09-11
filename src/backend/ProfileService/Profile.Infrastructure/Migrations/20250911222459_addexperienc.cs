using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Profile.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addexperienc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Experience",
                table: "users_skills",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Experience",
                table: "users_skills");
        }
    }
}
