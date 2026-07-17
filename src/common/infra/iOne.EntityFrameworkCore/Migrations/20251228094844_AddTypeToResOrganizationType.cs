using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class AddTypeToResOrganizationType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "type",
                table: "res_organization_type",
                type: "character varying(15)",
                maxLength: 15,
                nullable: false,
                defaultValue: "TC",
                comment: "Loại tổ chức:\n- TC: Tổ chức\n- CN: Cá nhân");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "type",
                table: "res_organization_type");
        }
    }
}
