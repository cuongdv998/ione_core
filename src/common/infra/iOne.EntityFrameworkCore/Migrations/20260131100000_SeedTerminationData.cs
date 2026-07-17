using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class SeedTerminationData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Generate IDs
            var feeItemId = Guid.NewGuid();
            var reasonGroupId = Guid.NewGuid();
            var reason1Id = Guid.NewGuid();
            var reason2Id = Guid.NewGuid();
            var reason3Id = Guid.NewGuid();
            var reason4Id = Guid.NewGuid();
            var reason5Id = Guid.NewGuid();

            // Generate concurrency stamps
            var feeItemStamp = (Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N")).Substring(0, 40);
            var reasonGroupStamp = (Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N")).Substring(0, 40);
            var reason1Stamp = (Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N")).Substring(0, 40);
            var reason2Stamp = (Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N")).Substring(0, 40);
            var reason3Stamp = (Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N")).Substring(0, 40);
            var reason4Stamp = (Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N")).Substring(0, 40);
            var reason5Stamp = (Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N")).Substring(0, 40);

            // 1. Insert TERMINATE_REFUND_AMOUNT fee item (WHERE NOT EXISTS: unique on code may have been removed)
            migrationBuilder.Sql($@"
                INSERT INTO res_fee_item (
                    id, 
                    code, 
                    name, 
                    description, 
                    status, 
                    tax_id,
                    extra_properties, 
                    concurrency_stamp, 
                    creation_time, 
                    creator_id, 
                    is_deleted
                )
                SELECT
                    '{feeItemId}',
                    'TERMINATE_REFUND_AMOUNT',
                    'Hoàn phí chấm dứt hợp đồng',
                    'Khoản hoàn phí khi chấm dứt hợp đồng bảo hiểm trước hạn',
                    'active',
                    NULL,
                    '{{}}',
                    '{feeItemStamp}',
                    CURRENT_TIMESTAMP,
                    NULL,
                    false
                WHERE NOT EXISTS (SELECT 1 FROM res_fee_item WHERE code = 'TERMINATE_REFUND_AMOUNT');
            ");

            // 2. Insert TERMINATE_POLICY_REASON reason group (WHERE NOT EXISTS: unique on code may have been removed)
            migrationBuilder.Sql($@"
                INSERT INTO res_reason_group (
                    id, 
                    code, 
                    name, 
                    description, 
                    status, 
                    extra_properties, 
                    concurrency_stamp, 
                    creation_time, 
                    creator_id, 
                    is_deleted
                )
                SELECT
                    '{reasonGroupId}',
                    'TERMINATE_POLICY_REASON',
                    'Lý do chấm dứt hợp đồng bảo hiểm',
                    'Nhóm lý do dùng khi chấm dứt hợp đồng/đơn bảo hiểm trước hạn',
                    'active',
                    '{{}}',
                    '{reasonGroupStamp}',
                    CURRENT_TIMESTAMP,
                    NULL,
                    false
                WHERE NOT EXISTS (SELECT 1 FROM res_reason_group WHERE code = 'TERMINATE_POLICY_REASON');
            ");

            // 3. Insert termination reasons (WHERE NOT EXISTS: unique on code/group_id may have been removed)
            var insertReasonsSql = $@"
                DO $$
                DECLARE
                    v_group_id UUID;
                BEGIN
                    SELECT id INTO v_group_id FROM res_reason_group WHERE code = 'TERMINATE_POLICY_REASON' LIMIT 1;
                    
                    IF v_group_id IS NOT NULL THEN
                        -- Reason 1: Khách hàng yêu cầu chấm dứt
                        INSERT INTO res_reason (
                            id, group_id, code, name, description, status, 
                            extra_properties, concurrency_stamp, creation_time, creator_id, is_deleted
                        )
                        SELECT '{reason1Id}', v_group_id, 'CUSTOMER_REQUEST', 
                            'Khách hàng yêu cầu chấm dứt', 
                            'Khách hàng chủ động yêu cầu chấm dứt hợp đồng bảo hiểm',
                            'active', '{{}}', '{reason1Stamp}', CURRENT_TIMESTAMP, NULL, false
                        WHERE NOT EXISTS (SELECT 1 FROM res_reason WHERE code = 'CUSTOMER_REQUEST' AND group_id = v_group_id);

                        -- Reason 2: Bán xe / Chuyển nhượng
                        INSERT INTO res_reason (
                            id, group_id, code, name, description, status, 
                            extra_properties, concurrency_stamp, creation_time, creator_id, is_deleted
                        )
                        SELECT '{reason2Id}', v_group_id, 'VEHICLE_SOLD', 
                            'Bán xe / Chuyển nhượng', 
                            'Khách hàng bán xe hoặc chuyển nhượng quyền sở hữu phương tiện',
                            'active', '{{}}', '{reason2Stamp}', CURRENT_TIMESTAMP, NULL, false
                        WHERE NOT EXISTS (SELECT 1 FROM res_reason WHERE code = 'VEHICLE_SOLD' AND group_id = v_group_id);

                        -- Reason 3: Tổn thất toàn bộ
                        INSERT INTO res_reason (
                            id, group_id, code, name, description, status, 
                            extra_properties, concurrency_stamp, creation_time, creator_id, is_deleted
                        )
                        SELECT '{reason3Id}', v_group_id, 'TOTAL_LOSS', 
                            'Tổn thất toàn bộ', 
                            'Phương tiện bị tổn thất toàn bộ và đã được bồi thường',
                            'active', '{{}}', '{reason3Stamp}', CURRENT_TIMESTAMP, NULL, false
                        WHERE NOT EXISTS (SELECT 1 FROM res_reason WHERE code = 'TOTAL_LOSS' AND group_id = v_group_id);

                        -- Reason 4: Vi phạm điều khoản hợp đồng
                        INSERT INTO res_reason (
                            id, group_id, code, name, description, status, 
                            extra_properties, concurrency_stamp, creation_time, creator_id, is_deleted
                        )
                        SELECT '{reason4Id}', v_group_id, 'CONTRACT_VIOLATION', 
                            'Vi phạm điều khoản hợp đồng', 
                            'Chấm dứt do vi phạm các điều khoản trong hợp đồng bảo hiểm',
                            'active', '{{}}', '{reason4Stamp}', CURRENT_TIMESTAMP, NULL, false
                        WHERE NOT EXISTS (SELECT 1 FROM res_reason WHERE code = 'CONTRACT_VIOLATION' AND group_id = v_group_id);

                        -- Reason 5: Lý do khác
                        INSERT INTO res_reason (
                            id, group_id, code, name, description, status, 
                            extra_properties, concurrency_stamp, creation_time, creator_id, is_deleted
                        )
                        SELECT '{reason5Id}', v_group_id, 'OTHER', 
                            'Lý do khác', 
                            'Các lý do khác không thuộc các mục trên',
                            'active', '{{}}', '{reason5Stamp}', CURRENT_TIMESTAMP, NULL, false
                        WHERE NOT EXISTS (SELECT 1 FROM res_reason WHERE code = 'OTHER' AND group_id = v_group_id);
                    END IF;
                END $$;
            ";

            migrationBuilder.Sql(insertReasonsSql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove termination reasons
            migrationBuilder.Sql(@"
                DELETE FROM res_reason 
                WHERE group_id IN (SELECT id FROM res_reason_group WHERE code = 'TERMINATE_POLICY_REASON');
            ");

            // Remove termination reason group
            migrationBuilder.Sql(@"
                DELETE FROM res_reason_group 
                WHERE code = 'TERMINATE_POLICY_REASON';
            ");

            // Remove fee item
            migrationBuilder.Sql(@"
                DELETE FROM res_fee_item 
                WHERE code = 'TERMINATE_REFUND_AMOUNT';
            ");
        }
    }
}
