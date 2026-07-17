using iOne.Hr.Localization;
using iOne.Hr.Permissions;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Hr.Permissions;

public class HrDepartmentPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var hrDepartmentGroup = context.AddGroup(
            HrDepartmentPermissions.GroupName,
            L("Permission:HrDepartment")
        );

        var hrDepartmentPermission = hrDepartmentGroup.AddPermission(
            HrDepartmentPermissions.Default,
            L("Permission:HrDepartment")
        );

        hrDepartmentPermission.AddChild(
            HrDepartmentPermissions.Create,
            L("Permission:Create")
        );

        hrDepartmentPermission.AddChild(
            HrDepartmentPermissions.Edit,
            L("Permission:Edit")
        );

        hrDepartmentPermission.AddChild(
            HrDepartmentPermissions.Delete,
            L("Permission:Delete")
        );

        hrDepartmentPermission.AddChild(
            HrDepartmentPermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<HrResource>(name);
    }
}

