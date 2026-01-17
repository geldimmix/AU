using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AlgoritmaUzmani.Migrations
{
    /// <inheritdoc />
    public partial class AddCodeBlocks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CodeBlocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuideId = table.Column<int>(type: "integer", nullable: false),
                    BlockId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SourceLanguage = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SourceCode = table.Column<string>(type: "text", nullable: false),
                    Translations = table.Column<string>(type: "jsonb", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodeBlocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CodeBlocks_Guides_GuideId",
                        column: x => x.GuideId,
                        principalTable: "Guides",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CodeBlocks_GuideId",
                table: "CodeBlocks",
                column: "GuideId");

            migrationBuilder.CreateIndex(
                name: "IX_CodeBlocks_GuideId_BlockId",
                table: "CodeBlocks",
                columns: new[] { "GuideId", "BlockId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CodeBlocks_GuideId_DisplayOrder",
                table: "CodeBlocks",
                columns: new[] { "GuideId", "DisplayOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CodeBlocks");
        }
    }
}
