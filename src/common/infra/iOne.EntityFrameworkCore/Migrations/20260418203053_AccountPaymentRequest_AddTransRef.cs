using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class AccountPaymentRequest_AddTransRef : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "trans_ref",
                table: "account_payment_request",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                comment: "Mã tham chiếu giao dịch (payment gateway / đối tác)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "trans_ref",
                table: "account_payment_request");
        }
    }
}
