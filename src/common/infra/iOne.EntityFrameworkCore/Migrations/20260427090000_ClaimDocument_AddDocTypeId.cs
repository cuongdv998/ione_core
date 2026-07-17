using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class ClaimDocument_AddDocTypeId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "doc_type_id",
                table: "claim_document",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_claim_document_doc_type_id",
                table: "claim_document",
                column: "doc_type_id");

            migrationBuilder.AddForeignKey(
                name: "FK_claim_document_res_document_type_doc_type_id",
                table: "claim_document",
                column: "doc_type_id",
                principalTable: "res_document_type",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_claim_document_res_document_type_doc_type_id",
                table: "claim_document");

            migrationBuilder.DropIndex(
                name: "IX_claim_document_doc_type_id",
                table: "claim_document");

            migrationBuilder.DropColumn(
                name: "doc_type_id",
                table: "claim_document");
        }
    }
}
