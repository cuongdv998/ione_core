using iOne.Master.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Master.Permissions;

public class ResFeeItemPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var resFeeItemGroup = context.AddGroup(
            ResFeeItemPermissions.GroupName,
            L("Permission:ResFeeItem")
        );

        var resFeeItemPermission = resFeeItemGroup.AddPermission(
            ResFeeItemPermissions.Default,
            L("Permission:ResFeeItem")
        );

        resFeeItemPermission.AddChild(
            ResFeeItemPermissions.Create,
            L("Permission:Create")
        );

        resFeeItemPermission.AddChild(
            ResFeeItemPermissions.Edit,
            L("Permission:Edit")
        );

        resFeeItemPermission.AddChild(
            ResFeeItemPermissions.Delete,
            L("Permission:Delete")
        );

        resFeeItemPermission.AddChild(
            ResFeeItemPermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MasterResource>(name);
    }
}
