using iOne.Product.Localization;
using iOne.Product.Permissions;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Product.Permissions;

public class ProProductTypePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var productTypeGroup = context.AddGroup(
            ProProductTypePermissions.GroupName,
            L("Permission:ProProductType")
        );

        var productTypePermission = productTypeGroup.AddPermission(
            ProProductTypePermissions.Default,
            L("Permission:ProProductType")
        );

        productTypePermission.AddChild(
            ProProductTypePermissions.Create,
            L("Permission:Create")
        );

        productTypePermission.AddChild(
            ProProductTypePermissions.Edit,
            L("Permission:Edit")
        );

        productTypePermission.AddChild(
            ProProductTypePermissions.Delete,
            L("Permission:Delete")
        );

        productTypePermission.AddChild(
            ProProductTypePermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<ProductResource>(name);
    }
}
