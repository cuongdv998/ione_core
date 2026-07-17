using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class ProProductAttribute : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pro_product_attribute",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    attribute_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_required = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false, defaultValue: "N"),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
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
                    table.PrimaryKey("PK_pro_product_attribute", x => x.id);
                    table.ForeignKey(
                        name: "FK_pro_product_attribute_pro_attribute_attribute_id",
                        column: x => x.attribute_id,
                        principalTable: "pro_attribute",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_pro_product_attribute_pro_product_product_id",
                        column: x => x.product_id,
                        principalTable: "pro_product",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Định nghĩa các đầu vào cần có của Product");

            migrationBuilder.CreateIndex(
                name: "IX_pro_product_attribute_attribute_id",
                table: "pro_product_attribute",
                column: "attribute_id");

            migrationBuilder.CreateIndex(
                name: "IX_pro_product_attribute_product_id",
                table: "pro_product_attribute",
                column: "product_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pro_product_attribute");
        }
    }
}
