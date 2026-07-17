using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class AddMenuConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppMenuConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MenuName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ItemName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ParentItemName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    IsVisible = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppMenuConfigurations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppMenuConfigurations_MenuName",
                table: "AppMenuConfigurations",
                column: "MenuName");

            migrationBuilder.CreateIndex(
                name: "IX_AppMenuConfigurations_MenuName_ItemName",
                table: "AppMenuConfigurations",
                columns: new[] { "MenuName", "ItemName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppMenuConfigurations");
        }
    }
}
