using iOne.Master.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Master.Permissions;

public class ResObjectItemDepreciationPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var resObjectItemDepreciationGroup = context.AddGroup(
            ResObjectItemDepreciationPermissions.GroupName,
            L("Permission:ResObjectItemDepreciation")
        );

        var resObjectItemDepreciationPermission = resObjectItemDepreciationGroup.AddPermission(
            ResObjectItemDepreciationPermissions.Default,
            L("Permission:ResObjectItemDepreciation")
        );

        resObjectItemDepreciationPermission.AddChild(
            ResObjectItemDepreciationPermissions.Create,
            L("Permission:Create")
        );

        resObjectItemDepreciationPermission.AddChild(
            ResObjectItemDepreciationPermissions.Edit,
            L("Permission:Edit")
        );

        resObjectItemDepreciationPermission.AddChild(
            ResObjectItemDepreciationPermissions.Delete,
            L("Permission:Delete")
        );

        resObjectItemDepreciationPermission.AddChild(
            ResObjectItemDepreciationPermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MasterResource>(name);
    }
}
