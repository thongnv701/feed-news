using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FeedNews.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "game",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    logo_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_game", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "role",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "system_config",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    popup_message = table.Column<string>(type: "text", nullable: false),
                    announce_message = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_system_config", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "wallet",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    available_amount = table.Column<decimal>(type: "numeric(8,2)", nullable: false),
                    incoming_amount = table.Column<decimal>(type: "numeric(8,2)", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_wallet", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "attribute",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    game_id = table.Column<long>(type: "bigint", nullable: false),
                    key = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    value = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    image_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_attribute", x => x.id);
                    table.ForeignKey(
                        name: "fk_attribute_game",
                        column: x => x.game_id,
                        principalTable: "game",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "game_account",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    game_id = table.Column<long>(type: "bigint", nullable: false),
                    user_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    password = table.Column<string>(type: "text", nullable: false),
                    price = table.Column<decimal>(type: "numeric(8,2)", nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    image_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    attributes_json = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_game_account", x => x.id);
                    table.ForeignKey(
                        name: "fk_gameaccount_game",
                        column: x => x.game_id,
                        principalTable: "game",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "account",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    password = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    avatar_url = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    full_name = table.Column<string>(type: "text", nullable: true),
                    gender = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    device_token = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    num_of_flag = table.Column<int>(type: "integer", nullable: false),
                    role_id = table.Column<long>(type: "bigint", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_account", x => x.id);
                    table.ForeignKey(
                        name: "fk_account_role",
                        column: x => x.role_id,
                        principalTable: "role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "customer",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    wallet_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_customer", x => x.id);
                    table.ForeignKey(
                        name: "fk_customer_wallet",
                        column: x => x.wallet_id,
                        principalTable: "wallet",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "wallet_transaction",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    wallet_id = table.Column<long>(type: "bigint", nullable: false),
                    available_amount_before = table.Column<decimal>(type: "numeric(8,2)", nullable: false),
                    incoming_amount_before = table.Column<decimal>(type: "numeric(8,2)", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(8,2)", nullable: false),
                    type = table.Column<short>(type: "smallint", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_wallet_transaction", x => x.id);
                    table.ForeignKey(
                        name: "fk_wallettransaction_wallet",
                        column: x => x.wallet_id,
                        principalTable: "wallet",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "withdraw_request",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    wallet_id = table.Column<long>(type: "bigint", nullable: false),
                    bank_code = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    bank_short_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    bank_account_number = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    bank_account_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(8,2)", nullable: false),
                    reason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_withdraw_request", x => x.id);
                    table.ForeignKey(
                        name: "fk_withdrawrequest_wallet",
                        column: x => x.wallet_id,
                        principalTable: "wallet",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "gaccount_attribute",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    attribute_id = table.Column<long>(type: "bigint", nullable: false),
                    g_account_id = table.Column<long>(type: "bigint", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    is_special = table.Column<bool>(type: "boolean", nullable: false),
                    level = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_gaccount_attribute", x => x.id);
                    table.ForeignKey(
                        name: "fk_gaccountattribute_attribute",
                        column: x => x.attribute_id,
                        principalTable: "attribute",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_gaccountattribute_gameaccount",
                        column: x => x.g_account_id,
                        principalTable: "game_account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "favourite",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    g_account_id = table.Column<long>(type: "bigint", nullable: false),
                    customer_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_favourite", x => x.id);
                    table.ForeignKey(
                        name: "fk_favourite_customer",
                        column: x => x.customer_id,
                        principalTable: "account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_favourite_gameaccount",
                        column: x => x.g_account_id,
                        principalTable: "game_account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notification",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    account_id = table.Column<long>(type: "bigint", nullable: false),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    data = table.Column<string>(type: "text", nullable: true),
                    is_read = table.Column<bool>(type: "boolean", nullable: false),
                    type = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notification", x => x.id);
                    table.ForeignKey(
                        name: "fk_notification_account",
                        column: x => x.account_id,
                        principalTable: "account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    customer_id = table.Column<long>(type: "bigint", nullable: false),
                    g_account_id = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    price = table.Column<decimal>(type: "numeric(8,2)", nullable: false),
                    promotion = table.Column<decimal>(type: "numeric(8,2)", nullable: false),
                    order_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    complete_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_refund = table.Column<bool>(type: "boolean", nullable: false),
                    is_report = table.Column<bool>(type: "boolean", nullable: false),
                    reason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order", x => x.id);
                    table.ForeignKey(
                        name: "fk_order_customer",
                        column: x => x.customer_id,
                        principalTable: "customer",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_order_gameaccount",
                        column: x => x.g_account_id,
                        principalTable: "game_account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payment",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_id = table.Column<long>(type: "bigint", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(8,2)", nullable: false),
                    status = table.Column<long>(type: "bigint", nullable: false),
                    type = table.Column<long>(type: "bigint", nullable: false),
                    transaction_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    transaction_content = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment", x => x.id);
                    table.ForeignKey(
                        name: "fk_payment_order",
                        column: x => x.order_id,
                        principalTable: "order",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_account_email",
                table: "account",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_account_phone_number",
                table: "account",
                column: "phone_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_account_role_id",
                table: "account",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_attribute_game_id",
                table: "attribute",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "ix_customer_wallet_id",
                table: "customer",
                column: "wallet_id");

            migrationBuilder.CreateIndex(
                name: "ix_favourite_customer_id",
                table: "favourite",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_favourite_g_account_id",
                table: "favourite",
                column: "g_account_id");

            migrationBuilder.CreateIndex(
                name: "ix_gaccount_attribute_attribute_id",
                table: "gaccount_attribute",
                column: "attribute_id");

            migrationBuilder.CreateIndex(
                name: "ix_gaccount_attribute_g_account_id",
                table: "gaccount_attribute",
                column: "g_account_id");

            migrationBuilder.CreateIndex(
                name: "ix_game_account_game_id",
                table: "game_account",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "ix_notification_account_id",
                table: "notification",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_customer_id",
                table: "order",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_g_account_id",
                table: "order",
                column: "g_account_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_order_id",
                table: "payment",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_wallet_transaction_wallet_id",
                table: "wallet_transaction",
                column: "wallet_id");

            migrationBuilder.CreateIndex(
                name: "ix_withdraw_request_wallet_id",
                table: "withdraw_request",
                column: "wallet_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "favourite");

            migrationBuilder.DropTable(
                name: "gaccount_attribute");

            migrationBuilder.DropTable(
                name: "notification");

            migrationBuilder.DropTable(
                name: "payment");

            migrationBuilder.DropTable(
                name: "system_config");

            migrationBuilder.DropTable(
                name: "wallet_transaction");

            migrationBuilder.DropTable(
                name: "withdraw_request");

            migrationBuilder.DropTable(
                name: "attribute");

            migrationBuilder.DropTable(
                name: "account");

            migrationBuilder.DropTable(
                name: "order");

            migrationBuilder.DropTable(
                name: "role");

            migrationBuilder.DropTable(
                name: "customer");

            migrationBuilder.DropTable(
                name: "game_account");

            migrationBuilder.DropTable(
                name: "wallet");

            migrationBuilder.DropTable(
                name: "game");
        }
    }
}
