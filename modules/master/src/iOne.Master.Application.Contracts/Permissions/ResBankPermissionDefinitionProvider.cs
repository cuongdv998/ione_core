using iOne.Master.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Master.Permissions;

public class ResBankPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var resBankGroup = context.AddGroup(
            ResBankPermissions.GroupName,
            L("Permission:ResBank")
        );

        var resBankPermission = resBankGroup.AddPermission(
            ResBankPermissions.Default,
            L("Permission:ResBank")
        );

        resBankPermission.AddChild(
            ResBankPermissions.Create,
            L("Permission:Create")
        );

        resBankPermission.AddChild(
            ResBankPermissions.Edit,
            L("Permission:Edit")
        );

        resBankPermission.AddChild(
            ResBankPermissions.Delete,
            L("Permission:Delete")
        );

        resBankPermission.AddChild(
            ResBankPermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MasterResource>(name);
    }
}

