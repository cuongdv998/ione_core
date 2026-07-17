using iOne.Partner.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Partner.Permissions;

public class ResChannelPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var resChannelGroup = context.AddGroup(
            ResChannelPermissions.GroupName,
            L("Permission:ResChannel")
        );

        var resChannelPermission = resChannelGroup.AddPermission(
            ResChannelPermissions.Default,
            L("Permission:ResChannel")
        );

        resChannelPermission.AddChild(
            ResChannelPermissions.Create,
            L("Permission:Create")
        );

        resChannelPermission.AddChild(
            ResChannelPermissions.Edit,
            L("Permission:Edit")
        );

        resChannelPermission.AddChild(
            ResChannelPermissions.Delete,
            L("Permission:Delete")
        );

        resChannelPermission.AddChild(
            ResChannelPermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<PartnerResource>(name);
    }
}

