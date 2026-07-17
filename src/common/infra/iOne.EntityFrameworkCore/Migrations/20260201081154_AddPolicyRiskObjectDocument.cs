using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class AddPolicyRiskObjectDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "policy_risk_object_document",
                columns: table => new
                {
                    policy_risk_object_id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId1 = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_policy_risk_object_document", x => new { x.policy_risk_object_id, x.document_id });
                    table.ForeignKey(
                        name: "FK_policy_risk_object_document_res_document_DocumentId1",
                        column: x => x.DocumentId1,
                        principalTable: "res_document",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_policy_risk_object_document_document_id",
                        column: x => x.document_id,
                        principalTable: "res_document",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_policy_risk_object_document_risk_object_id",
                        column: x => x.policy_risk_object_id,
                        principalTable: "policy_risk_object",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Tài liệu liên quan đến đối tượng bảo hiểm (PolicyRiskObject)");

            migrationBuilder.CreateIndex(
                name: "ix_policy_risk_object_document_document_id",
                table: "policy_risk_object_document",
                column: "document_id");

            migrationBuilder.CreateIndex(
                name: "IX_policy_risk_object_document_DocumentId1",
                table: "policy_risk_object_document",
                column: "DocumentId1");

            migrationBuilder.CreateIndex(
                name: "ix_policy_risk_object_document_risk_object_id",
                table: "policy_risk_object_document",
                column: "policy_risk_object_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "policy_risk_object_document");
        }
    }
}
