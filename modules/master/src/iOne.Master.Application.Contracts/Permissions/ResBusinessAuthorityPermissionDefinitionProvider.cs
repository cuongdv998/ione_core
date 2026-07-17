using iOne.Master.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Master.Permissions;

public class ResBusinessAuthorityPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var resBusinessAuthorityGroup = context.AddGroup(
            ResBusinessAuthorityPermissions.GroupName,
            L("Permission:ResBusinessAuthority")
        );

        var resBusinessAuthorityPermission = resBusinessAuthorityGroup.AddPermission(
            ResBusinessAuthorityPermissions.Default,
            L("Permission:ResBusinessAuthority")
        );

        resBusinessAuthorityPermission.AddChild(
            ResBusinessAuthorityPermissions.Create,
            L("Permission:Create")
        );

        resBusinessAuthorityPermission.AddChild(
            ResBusinessAuthorityPermissions.Edit,
            L("Permission:Edit")
        );

        resBusinessAuthorityPermission.AddChild(
            ResBusinessAuthorityPermissions.Delete,
            L("Permission:Delete")
        );

        resBusinessAuthorityPermission.AddChild(
            ResBusinessAuthorityPermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MasterResource>(name);
    }
}
