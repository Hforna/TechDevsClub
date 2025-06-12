using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Profile.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeConnectionToProfileAsConnector : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Connections_AspNetUsers_ConnectedId",
                table: "Connections");

            migrationBuilder.DropForeignKey(
                name: "FK_Connections_AspNetUsers_ConnectorId",
                table: "Connections");

            migrationBuilder.AddForeignKey(
                name: "FK_Connections_profiles_ConnectedId",
                table: "Connections",
                column: "ConnectedId",
                principalTable: "profiles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Connections_profiles_ConnectorId",
                table: "Connections",
                column: "ConnectorId",
                principalTable: "profiles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Connections_profiles_ConnectedId",
                table: "Connections");

            migrationBuilder.DropForeignKey(
                name: "FK_Connections_profiles_ConnectorId",
                table: "Connections");

            migrationBuilder.AddForeignKey(
                name: "FK_Connections_AspNetUsers_ConnectedId",
                table: "Connections",
                column: "ConnectedId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Connections_AspNetUsers_ConnectorId",
                table: "Connections",
                column: "ConnectorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
