using iOne.Product.Localization;
using iOne.Product.Permissions;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Product.Permissions;

public class ProCoverageLevelTypePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var coverageLevelTypeGroup = context.AddGroup(
            ProCoverageLevelTypePermissions.GroupName,
            L("Permission:ProCoverageLevelType")
        );

        var coverageLevelTypePermission = coverageLevelTypeGroup.AddPermission(
            ProCoverageLevelTypePermissions.Default,
            L("Permission:ProCoverageLevelType")
        );

        coverageLevelTypePermission.AddChild(
            ProCoverageLevelTypePermissions.Create,
            L("Permission:Create")
        );

        coverageLevelTypePermission.AddChild(
            ProCoverageLevelTypePermissions.Edit,
            L("Permission:Edit")
        );

        coverageLevelTypePermission.AddChild(
            ProCoverageLevelTypePermissions.Delete,
            L("Permission:Delete")
        );

        coverageLevelTypePermission.AddChild(
            ProCoverageLevelTypePermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<ProductResource>(name);
    }
}
