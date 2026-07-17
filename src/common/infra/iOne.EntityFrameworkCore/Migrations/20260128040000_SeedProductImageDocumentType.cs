using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class SeedProductImageDocumentType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insert PRODUCT_IMAGE ResDocumentType
            var productImageId = Guid.NewGuid();
            var concurrencyStamp = (Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N")).Substring(0, 40);

            // Use WHERE NOT EXISTS because unique on code may have been removed by RemoveUniquedCodeAllTable
            migrationBuilder.Sql($@"
                INSERT INTO res_document_type (
                    id, 
                    code, 
                    name, 
                    description, 
                    status, 
                    bucket, 
                    extra_properties, 
                    concurrency_stamp, 
                    creation_time, 
                    creator_id, 
                    is_deleted
                )
                SELECT
                    '{productImageId}',
                    'PRODUCT_IMAGE',
                    'Product Image',
                    'Document type for product images',
                    'active',
                    'product-image',
                    '{{}}',
                    '{concurrencyStamp}',
                    CURRENT_TIMESTAMP,
                    NULL,
                    false
                WHERE NOT EXISTS (SELECT 1 FROM res_document_type WHERE code = 'PRODUCT_IMAGE');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove PRODUCT_IMAGE ResDocumentType
            migrationBuilder.Sql(@"
                DELETE FROM res_document_type 
                WHERE code = 'PRODUCT_IMAGE';
            ");
        }
    }
}
