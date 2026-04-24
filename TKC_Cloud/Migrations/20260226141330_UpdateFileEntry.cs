using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TKC_Cloud.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFileEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Path",
                table: "Files",
                newName: "StoredFileName");

            migrationBuilder.RenameColumn(
                name: "FileName",
                table: "Files",
                newName: "OrginalFileName");

            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "Files",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "Files");

            migrationBuilder.RenameColumn(
                name: "StoredFileName",
                table: "Files",
                newName: "Path");

            migrationBuilder.RenameColumn(
                name: "OrginalFileName",
                table: "Files",
                newName: "FileName");
        }
    }
}
