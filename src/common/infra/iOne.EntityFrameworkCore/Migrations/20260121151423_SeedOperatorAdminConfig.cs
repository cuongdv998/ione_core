using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class SeedOperatorAdminConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insert OPERATOR AdminConfig records
            // These operators are used in ProTableRateVariable for condition evaluation
            // Based on WebApplicationPrototype_v1.3/1_9__quan_ly_bang_phi.html
            var equalId = Guid.NewGuid();
            var greaterThanOrEqualId = Guid.NewGuid();
            var lessThanOrEqualId = Guid.NewGuid();
            var inRangeId = Guid.NewGuid();
            var inId = Guid.NewGuid();
            var containsId = Guid.NewGuid();
            var notInId = Guid.NewGuid();
            var notContainsId = Guid.NewGuid();
            var creatorId = Guid.NewGuid();
            // Generate 40-character concurrency stamps (GUID "N" format is 32 chars, so we concatenate and take first 40)
            var concurrencyStamp1 = (Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N")).Substring(0, 40);
            var concurrencyStamp2 = (Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N")).Substring(0, 40);
            var concurrencyStamp3 = (Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N")).Substring(0, 40);
            var concurrencyStamp4 = (Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N")).Substring(0, 40);
            var concurrencyStamp5 = (Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N")).Substring(0, 40);
            var concurrencyStamp6 = (Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N")).Substring(0, 40);
            var concurrencyStamp7 = (Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N")).Substring(0, 40);
            var concurrencyStamp8 = (Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N")).Substring(0, 40);

            migrationBuilder.Sql($@"
                INSERT INTO admin_config (id, code, name, sub_code, value, description, status, extra_properties, concurrency_stamp, creation_time, creator_id, is_deleted)
                VALUES 
                ('{equalId}', 'OPERATOR', 'Bằng', 'EQUAL', '1', 'Phép toán bằng', 'active', '{{}}', '{concurrencyStamp1}', CURRENT_TIMESTAMP, '{creatorId}', false),
                ('{greaterThanOrEqualId}', 'OPERATOR', 'Lớn hơn hoặc bằng', 'GREATER_OR_EQUAL', '2', 'Phép toán lớn hơn hoặc bằng', 'active', '{{}}', '{concurrencyStamp2}', CURRENT_TIMESTAMP, '{creatorId}', false),
                ('{lessThanOrEqualId}', 'OPERATOR', 'Nhỏ hơn hoặc bằng', 'LESS_OR_EQUAL', '3', 'Phép toán nhỏ hơn hoặc bằng', 'active', '{{}}', '{concurrencyStamp3}', CURRENT_TIMESTAMP, '{creatorId}', false),
                ('{inRangeId}', 'OPERATOR', 'Trong khoảng', 'IN_RANGE', '4', 'Phép toán trong khoảng', 'active', '{{}}', '{concurrencyStamp4}', CURRENT_TIMESTAMP, '{creatorId}', false),
                ('{inId}', 'OPERATOR', 'Thuộc', 'IN', '5', 'Phép toán thuộc', 'active', '{{}}', '{concurrencyStamp5}', CURRENT_TIMESTAMP, '{creatorId}', false),
                ('{containsId}', 'OPERATOR', 'Chứa', 'CONTAINS', '6', 'Phép toán chứa', 'active', '{{}}', '{concurrencyStamp6}', CURRENT_TIMESTAMP, '{creatorId}', false),
                ('{notInId}', 'OPERATOR', 'Không thuộc', 'NOT_IN', '7', 'Phép toán không thuộc', 'active', '{{}}', '{concurrencyStamp7}', CURRENT_TIMESTAMP, '{creatorId}', false),
                ('{notContainsId}', 'OPERATOR', 'không chứa', 'NOT_CONTAINS', '8', 'Phép toán không chứa', 'active', '{{}}', '{concurrencyStamp8}', CURRENT_TIMESTAMP, '{creatorId}', false)
                ON CONFLICT (code, sub_code) DO NOTHING;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove OPERATOR AdminConfig records
            migrationBuilder.Sql(@"
                DELETE FROM admin_config 
                WHERE code = 'OPERATOR';
            ");
        }
    }
}
