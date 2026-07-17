using iOne.Hr.Localization;
using iOne.Hr.Permissions;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Hr.Permissions;

public class HrEmployeePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var employeeGroup = context.AddGroup(
            HrEmployeePermissions.GroupName,
            L("Permission:HrEmployee")
        );

        var employeePermission = employeeGroup.AddPermission(
            HrEmployeePermissions.Default,
            L("Permission:HrEmployee")
        );

        employeePermission.AddChild(
            HrEmployeePermissions.Create,
            L("Permission:Create")
        );

        employeePermission.AddChild(
            HrEmployeePermissions.Edit,
            L("Permission:Edit")
        );

        employeePermission.AddChild(
            HrEmployeePermissions.Delete,
            L("Permission:Delete")
        );

        employeePermission.AddChild(
            HrEmployeePermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<HrResource>(name);
    }
}

