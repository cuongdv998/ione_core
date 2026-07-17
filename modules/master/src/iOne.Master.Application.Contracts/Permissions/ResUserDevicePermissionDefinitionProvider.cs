using iOne.Master.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Master.Permissions;

public class ResUserDevicePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var resUserDeviceGroup = context.AddGroup(
            ResUserDevicePermissions.GroupName,
            L("Permission:ResUserDevice")
        );

        var resUserDevicePermission = resUserDeviceGroup.AddPermission(
            ResUserDevicePermissions.Default,
            L("Permission:ResUserDevice")
        );

        resUserDevicePermission.AddChild(
            ResUserDevicePermissions.Create,
            L("Permission:Create")
        );

        resUserDevicePermission.AddChild(
            ResUserDevicePermissions.Edit,
            L("Permission:Edit")
        );

        resUserDevicePermission.AddChild(
            ResUserDevicePermissions.Delete,
            L("Permission:Delete")
        );

        resUserDevicePermission.AddChild(
            ResUserDevicePermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MasterResource>(name);
    }
}
