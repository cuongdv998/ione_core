using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class AccountPaymentRequest_AddPaymentProvider : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "payment_provider",
                table: "account_payment_request",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                comment: "Nhà cung cấp thanh toán (VNPAY, MOMO, ...)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "payment_provider",
                table: "account_payment_request");
        }
    }
}
