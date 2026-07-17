using iOne.Hr.Localization;
using iOne.Hr.Permissions;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Hr.Permissions;

public class HrEmployeeLevelPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var employeeLevelGroup = context.AddGroup(
            HrEmployeeLevelPermissions.GroupName,
            L("Permission:HrEmployeeLevel")
        );

        var employeeLevelPermission = employeeLevelGroup.AddPermission(
            HrEmployeeLevelPermissions.Default,
            L("Permission:HrEmployeeLevel")
        );

        employeeLevelPermission.AddChild(
            HrEmployeeLevelPermissions.Create,
            L("Permission:Create")
        );

        employeeLevelPermission.AddChild(
            HrEmployeeLevelPermissions.Edit,
            L("Permission:Edit")
        );

        employeeLevelPermission.AddChild(
            HrEmployeeLevelPermissions.Delete,
            L("Permission:Delete")
        );

        employeeLevelPermission.AddChild(
            HrEmployeeLevelPermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<HrResource>(name);
    }
}

