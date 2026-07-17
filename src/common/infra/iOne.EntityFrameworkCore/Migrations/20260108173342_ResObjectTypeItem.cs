using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class ResObjectTypeItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "res_object_type_item",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ObjectTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ObjectItemType = table.Column<Guid>(type: "uuid", nullable: true),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    UomId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ObjectTypeNavigationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ObjectItemTypeNavigationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_res_object_type_item", x => x.Id);
                    table.ForeignKey(
                        name: "FK_res_object_type_item_res_object_item_type_ObjectItemTypeNav~",
                        column: x => x.ObjectItemTypeNavigationId,
                        principalTable: "res_object_item_type",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_res_object_type_item_res_object_type_ObjectTypeNavigationId",
                        column: x => x.ObjectTypeNavigationId,
                        principalTable: "res_object_type",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_res_object_type_item_res_uom_UomId",
                        column: x => x.UomId,
                        principalTable: "res_uom",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_res_object_type_item_ObjectItemTypeNavigationId",
                table: "res_object_type_item",
                column: "ObjectItemTypeNavigationId");

            migrationBuilder.CreateIndex(
                name: "IX_res_object_type_item_ObjectTypeNavigationId",
                table: "res_object_type_item",
                column: "ObjectTypeNavigationId");

            migrationBuilder.CreateIndex(
                name: "IX_res_object_type_item_UomId",
                table: "res_object_type_item",
                column: "UomId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "res_object_type_item");
        }
    }
}
