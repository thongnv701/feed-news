using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FeedNews.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateKnowledgeBaseTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "analysis_questions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    question = table.Column<string>(type: "text", nullable: false),
                    purpose = table.Column<string>(type: "text", nullable: true),
                    priority = table.Column<int>(type: "integer", nullable: false, defaultValue: 2),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_analysis_questions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "article_analysis_results",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    news_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    original_summary = table.Column<string>(type: "text", nullable: false),
                    enhanced_analysis = table.Column<string>(type: "text", nullable: true),
                    referenced_knowledge = table.Column<string[]>(type: "text[]", nullable: false, defaultValue: new string[0]),
                    source_urls = table.Column<string[]>(type: "text[]", nullable: false, defaultValue: new string[0]),
                    confidence_level = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    questions_answered = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_article_analysis_results", x => x.id);
                    table.ForeignKey(
                        name: "fk_article_analysis_results_news_feeds_news_id",
                        column: x => x.news_id,
                        principalTable: "news_feeds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "knowledge_entries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    topic = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    source_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    confidence_score = table.Column<decimal>(type: "numeric(3,2)", precision: 3, scale: 2, nullable: false),
                    tags = table.Column<string[]>(type: "text[]", nullable: false, defaultValue: new string[0]),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_knowledge_entries", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "knowledge_disputes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    knowledge_entry_id = table.Column<Guid>(type: "uuid", nullable: false),
                    conflicting_entry_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reason = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    resolved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    resolution = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_knowledge_disputes", x => x.id);
                    table.ForeignKey(
                        name: "fk_knowledge_disputes_knowledge_entries_conflicting_entry_id",
                        column: x => x.conflicting_entry_id,
                        principalTable: "knowledge_entries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_knowledge_disputes_knowledge_entries_knowledge_entry_id",
                        column: x => x.knowledge_entry_id,
                        principalTable: "knowledge_entries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_analysis_questions_category",
                table: "analysis_questions",
                column: "category");

            migrationBuilder.CreateIndex(
                name: "ix_article_analysis_results_category",
                table: "article_analysis_results",
                column: "category");

            migrationBuilder.CreateIndex(
                name: "ix_article_analysis_results_news_id",
                table: "article_analysis_results",
                column: "news_id");

            migrationBuilder.CreateIndex(
                name: "ix_knowledge_disputes_conflicting_entry_id",
                table: "knowledge_disputes",
                column: "conflicting_entry_id");

            migrationBuilder.CreateIndex(
                name: "ix_knowledge_disputes_knowledge_entry_id",
                table: "knowledge_disputes",
                column: "knowledge_entry_id");

            migrationBuilder.CreateIndex(
                name: "ix_knowledge_entries_category",
                table: "knowledge_entries",
                column: "category");

            migrationBuilder.CreateIndex(
                name: "ix_knowledge_entries_is_active",
                table: "knowledge_entries",
                column: "is_active");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "analysis_questions");

            migrationBuilder.DropTable(
                name: "article_analysis_results");

            migrationBuilder.DropTable(
                name: "knowledge_disputes");

            migrationBuilder.DropTable(
                name: "knowledge_entries");
        }
    }
}
