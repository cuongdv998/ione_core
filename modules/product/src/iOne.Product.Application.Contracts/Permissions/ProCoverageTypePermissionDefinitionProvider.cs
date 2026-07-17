using iOne.Product.Localization;
using iOne.Product.Permissions;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Product.Permissions;

public class ProCoverageTypePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var coverageTypeGroup = context.AddGroup(
            ProCoverageTypePermissions.GroupName,
            L("Permission:ProCoverageType")
        );

        var coverageTypePermission = coverageTypeGroup.AddPermission(
            ProCoverageTypePermissions.Default,
            L("Permission:ProCoverageType")
        );

        coverageTypePermission.AddChild(
            ProCoverageTypePermissions.Create,
            L("Permission:Create")
        );

        coverageTypePermission.AddChild(
            ProCoverageTypePermissions.Edit,
            L("Permission:Edit")
        );

        coverageTypePermission.AddChild(
            ProCoverageTypePermissions.Delete,
            L("Permission:Delete")
        );

        coverageTypePermission.AddChild(
            ProCoverageTypePermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<ProductResource>(name);
    }
}

