using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class AddResPartner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "res_partner",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    channel_id = table.Column<Guid>(type: "uuid", nullable: true),
                    partner_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    partner_role = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    organization_type_id = table.Column<Guid>(type: "uuid", nullable: true),
                    province_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ward_id = table.Column<Guid>(type: "uuid", nullable: false),
                    invoice_province_id = table.Column<Guid>(type: "uuid", nullable: true),
                    invoice_ward_id = table.Column<Guid>(type: "uuid", nullable: true),
                    code = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true),
                    name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    address = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    full_address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    email = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    phone = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, comment: "Trạng thái:\n- active: hoạt động\n- deactive: không hoạt động"),
                    invoice_address = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    invoice_full_address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    id_no = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true),
                    tin = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    rep_name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    rep_email = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    rep_phone = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    rep_id_no = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true),
                    rep_title = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    authorizer = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    authorizer_phone = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    authorizer_email = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    authorizer_no = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true),
                    authorizer_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    authorizer_title = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    business_no = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true),
                    extra_properties = table.Column<string>(type: "text", nullable: false),
                    concurrency_stamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    creation_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    creator_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modification_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    last_modifier_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleter_id = table.Column<Guid>(type: "uuid", nullable: true),
                    deletion_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_res_partner", x => x.id);
                    table.ForeignKey(
                        name: "fk_res_partner_res_channel_channel_id",
                        column: x => x.channel_id,
                        principalTable: "res_channel",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_res_partner_res_organization_type_organization_type_id",
                        column: x => x.organization_type_id,
                        principalTable: "res_organization_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_res_partner_res_partner_type_partner_type_id",
                        column: x => x.partner_type_id,
                        principalTable: "res_partner_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_res_partner_res_province_invoice_province_id",
                        column: x => x.invoice_province_id,
                        principalTable: "res_province",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_res_partner_res_province_province_id",
                        column: x => x.province_id,
                        principalTable: "res_province",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_res_partner_res_ward_invoice_ward_id",
                        column: x => x.invoice_ward_id,
                        principalTable: "res_ward",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_res_partner_res_ward_ward_id",
                        column: x => x.ward_id,
                        principalTable: "res_ward",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Bảng thông tin đối tác");

            migrationBuilder.CreateTable(
                name: "res_partner_agreement",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    partner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    agreement_term_id = table.Column<Guid>(type: "uuid", nullable: false),
                    value = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    effect_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    expire_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    creation_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    creator_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modification_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    last_modifier_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_res_partner_agreement", x => x.id);
                    table.ForeignKey(
                        name: "fk_res_partner_agreement_res_agreement_term_agreement_term_id",
                        column: x => x.agreement_term_id,
                        principalTable: "res_agreement_term",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_res_partner_agreement_res_partner_partner_id",
                        column: x => x.partner_id,
                        principalTable: "res_partner",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Định nghĩa một số thỏa thuận với Cty bảo hiểm gốc");

            migrationBuilder.CreateIndex(
                name: "ix_res_partner_channel_id",
                table: "res_partner",
                column: "channel_id");

            migrationBuilder.CreateIndex(
                name: "ix_res_partner_code",
                table: "res_partner",
                column: "code",
                unique: true,
                filter: "\"code\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_res_partner_invoice_province_id",
                table: "res_partner",
                column: "invoice_province_id");

            migrationBuilder.CreateIndex(
                name: "ix_res_partner_invoice_ward_id",
                table: "res_partner",
                column: "invoice_ward_id");

            migrationBuilder.CreateIndex(
                name: "ix_res_partner_organization_type_id",
                table: "res_partner",
                column: "organization_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_res_partner_partner_type_id",
                table: "res_partner",
                column: "partner_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_res_partner_province_id",
                table: "res_partner",
                column: "province_id");

            migrationBuilder.CreateIndex(
                name: "ix_res_partner_ward_id",
                table: "res_partner",
                column: "ward_id");

            migrationBuilder.CreateIndex(
                name: "ix_res_partner_agreement_agreement_term_id",
                table: "res_partner_agreement",
                column: "agreement_term_id");

            migrationBuilder.CreateIndex(
                name: "ix_res_partner_agreement_partner_id",
                table: "res_partner_agreement",
                column: "partner_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "res_partner_agreement");

            migrationBuilder.DropTable(
                name: "res_partner");
        }
    }
}
