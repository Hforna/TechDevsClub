using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Career.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class asdkjlf : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "job_applications");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "reviews",
                newName: "ProfileId");

            migrationBuilder.AlterColumn<string>(
                name: "SenderId",
                table: "notifications",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "LocalType",
                table: "jobs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ProfileId",
                table: "job_applications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ResumeName",
                table: "job_applications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_staff_roles_StaffId",
                table: "staff_roles",
                column: "StaffId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_staff_roles_Staffs_StaffId",
                table: "staff_roles",
                column: "StaffId",
                principalTable: "Staffs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_staff_roles_Staffs_StaffId",
                table: "staff_roles");

            migrationBuilder.DropIndex(
                name: "IX_staff_roles_StaffId",
                table: "staff_roles");

            migrationBuilder.DropColumn(
                name: "LocalType",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "ProfileId",
                table: "job_applications");

            migrationBuilder.DropColumn(
                name: "ResumeName",
                table: "job_applications");

            migrationBuilder.RenameColumn(
                name: "ProfileId",
                table: "reviews",
                newName: "UserId");

            migrationBuilder.AlterColumn<string>(
                name: "SenderId",
                table: "notifications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "job_applications",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }
    }
}
