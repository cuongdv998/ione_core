using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class AddHrDepartmentType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop table only if it exists (safe for fresh databases)
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""AppMenuConfigurations"";");

            migrationBuilder.CreateTable(
                name: "HrDepartmentType",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, comment: "Trạng thái: - active: hoạt động - deactive: không hoạt động"),
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
                    table.PrimaryKey("PK_HrDepartmentType", x => x.Id);
                },
                comment: "Loại đơn vị: - CT: công ty - CN: chi nhánh - DV: đơn vị - PB: phòng ban");

            migrationBuilder.CreateIndex(
                name: "IX_HrDepartmentType_Code",
                table: "HrDepartmentType",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HrDepartmentType");

            migrationBuilder.CreateTable(
                name: "AppMenuConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: true),
                    IsVisible = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ItemName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    MenuName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    ParentItemName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
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
    }
}
