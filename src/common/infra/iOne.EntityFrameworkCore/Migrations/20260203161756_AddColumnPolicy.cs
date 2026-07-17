using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnPolicy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "policy_id",
                table: "account_payment_request",
                type: "uuid",
                nullable: true,
                comment: "ID đơn bảo hiểm liên quan (thanh toán công nợ)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "policy_id",
                table: "account_payment_request");
        }
    }
}
