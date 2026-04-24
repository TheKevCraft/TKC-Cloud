using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TKC_Cloud.Migrations
{
    /// <inheritdoc />
    public partial class AddFixNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UploadedBystes",
                table: "UploadSessions",
                newName: "UploadedBytes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UploadedBytes",
                table: "UploadSessions",
                newName: "UploadedBystes");
        }
    }
}
