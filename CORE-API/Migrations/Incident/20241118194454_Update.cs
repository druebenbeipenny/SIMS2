using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CORE_API.Migrations.Incident
{
    /// <inheritdoc />
    public partial class Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "updatedAt",
                table: "Incidents",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "title",
                table: "Incidents",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "Incidents",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "severity",
                table: "Incidents",
                newName: "Severity");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "Incidents",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "creatorId",
                table: "Incidents",
                newName: "CreatorId");

            migrationBuilder.RenameColumn(
                name: "createdAt",
                table: "Incidents",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "assignedUserId",
                table: "Incidents",
                newName: "AssignedUserId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Incidents",
                newName: "Id");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Incidents",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Incidents",
                newName: "updatedAt");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Incidents",
                newName: "title");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Incidents",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Severity",
                table: "Incidents",
                newName: "severity");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Incidents",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "CreatorId",
                table: "Incidents",
                newName: "creatorId");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Incidents",
                newName: "createdAt");

            migrationBuilder.RenameColumn(
                name: "AssignedUserId",
                table: "Incidents",
                newName: "assignedUserId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Incidents",
                newName: "id");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "Incidents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
