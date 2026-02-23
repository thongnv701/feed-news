using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FeedNews.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class inittable2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "content",
                table: "news_feeds",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "content",
                table: "news_feeds");
        }
    }
}
