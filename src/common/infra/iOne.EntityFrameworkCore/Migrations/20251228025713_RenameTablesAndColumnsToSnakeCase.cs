using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class RenameTablesAndColumnsToSnakeCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_HrEmployeeRole",
                table: "HrEmployeeRole");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HrEmployeePosition",
                table: "HrEmployeePosition");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HrEmployeeLevel",
                table: "HrEmployeeLevel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HrDepartmentType",
                table: "HrDepartmentType");

            migrationBuilder.RenameTable(
                name: "HrEmployeeRole",
                newName: "hr_employee_role");

            migrationBuilder.RenameTable(
                name: "HrEmployeePosition",
                newName: "hr_employee_position");

            migrationBuilder.RenameTable(
                name: "HrEmployeeLevel",
                newName: "hr_employee_level");

            migrationBuilder.RenameTable(
                name: "HrDepartmentType",
                newName: "hr_department_type");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "hr_employee_role",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "hr_employee_role",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "hr_employee_role",
                newName: "code");

            migrationBuilder.RenameColumn(
                name: "LastModifierId",
                table: "hr_employee_role",
                newName: "last_modifier_id");

            migrationBuilder.RenameColumn(
                name: "LastModificationTime",
                table: "hr_employee_role",
                newName: "last_modification_time");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "hr_employee_role",
                newName: "is_deleted");

            migrationBuilder.RenameColumn(
                name: "DeletionTime",
                table: "hr_employee_role",
                newName: "deletion_time");

            migrationBuilder.RenameColumn(
                name: "DeleterId",
                table: "hr_employee_role",
                newName: "deleter_id");

            migrationBuilder.RenameColumn(
                name: "CreatorId",
                table: "hr_employee_role",
                newName: "creator_id");

            migrationBuilder.RenameColumn(
                name: "CreationTime",
                table: "hr_employee_role",
                newName: "creation_time");

            migrationBuilder.RenameColumn(
                name: "ConcurrencyStamp",
                table: "hr_employee_role",
                newName: "concurrency_stamp");

            migrationBuilder.RenameIndex(
                name: "IX_HrEmployeeRole_Code",
                table: "hr_employee_role",
                newName: "ix_hr_employee_role_code");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "hr_employee_position",
                newName: "type");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "hr_employee_position",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "hr_employee_position",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "hr_employee_position",
                newName: "code");

            migrationBuilder.RenameColumn(
                name: "LastModifierId",
                table: "hr_employee_position",
                newName: "last_modifier_id");

            migrationBuilder.RenameColumn(
                name: "LastModificationTime",
                table: "hr_employee_position",
                newName: "last_modification_time");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "hr_employee_position",
                newName: "is_deleted");

            migrationBuilder.RenameColumn(
                name: "DeletionTime",
                table: "hr_employee_position",
                newName: "deletion_time");

            migrationBuilder.RenameColumn(
                name: "DeleterId",
                table: "hr_employee_position",
                newName: "deleter_id");

            migrationBuilder.RenameColumn(
                name: "CreatorId",
                table: "hr_employee_position",
                newName: "creator_id");

            migrationBuilder.RenameColumn(
                name: "CreationTime",
                table: "hr_employee_position",
                newName: "creation_time");

            migrationBuilder.RenameColumn(
                name: "ConcurrencyStamp",
                table: "hr_employee_position",
                newName: "concurrency_stamp");

            migrationBuilder.RenameIndex(
                name: "IX_HrEmployeePosition_Code",
                table: "hr_employee_position",
                newName: "ix_hr_employee_position_code");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "hr_employee_level",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "hr_employee_level",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "hr_employee_level",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "hr_employee_level",
                newName: "code");

            migrationBuilder.RenameColumn(
                name: "LastModifierId",
                table: "hr_employee_level",
                newName: "last_modifier_id");

            migrationBuilder.RenameColumn(
                name: "LastModificationTime",
                table: "hr_employee_level",
                newName: "last_modification_time");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "hr_employee_level",
                newName: "is_deleted");

            migrationBuilder.RenameColumn(
                name: "DeletionTime",
                table: "hr_employee_level",
                newName: "deletion_time");

            migrationBuilder.RenameColumn(
                name: "DeleterId",
                table: "hr_employee_level",
                newName: "deleter_id");

            migrationBuilder.RenameColumn(
                name: "CreatorId",
                table: "hr_employee_level",
                newName: "creator_id");

            migrationBuilder.RenameColumn(
                name: "CreationTime",
                table: "hr_employee_level",
                newName: "creation_time");

            migrationBuilder.RenameColumn(
                name: "ConcurrencyStamp",
                table: "hr_employee_level",
                newName: "concurrency_stamp");

            migrationBuilder.RenameIndex(
                name: "IX_HrEmployeeLevel_Code",
                table: "hr_employee_level",
                newName: "ix_hr_employee_level_code");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "hr_department_type",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "hr_department_type",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "hr_department_type",
                newName: "code");

            migrationBuilder.RenameColumn(
                name: "LastModifierId",
                table: "hr_department_type",
                newName: "last_modifier_id");

            migrationBuilder.RenameColumn(
                name: "LastModificationTime",
                table: "hr_department_type",
                newName: "last_modification_time");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "hr_department_type",
                newName: "is_deleted");

            migrationBuilder.RenameColumn(
                name: "DeletionTime",
                table: "hr_department_type",
                newName: "deletion_time");

            migrationBuilder.RenameColumn(
                name: "DeleterId",
                table: "hr_department_type",
                newName: "deleter_id");

            migrationBuilder.RenameColumn(
                name: "CreatorId",
                table: "hr_department_type",
                newName: "creator_id");

            migrationBuilder.RenameColumn(
                name: "CreationTime",
                table: "hr_department_type",
                newName: "creation_time");

            migrationBuilder.RenameColumn(
                name: "ConcurrencyStamp",
                table: "hr_department_type",
                newName: "concurrency_stamp");

            migrationBuilder.RenameIndex(
                name: "IX_HrDepartmentType_Code",
                table: "hr_department_type",
                newName: "ix_hr_department_type_code");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "hr_employee_role",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "hr_employee_position",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "hr_employee_level",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "hr_department_type",
                newName: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_hr_employee_role",
                table: "hr_employee_role",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_hr_employee_position",
                table: "hr_employee_position",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_hr_employee_level",
                table: "hr_employee_level",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_hr_department_type",
                table: "hr_department_type",
                column: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_hr_employee_role",
                table: "hr_employee_role");

            migrationBuilder.DropPrimaryKey(
                name: "pk_hr_employee_position",
                table: "hr_employee_position");

            migrationBuilder.DropPrimaryKey(
                name: "pk_hr_employee_level",
                table: "hr_employee_level");

            migrationBuilder.DropPrimaryKey(
                name: "pk_hr_department_type",
                table: "hr_department_type");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "hr_employee_role",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "hr_employee_position",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "hr_employee_level",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "hr_department_type",
                newName: "Id");

            migrationBuilder.RenameTable(
                name: "hr_employee_role",
                newName: "HrEmployeeRole");

            migrationBuilder.RenameTable(
                name: "hr_employee_position",
                newName: "HrEmployeePosition");

            migrationBuilder.RenameTable(
                name: "hr_employee_level",
                newName: "HrEmployeeLevel");

            migrationBuilder.RenameTable(
                name: "hr_department_type",
                newName: "HrDepartmentType");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "HrEmployeeRole",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "HrEmployeeRole",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "code",
                table: "HrEmployeeRole",
                newName: "Code");

            migrationBuilder.RenameColumn(
                name: "last_modifier_id",
                table: "HrEmployeeRole",
                newName: "LastModifierId");

            migrationBuilder.RenameColumn(
                name: "last_modification_time",
                table: "HrEmployeeRole",
                newName: "LastModificationTime");

            migrationBuilder.RenameColumn(
                name: "is_deleted",
                table: "HrEmployeeRole",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "deletion_time",
                table: "HrEmployeeRole",
                newName: "DeletionTime");

            migrationBuilder.RenameColumn(
                name: "deleter_id",
                table: "HrEmployeeRole",
                newName: "DeleterId");

            migrationBuilder.RenameColumn(
                name: "creator_id",
                table: "HrEmployeeRole",
                newName: "CreatorId");

            migrationBuilder.RenameColumn(
                name: "creation_time",
                table: "HrEmployeeRole",
                newName: "CreationTime");

            migrationBuilder.RenameColumn(
                name: "concurrency_stamp",
                table: "HrEmployeeRole",
                newName: "ConcurrencyStamp");

            migrationBuilder.RenameIndex(
                name: "ix_hr_employee_role_code",
                table: "HrEmployeeRole",
                newName: "IX_HrEmployeeRole_Code");

            migrationBuilder.RenameColumn(
                name: "type",
                table: "HrEmployeePosition",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "HrEmployeePosition",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "HrEmployeePosition",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "code",
                table: "HrEmployeePosition",
                newName: "Code");

            migrationBuilder.RenameColumn(
                name: "last_modifier_id",
                table: "HrEmployeePosition",
                newName: "LastModifierId");

            migrationBuilder.RenameColumn(
                name: "last_modification_time",
                table: "HrEmployeePosition",
                newName: "LastModificationTime");

            migrationBuilder.RenameColumn(
                name: "is_deleted",
                table: "HrEmployeePosition",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "deletion_time",
                table: "HrEmployeePosition",
                newName: "DeletionTime");

            migrationBuilder.RenameColumn(
                name: "deleter_id",
                table: "HrEmployeePosition",
                newName: "DeleterId");

            migrationBuilder.RenameColumn(
                name: "creator_id",
                table: "HrEmployeePosition",
                newName: "CreatorId");

            migrationBuilder.RenameColumn(
                name: "creation_time",
                table: "HrEmployeePosition",
                newName: "CreationTime");

            migrationBuilder.RenameColumn(
                name: "concurrency_stamp",
                table: "HrEmployeePosition",
                newName: "ConcurrencyStamp");

            migrationBuilder.RenameIndex(
                name: "ix_hr_employee_position_code",
                table: "HrEmployeePosition",
                newName: "IX_HrEmployeePosition_Code");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "HrEmployeeLevel",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "HrEmployeeLevel",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "HrEmployeeLevel",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "code",
                table: "HrEmployeeLevel",
                newName: "Code");

            migrationBuilder.RenameColumn(
                name: "last_modifier_id",
                table: "HrEmployeeLevel",
                newName: "LastModifierId");

            migrationBuilder.RenameColumn(
                name: "last_modification_time",
                table: "HrEmployeeLevel",
                newName: "LastModificationTime");

            migrationBuilder.RenameColumn(
                name: "is_deleted",
                table: "HrEmployeeLevel",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "deletion_time",
                table: "HrEmployeeLevel",
                newName: "DeletionTime");

            migrationBuilder.RenameColumn(
                name: "deleter_id",
                table: "HrEmployeeLevel",
                newName: "DeleterId");

            migrationBuilder.RenameColumn(
                name: "creator_id",
                table: "HrEmployeeLevel",
                newName: "CreatorId");

            migrationBuilder.RenameColumn(
                name: "creation_time",
                table: "HrEmployeeLevel",
                newName: "CreationTime");

            migrationBuilder.RenameColumn(
                name: "concurrency_stamp",
                table: "HrEmployeeLevel",
                newName: "ConcurrencyStamp");

            migrationBuilder.RenameIndex(
                name: "ix_hr_employee_level_code",
                table: "HrEmployeeLevel",
                newName: "IX_HrEmployeeLevel_Code");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "HrDepartmentType",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "HrDepartmentType",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "code",
                table: "HrDepartmentType",
                newName: "Code");

            migrationBuilder.RenameColumn(
                name: "last_modifier_id",
                table: "HrDepartmentType",
                newName: "LastModifierId");

            migrationBuilder.RenameColumn(
                name: "last_modification_time",
                table: "HrDepartmentType",
                newName: "LastModificationTime");

            migrationBuilder.RenameColumn(
                name: "is_deleted",
                table: "HrDepartmentType",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "deletion_time",
                table: "HrDepartmentType",
                newName: "DeletionTime");

            migrationBuilder.RenameColumn(
                name: "deleter_id",
                table: "HrDepartmentType",
                newName: "DeleterId");

            migrationBuilder.RenameColumn(
                name: "creator_id",
                table: "HrDepartmentType",
                newName: "CreatorId");

            migrationBuilder.RenameColumn(
                name: "creation_time",
                table: "HrDepartmentType",
                newName: "CreationTime");

            migrationBuilder.RenameColumn(
                name: "concurrency_stamp",
                table: "HrDepartmentType",
                newName: "ConcurrencyStamp");

            migrationBuilder.RenameIndex(
                name: "ix_hr_department_type_code",
                table: "HrDepartmentType",
                newName: "IX_HrDepartmentType_Code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HrEmployeeRole",
                table: "HrEmployeeRole",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HrEmployeePosition",
                table: "HrEmployeePosition",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HrEmployeeLevel",
                table: "HrEmployeeLevel",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HrDepartmentType",
                table: "HrDepartmentType",
                column: "Id");
        }
    }
}
