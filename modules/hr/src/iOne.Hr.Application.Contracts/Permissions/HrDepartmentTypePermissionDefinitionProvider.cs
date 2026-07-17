using iOne.Hr.Localization;
using iOne.Hr.Permissions;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Hr.Permissions;

public class HrDepartmentTypePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var hrDepartmentTypeGroup = context.AddGroup(
            HrDepartmentTypePermissions.GroupName,
            L("Permission:HrDepartmentType")
        );

        var hrDepartmentTypePermission = hrDepartmentTypeGroup.AddPermission(
            HrDepartmentTypePermissions.Default,
            L("Permission:HrDepartmentType")
        );

        hrDepartmentTypePermission.AddChild(
            HrDepartmentTypePermissions.Create,
            L("Permission:Create")
        );

        hrDepartmentTypePermission.AddChild(
            HrDepartmentTypePermissions.Edit,
            L("Permission:Edit")
        );

        hrDepartmentTypePermission.AddChild(
            HrDepartmentTypePermissions.Delete,
            L("Permission:Delete")
        );

        hrDepartmentTypePermission.AddChild(
            HrDepartmentTypePermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<HrResource>(name);
    }
}

