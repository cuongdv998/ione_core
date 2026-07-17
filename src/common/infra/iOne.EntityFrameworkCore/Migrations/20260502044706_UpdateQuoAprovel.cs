using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class UpdateQuoAprovel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "claim_folder_quotation_approval",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                comment: "Trạng thái: new (mới), inprogress (đang xử lý), pending_approval (chờ duyệt), approved (đã duyệt), rejected (từ chối), done (hoàn thành), cancelled (hủy)",
                oldClrType: typeof(string),
                oldType: "character varying(15)",
                oldMaxLength: 15,
                oldComment: "Trạng thái: new (mới), inprogress (đang xử lý), pending_approval (chờ duyệt), approved (đã duyệt), rejected (từ chối), done (hoàn thành), cancelled (hủy)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "claim_folder_quotation_approval",
                type: "character varying(15)",
                maxLength: 15,
                nullable: false,
                comment: "Trạng thái: new (mới), inprogress (đang xử lý), pending_approval (chờ duyệt), approved (đã duyệt), rejected (từ chối), done (hoàn thành), cancelled (hủy)",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldComment: "Trạng thái: new (mới), inprogress (đang xử lý), pending_approval (chờ duyệt), approved (đã duyệt), rejected (từ chối), done (hoàn thành), cancelled (hủy)");
        }
    }
}
