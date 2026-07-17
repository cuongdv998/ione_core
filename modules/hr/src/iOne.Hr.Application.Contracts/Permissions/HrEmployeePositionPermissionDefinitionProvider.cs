using iOne.Hr.Localization;
using iOne.Hr.Permissions;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Hr.Permissions;

public class HrEmployeePositionPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var hrEmployeePositionGroup = context.AddGroup(
            HrEmployeePositionPermissions.GroupName,
            L("Permission:HrEmployeePosition")
        );

        var hrEmployeePositionPermission = hrEmployeePositionGroup.AddPermission(
            HrEmployeePositionPermissions.Default,
            L("Permission:HrEmployeePosition")
        );

        hrEmployeePositionPermission.AddChild(
            HrEmployeePositionPermissions.Create,
            L("Permission:Create")
        );

        hrEmployeePositionPermission.AddChild(
            HrEmployeePositionPermissions.Edit,
            L("Permission:Edit")
        );

        hrEmployeePositionPermission.AddChild(
            HrEmployeePositionPermissions.Delete,
            L("Permission:Delete")
        );

        hrEmployeePositionPermission.AddChild(
            HrEmployeePositionPermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<HrResource>(name);
    }
}

