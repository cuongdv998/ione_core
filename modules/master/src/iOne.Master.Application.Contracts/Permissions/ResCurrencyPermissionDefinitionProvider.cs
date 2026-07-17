using iOne.Master.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Master.Permissions;

public class ResCurrencyPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var resCurrencyGroup = context.AddGroup(
            ResCurrencyPermissions.GroupName,
            L("Permission:ResCurrency")
        );

        var resCurrencyPermission = resCurrencyGroup.AddPermission(
            ResCurrencyPermissions.Default,
            L("Permission:ResCurrency")
        );

        resCurrencyPermission.AddChild(
            ResCurrencyPermissions.Create,
            L("Permission:Create")
        );

        resCurrencyPermission.AddChild(
            ResCurrencyPermissions.Edit,
            L("Permission:Edit")
        );

        resCurrencyPermission.AddChild(
            ResCurrencyPermissions.Delete,
            L("Permission:Delete")
        );

        resCurrencyPermission.AddChild(
            ResCurrencyPermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MasterResource>(name);
    }
}
