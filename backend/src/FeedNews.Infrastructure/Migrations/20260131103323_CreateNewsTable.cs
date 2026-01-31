using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FeedNews.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateNewsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "news_feeds",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    title = table.Column<string>(type: "varchar(500)", nullable: false),
                    url = table.Column<string>(type: "varchar(2000)", nullable: false),
                    summary = table.Column<string>(type: "varchar(2000)", nullable: true),
                    source = table.Column<string>(type: "varchar(50)", nullable: false),
                    category = table.Column<string>(type: "varchar(50)", nullable: false),
                    published_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fetched_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ranking_score = table.Column<decimal>(type: "numeric(10,2)", nullable: false, defaultValue: 0m),
                    slack_sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_news_feeds_id", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_news_feeds_category_pubdate",
                table: "news_feeds",
                columns: new[] { "category", "published_date" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "idx_news_feeds_fetched_slack",
                table: "news_feeds",
                columns: new[] { "fetched_at", "slack_sent_at" });

            migrationBuilder.CreateIndex(
                name: "uq_news_feeds_url",
                table: "news_feeds",
                column: "url",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "news_feeds");
        }
    }
}
