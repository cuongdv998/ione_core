using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using iOne.EntityFrameworkCore;

#nullable disable

namespace iOne.Migrations;

/// <summary>
/// pending_approval (17 ký tự) không vừa varchar(15) — mở rộng cột status.
/// </summary>
[DbContext(typeof(iOneDbContext))]
[Migration("20260502104000_ClaimFolderQuotationApproval_Status_Length")]
public partial class ClaimFolderQuotationApproval_Status_Length : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "status",
            table: "claim_folder_quotation_approval",
            type: "character varying(32)",
            maxLength: 32,
            nullable: false,
            comment: "Trạng thái: new (mới), inprogress (đang xử lý), pending_approval (chờ duyệt), approved (đã duyệt), rejected (từ chối), done (hoàn thành), cancelled (hủy)",
            oldClrType: typeof(string),
            oldType: "character varying(15)",
            oldMaxLength: 15);
    }

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
            oldType: "character varying(32)",
            oldMaxLength: 32);
    }
}
