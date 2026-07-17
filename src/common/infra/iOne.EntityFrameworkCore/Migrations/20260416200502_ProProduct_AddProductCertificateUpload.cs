using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class ProProduct_AddProductCertificateUpload : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "certificate_template_document_id",
                table: "pro_product",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_pro_product_certificate_template_document_id",
                table: "pro_product",
                column: "certificate_template_document_id");

            migrationBuilder.AddForeignKey(
                name: "FK_pro_product_res_document_certificate_template_document_id",
                table: "pro_product",
                column: "certificate_template_document_id",
                principalTable: "res_document",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_pro_product_res_document_certificate_template_document_id",
                table: "pro_product");

            migrationBuilder.DropIndex(
                name: "IX_pro_product_certificate_template_document_id",
                table: "pro_product");

            migrationBuilder.DropColumn(
                name: "certificate_template_document_id",
                table: "pro_product");
        }
    }
}
