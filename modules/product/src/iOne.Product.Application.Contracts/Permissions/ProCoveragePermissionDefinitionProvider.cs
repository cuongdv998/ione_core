using iOne.Product.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Product.Permissions;

public class ProCoveragePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var proCoverageGroup = context.AddGroup(
            ProCoveragePermissions.GroupName,
            L("Permission:ProCoverage")
        );

        var proCoveragePermission = proCoverageGroup.AddPermission(
            ProCoveragePermissions.Default,
            L("Permission:ProCoverage")
        );

        proCoveragePermission.AddChild(
            ProCoveragePermissions.Create,
            L("Permission:Create")
        );

        proCoveragePermission.AddChild(
            ProCoveragePermissions.Edit,
            L("Permission:Edit")
        );

        proCoveragePermission.AddChild(
            ProCoveragePermissions.Delete,
            L("Permission:Delete")
        );

        proCoveragePermission.AddChild(
            ProCoveragePermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<ProductResource>(name);
    }
}
