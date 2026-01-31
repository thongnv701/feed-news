using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FeedNews.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRoleTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "created_by",
                table: "role",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created_date",
                table: "role",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<long>(
                name: "updated_by",
                table: "role",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "updated_date",
                table: "role",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_by",
                table: "role");

            migrationBuilder.DropColumn(
                name: "created_date",
                table: "role");

            migrationBuilder.DropColumn(
                name: "updated_by",
                table: "role");

            migrationBuilder.DropColumn(
                name: "updated_date",
                table: "role");
        }
    }
}
