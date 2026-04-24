using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TKC_Cloud.Migrations
{
    /// <inheritdoc />
    public partial class AddChunkTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ChunkSize",
                table: "UploadSessions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalChunks",
                table: "UploadSessions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "uploadedChunks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UploadSessionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ChunkIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_uploadedChunks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_uploadedChunks_UploadSessions_UploadSessionId",
                        column: x => x.UploadSessionId,
                        principalTable: "UploadSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_uploadedChunks_UploadSessionId_ChunkIndex",
                table: "uploadedChunks",
                columns: new[] { "UploadSessionId", "ChunkIndex" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "uploadedChunks");

            migrationBuilder.DropColumn(
                name: "ChunkSize",
                table: "UploadSessions");

            migrationBuilder.DropColumn(
                name: "TotalChunks",
                table: "UploadSessions");
        }
    }
}
