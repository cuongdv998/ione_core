using iOne.Master.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Master.Permissions;

public class ResAppChannelPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var resAppChannelGroup = context.AddGroup(
            ResAppChannelPermissions.GroupName,
            L("Permission:ResAppChannel")
        );

        var resAppChannelPermission = resAppChannelGroup.AddPermission(
            ResAppChannelPermissions.Default,
            L("Permission:ResAppChannel")
        );

        resAppChannelPermission.AddChild(
            ResAppChannelPermissions.Create,
            L("Permission:Create")
        );

        resAppChannelPermission.AddChild(
            ResAppChannelPermissions.Edit,
            L("Permission:Edit")
        );

        resAppChannelPermission.AddChild(
            ResAppChannelPermissions.Delete,
            L("Permission:Delete")
        );

        resAppChannelPermission.AddChild(
            ResAppChannelPermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MasterResource>(name);
    }
}
