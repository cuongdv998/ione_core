using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class AddPolicyAmount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "policy_amount",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    policy_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Tham chiếu đến đơn bảo hiểm"),
                    policy_version_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Tham chiếu đến phiên bản đơn bảo hiểm"),
                    fee_item_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Tham chiếu đến khoản phí"),
                    issue_date = table.Column<DateTime>(type: "DATE", nullable: false, comment: "Ngày phát hành"),
                    amount_total = table.Column<decimal>(type: "numeric(15,3)", nullable: false, comment: "Tổng số tiền"),
                    amount = table.Column<decimal>(type: "numeric(15,3)", nullable: false, comment: "Số tiền"),
                    vat = table.Column<decimal>(type: "numeric(15,3)", nullable: false, comment: "VAT"),
                    payment_status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, defaultValue: "new", comment: "Trạng thái thanh toán: new, paid, partial, cancelled"),
                    payment_method_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tham chiếu đến hình thức thanh toán"),
                    payment_date = table.Column<DateTime>(type: "DATE", nullable: true, comment: "Ngày thanh toán"),
                    PolicyVersionId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    FeeItemId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    PaymentMethodId1 = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_policy_amount", x => x.id);
                    table.ForeignKey(
                        name: "FK_policy_amount_policy_version_PolicyVersionId1",
                        column: x => x.PolicyVersionId1,
                        principalTable: "policy_version",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_policy_amount_res_fee_item_FeeItemId1",
                        column: x => x.FeeItemId1,
                        principalTable: "res_fee_item",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_policy_amount_res_payment_method_PaymentMethodId1",
                        column: x => x.PaymentMethodId1,
                        principalTable: "res_payment_method",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_policy_amount_policy_id",
                        column: x => x.policy_id,
                        principalTable: "policy",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_policy_amount_policy_version_id",
                        column: x => x.policy_version_id,
                        principalTable: "policy_version",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_policy_amount_res_fee_item_id",
                        column: x => x.fee_item_id,
                        principalTable: "res_fee_item",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_policy_amount_res_payment_method_id",
                        column: x => x.payment_method_id,
                        principalTable: "res_payment_method",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Số tiền thanh toán của đơn bảo hiểm");

            migrationBuilder.CreateTable(
                name: "policy_document",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    policy_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tham chiếu đến đơn bảo hiểm gốc"),
                    document_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tham chiếu đến res_document"),
                    DocumentId1 = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_policy_document", x => x.id);
                    table.ForeignKey(
                        name: "FK_policy_document_res_document_DocumentId1",
                        column: x => x.DocumentId1,
                        principalTable: "res_document",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_policy_document_policy_id",
                        column: x => x.policy_id,
                        principalTable: "policy",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_policy_document_res_document_id",
                        column: x => x.document_id,
                        principalTable: "res_document",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Tài liệu đính kèm đơn bảo hiểm");

            migrationBuilder.CreateIndex(
                name: "ix_policy_amount_fee_item_id",
                table: "policy_amount",
                column: "fee_item_id");

            migrationBuilder.CreateIndex(
                name: "IX_policy_amount_FeeItemId1",
                table: "policy_amount",
                column: "FeeItemId1");

            migrationBuilder.CreateIndex(
                name: "ix_policy_amount_payment_method_id",
                table: "policy_amount",
                column: "payment_method_id");

            migrationBuilder.CreateIndex(
                name: "IX_policy_amount_PaymentMethodId1",
                table: "policy_amount",
                column: "PaymentMethodId1");

            migrationBuilder.CreateIndex(
                name: "ix_policy_amount_policy_id",
                table: "policy_amount",
                column: "policy_id");

            migrationBuilder.CreateIndex(
                name: "ix_policy_amount_policy_version_id",
                table: "policy_amount",
                column: "policy_version_id");

            migrationBuilder.CreateIndex(
                name: "IX_policy_amount_PolicyVersionId1",
                table: "policy_amount",
                column: "PolicyVersionId1");

            migrationBuilder.CreateIndex(
                name: "ix_policy_document_document_id",
                table: "policy_document",
                column: "document_id");

            migrationBuilder.CreateIndex(
                name: "IX_policy_document_DocumentId1",
                table: "policy_document",
                column: "DocumentId1");

            migrationBuilder.CreateIndex(
                name: "ix_policy_document_policy_id",
                table: "policy_document",
                column: "policy_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "policy_amount");

            migrationBuilder.DropTable(
                name: "policy_document");
        }
    }
}
