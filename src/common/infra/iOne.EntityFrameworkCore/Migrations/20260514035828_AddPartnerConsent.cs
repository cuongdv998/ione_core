using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class AddPartnerConsent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "partner_consent",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    policy_id = table.Column<Guid>(type: "uuid", nullable: true),
                    consent_type = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false, comment: "Loại điều khoản:\nDATA_SHARING: chia sẻ thông tin cá nhân\nCONTRACT_TERM: điều khoản hợp đồng"),
                    consent_content = table.Column<string>(type: "text", nullable: true),
                    term_version = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    is_accepted = table.Column<bool>(type: "boolean", nullable: false),
                    accepted_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    partner_contract_id = table.Column<Guid>(type: "uuid", nullable: true),
                    creation_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    creator_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modification_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    last_modifier_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_partner_consent", x => x.id);
                },
                comment: "Bảng lưu lịch sử đồng ý điều khoản của đối tác");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "partner_consent");
        }
    }
}
