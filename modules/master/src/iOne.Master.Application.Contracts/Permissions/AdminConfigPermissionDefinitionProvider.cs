using iOne.Master.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Master.Permissions;

public class AdminConfigPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var adminConfigGroup = context.AddGroup(
            AdminConfigPermissions.GroupName,
            L("Permission:AdminConfig")
        );

        var adminConfigPermission = adminConfigGroup.AddPermission(
            AdminConfigPermissions.Default,
            L("Permission:AdminConfig")
        );

        adminConfigPermission.AddChild(
            AdminConfigPermissions.Create,
            L("Permission:Create")
        );

        adminConfigPermission.AddChild(
            AdminConfigPermissions.Edit,
            L("Permission:Edit")
        );

        adminConfigPermission.AddChild(
            AdminConfigPermissions.Delete,
            L("Permission:Delete")
        );

        adminConfigPermission.AddChild(
            AdminConfigPermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MasterResource>(name);
    }
}

