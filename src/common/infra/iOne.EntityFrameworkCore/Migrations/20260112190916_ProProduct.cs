using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class ProProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pro_product",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_type_id = table.Column<Guid>(type: "uuid", nullable: true),
                    partner_id = table.Column<Guid>(type: "uuid", nullable: true),
                    table_rate_id = table.Column<Guid>(type: "uuid", nullable: true),
                    root_product_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_root_product = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false, defaultValue: "Y"),
                    lob_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    currency_id = table.Column<Guid>(type: "uuid", nullable: true),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    insurer_product_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    short_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    internal_note = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    rate_type = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, defaultValue: "table_rate"),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, comment: "Trạng thái: active (hoạt động), deactive (không hoạt động)"),
                    seq_number = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    effect_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    expire_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    plan_definition_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_plan = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: true),
                    extra_properties = table.Column<string>(type: "text", nullable: false),
                    concurrency_stamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    creation_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    creator_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modification_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    last_modifier_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleter_id = table.Column<Guid>(type: "uuid", nullable: true),
                    deletion_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pro_product", x => x.id);
                    table.ForeignKey(
                        name: "FK_pro_product_pro_line_of_business_lob_id",
                        column: x => x.lob_id,
                        principalTable: "pro_line_of_business",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_pro_product_pro_product_category_product_category_id",
                        column: x => x.product_category_id,
                        principalTable: "pro_product_category",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_pro_product_pro_product_root_product_id",
                        column: x => x.root_product_id,
                        principalTable: "pro_product",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_pro_product_pro_product_type_product_type_id",
                        column: x => x.product_type_id,
                        principalTable: "pro_product_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_pro_product_res_currency_currency_id",
                        column: x => x.currency_id,
                        principalTable: "res_currency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_pro_product_res_partner_partner_id",
                        column: x => x.partner_id,
                        principalTable: "res_partner",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Bảng định nghĩa sản phẩm");

            migrationBuilder.CreateTable(
                name: "pro_product_table_rate",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    table_rate_id = table.Column<Guid>(type: "uuid", nullable: false),
                    effect_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    expire_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    extra_properties = table.Column<string>(type: "text", nullable: false),
                    concurrency_stamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    creation_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    creator_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modification_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    last_modifier_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleter_id = table.Column<Guid>(type: "uuid", nullable: true),
                    deletion_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pro_product_table_rate", x => x.id);
                    table.ForeignKey(
                        name: "FK_pro_product_table_rate_pro_product_product_id",
                        column: x => x.product_id,
                        principalTable: "pro_product",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_pro_product_table_rate_pro_table_rate_table_rate_id",
                        column: x => x.table_rate_id,
                        principalTable: "pro_table_rate",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Bảng quan hệ nhiều-nhiều giữa ProProduct và ProTableRate");

            migrationBuilder.CreateIndex(
                name: "ix_pro_product_code",
                table: "pro_product",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pro_product_currency_id",
                table: "pro_product",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "IX_pro_product_lob_id",
                table: "pro_product",
                column: "lob_id");

            migrationBuilder.CreateIndex(
                name: "IX_pro_product_partner_id",
                table: "pro_product",
                column: "partner_id");

            migrationBuilder.CreateIndex(
                name: "IX_pro_product_product_category_id",
                table: "pro_product",
                column: "product_category_id");

            migrationBuilder.CreateIndex(
                name: "IX_pro_product_product_type_id",
                table: "pro_product",
                column: "product_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_pro_product_root_product_id",
                table: "pro_product",
                column: "root_product_id");

            migrationBuilder.CreateIndex(
                name: "IX_pro_product_table_rate_product_id",
                table: "pro_product_table_rate",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_pro_product_table_rate_table_rate_id",
                table: "pro_product_table_rate",
                column: "table_rate_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pro_product_table_rate");

            migrationBuilder.DropTable(
                name: "pro_product");
        }
    }
}
