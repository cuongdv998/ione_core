using iOne.Master.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Master.Permissions;

public class ResDamageLevelPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var resDamageLevelGroup = context.AddGroup(
            ResDamageLevelPermissions.GroupName,
            L("Permission:ResDamageLevel")
        );

        var resDamageLevelPermission = resDamageLevelGroup.AddPermission(
            ResDamageLevelPermissions.Default,
            L("Permission:ResDamageLevel")
        );

        resDamageLevelPermission.AddChild(
            ResDamageLevelPermissions.Create,
            L("Permission:Create")
        );

        resDamageLevelPermission.AddChild(
            ResDamageLevelPermissions.Edit,
            L("Permission:Edit")
        );

        resDamageLevelPermission.AddChild(
            ResDamageLevelPermissions.Delete,
            L("Permission:Delete")
        );

        resDamageLevelPermission.AddChild(
            ResDamageLevelPermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MasterResource>(name);
    }
}

