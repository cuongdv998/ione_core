using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class ProProduct_Image : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "image_document_id",
                table: "pro_product",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_pro_product_image_document_id",
                table: "pro_product",
                column: "image_document_id");

            migrationBuilder.AddForeignKey(
                name: "FK_pro_product_res_document_image_document_id",
                table: "pro_product",
                column: "image_document_id",
                principalTable: "res_document",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_pro_product_res_document_image_document_id",
                table: "pro_product");

            migrationBuilder.DropIndex(
                name: "IX_pro_product_image_document_id",
                table: "pro_product");

            migrationBuilder.DropColumn(
                name: "image_document_id",
                table: "pro_product");
        }
    }
}
