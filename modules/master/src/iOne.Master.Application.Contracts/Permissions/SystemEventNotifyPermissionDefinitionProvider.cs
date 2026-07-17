using iOne.Master.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Master.Permissions;

public class SystemEventNotifyPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var systemEventNotifyGroup = context.AddGroup(
            SystemEventNotifyPermissions.GroupName,
            L("Permission:SystemEventNotify")
        );

        var systemEventNotifyPermission = systemEventNotifyGroup.AddPermission(
            SystemEventNotifyPermissions.Default,
            L("Permission:SystemEventNotify")
        );

        systemEventNotifyPermission.AddChild(
            SystemEventNotifyPermissions.Create,
            L("Permission:Create")
        );

        systemEventNotifyPermission.AddChild(
            SystemEventNotifyPermissions.Edit,
            L("Permission:Edit")
        );

        systemEventNotifyPermission.AddChild(
            SystemEventNotifyPermissions.Delete,
            L("Permission:Delete")
        );

        systemEventNotifyPermission.AddChild(
            SystemEventNotifyPermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MasterResource>(name);
    }
}
