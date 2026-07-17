using iOne.Hr.Localization;
using iOne.Hr.Permissions;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Hr.Permissions;

public class HrEmployeeRolePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var employeeRoleGroup = context.AddGroup(
            HrEmployeeRolePermissions.GroupName,
            L("Permission:HrEmployeeRole")
        );

        var employeeRolePermission = employeeRoleGroup.AddPermission(
            HrEmployeeRolePermissions.Default,
            L("Permission:HrEmployeeRole")
        );

        employeeRolePermission.AddChild(
            HrEmployeeRolePermissions.Create,
            L("Permission:Create")
        );

        employeeRolePermission.AddChild(
            HrEmployeeRolePermissions.Edit,
            L("Permission:Edit")
        );

        employeeRolePermission.AddChild(
            HrEmployeeRolePermissions.Delete,
            L("Permission:Delete")
        );

        employeeRolePermission.AddChild(
            HrEmployeeRolePermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<HrResource>(name);
    }
}

