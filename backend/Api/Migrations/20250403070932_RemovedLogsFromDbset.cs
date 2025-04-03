using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class RemovedLogsFromDbset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TemperatureLogs_Devices_DeviceId",
                table: "TemperatureLogs");

            migrationBuilder.AlterColumn<int>(
                name: "DeviceId",
                table: "TemperatureLogs",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_TemperatureLogs_Devices_DeviceId",
                table: "TemperatureLogs",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TemperatureLogs_Devices_DeviceId",
                table: "TemperatureLogs");

            migrationBuilder.AlterColumn<int>(
                name: "DeviceId",
                table: "TemperatureLogs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TemperatureLogs_Devices_DeviceId",
                table: "TemperatureLogs",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
