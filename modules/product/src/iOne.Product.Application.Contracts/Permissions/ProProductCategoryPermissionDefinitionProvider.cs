using iOne.Product.Localization;
using iOne.Product.Permissions;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Product.Permissions;

public class ProProductCategoryPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var productCategoryGroup = context.AddGroup(
            ProProductCategoryPermissions.GroupName,
            L("Permission:ProProductCategory")
        );

        var productCategoryPermission = productCategoryGroup.AddPermission(
            ProProductCategoryPermissions.Default,
            L("Permission:ProProductCategory")
        );

        productCategoryPermission.AddChild(
            ProProductCategoryPermissions.Create,
            L("Permission:Create")
        );

        productCategoryPermission.AddChild(
            ProProductCategoryPermissions.Edit,
            L("Permission:Edit")
        );

        productCategoryPermission.AddChild(
            ProProductCategoryPermissions.Delete,
            L("Permission:Delete")
        );

        productCategoryPermission.AddChild(
            ProProductCategoryPermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<ProductResource>(name);
    }
}
