using iOne.Master.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Master.Permissions;

public class ResObjectItemTypePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var resObjectItemTypeGroup = context.AddGroup(
            ResObjectItemTypePermissions.GroupName,
            L("Permission:ResObjectItemType")
        );

        var resObjectItemTypePermission = resObjectItemTypeGroup.AddPermission(
            ResObjectItemTypePermissions.Default,
            L("Permission:ResObjectItemType")
        );

        resObjectItemTypePermission.AddChild(
            ResObjectItemTypePermissions.Create,
            L("Permission:Create")
        );

        resObjectItemTypePermission.AddChild(
            ResObjectItemTypePermissions.Edit,
            L("Permission:Edit")
        );

        resObjectItemTypePermission.AddChild(
            ResObjectItemTypePermissions.Delete,
            L("Permission:Delete")
        );

        resObjectItemTypePermission.AddChild(
            ResObjectItemTypePermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MasterResource>(name);
    }
}
