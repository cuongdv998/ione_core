using iOne.Product.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Product.Permissions;

public class ResTaxPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var resTaxGroup = context.AddGroup(
            ResTaxPermissions.GroupName,
            L("Permission:ResTax")
        );

        var resTaxPermission = resTaxGroup.AddPermission(
            ResTaxPermissions.Default,
            L("Permission:ResTax")
        );

        resTaxPermission.AddChild(
            ResTaxPermissions.Create,
            L("Permission:Create")
        );

        resTaxPermission.AddChild(
            ResTaxPermissions.Edit,
            L("Permission:Edit")
        );

        resTaxPermission.AddChild(
            ResTaxPermissions.Delete,
            L("Permission:Delete")
        );

        resTaxPermission.AddChild(
            ResTaxPermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<ProductResource>(name);
    }
}

