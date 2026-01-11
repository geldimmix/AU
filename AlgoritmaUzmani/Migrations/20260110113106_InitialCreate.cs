using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AlgoritmaUzmani.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdminUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NameTr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SlugTr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SlugEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DescriptionTr = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DescriptionEn = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Icon = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SeoTags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NameTr = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeoTags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NameTr = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SlugTr = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SlugEn = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Guides",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    TitleTr = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    TitleEn = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    SlugTr = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    SlugEn = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    SummaryTr = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SummaryEn = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ContentTr = table.Column<string>(type: "text", nullable: false),
                    ContentEn = table.Column<string>(type: "text", nullable: true),
                    MetaDescriptionTr = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    MetaDescriptionEn = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    SeoKeywordsTr = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SeoKeywordsEn = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FeaturedImage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FeaturedImageAltTr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    FeaturedImageAltEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsFeatured = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsTranslated = table.Column<bool>(type: "boolean", nullable: false),
                    ViewCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guides", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Guides_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GuideSeoTags",
                columns: table => new
                {
                    GuideId = table.Column<int>(type: "integer", nullable: false),
                    SeoTagId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuideSeoTags", x => new { x.GuideId, x.SeoTagId });
                    table.ForeignKey(
                        name: "FK_GuideSeoTags_Guides_GuideId",
                        column: x => x.GuideId,
                        principalTable: "Guides",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GuideSeoTags_SeoTags_SeoTagId",
                        column: x => x.SeoTagId,
                        principalTable: "SeoTags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GuideTags",
                columns: table => new
                {
                    GuideId = table.Column<int>(type: "integer", nullable: false),
                    TagId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuideTags", x => new { x.GuideId, x.TagId });
                    table.ForeignKey(
                        name: "FK_GuideTags_Guides_GuideId",
                        column: x => x.GuideId,
                        principalTable: "Guides",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GuideTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RelatedGuides",
                columns: table => new
                {
                    GuideId = table.Column<int>(type: "integer", nullable: false),
                    RelatedGuideId = table.Column<int>(type: "integer", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelatedGuides", x => new { x.GuideId, x.RelatedGuideId });
                    table.ForeignKey(
                        name: "FK_RelatedGuides_Guides_GuideId",
                        column: x => x.GuideId,
                        principalTable: "Guides",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RelatedGuides_Guides_RelatedGuideId",
                        column: x => x.RelatedGuideId,
                        principalTable: "Guides",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdminUsers_Username",
                table: "AdminUsers",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_SlugEn",
                table: "Categories",
                column: "SlugEn",
                unique: true,
                filter: "\"SlugEn\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_SlugTr",
                table: "Categories",
                column: "SlugTr",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Guides_CategoryId",
                table: "Guides",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Guides_IsActive",
                table: "Guides",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Guides_IsFeatured",
                table: "Guides",
                column: "IsFeatured");

            migrationBuilder.CreateIndex(
                name: "IX_Guides_SlugEn",
                table: "Guides",
                column: "SlugEn",
                unique: true,
                filter: "\"SlugEn\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Guides_SlugTr",
                table: "Guides",
                column: "SlugTr",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GuideSeoTags_SeoTagId",
                table: "GuideSeoTags",
                column: "SeoTagId");

            migrationBuilder.CreateIndex(
                name: "IX_GuideTags_TagId",
                table: "GuideTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_RelatedGuides_RelatedGuideId",
                table: "RelatedGuides",
                column: "RelatedGuideId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_SlugTr",
                table: "Tags",
                column: "SlugTr",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminUsers");

            migrationBuilder.DropTable(
                name: "GuideSeoTags");

            migrationBuilder.DropTable(
                name: "GuideTags");

            migrationBuilder.DropTable(
                name: "RelatedGuides");

            migrationBuilder.DropTable(
                name: "SeoTags");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "Guides");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
