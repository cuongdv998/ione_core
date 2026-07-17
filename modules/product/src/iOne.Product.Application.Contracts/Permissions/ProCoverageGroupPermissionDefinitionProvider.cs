using iOne.Product.Localization;
using iOne.Product.Permissions;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Product.Permissions;

public class ProCoverageGroupPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var coverageGroupGroup = context.AddGroup(
            ProCoverageGroupPermissions.GroupName,
            L("Permission:ProCoverageGroup")
        );

        var coverageGroupPermission = coverageGroupGroup.AddPermission(
            ProCoverageGroupPermissions.Default,
            L("Permission:ProCoverageGroup")
        );

        coverageGroupPermission.AddChild(
            ProCoverageGroupPermissions.Create,
            L("Permission:Create")
        );

        coverageGroupPermission.AddChild(
            ProCoverageGroupPermissions.Edit,
            L("Permission:Edit")
        );

        coverageGroupPermission.AddChild(
            ProCoverageGroupPermissions.Delete,
            L("Permission:Delete")
        );

        coverageGroupPermission.AddChild(
            ProCoverageGroupPermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<ProductResource>(name);
    }
}

