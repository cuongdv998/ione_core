using iOne.Product.Localization;
using iOne.Product.Permissions;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Product.Permissions;

public class ProProductPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var productGroup = context.AddGroup(
            ProProductPermissions.GroupName,
            L("Permission:ProProduct")
        );

        var productPermission = productGroup.AddPermission(
            ProProductPermissions.Default,
            L("Permission:ProProduct")
        );

        productPermission.AddChild(
            ProProductPermissions.Create,
            L("Permission:Create")
        );

        productPermission.AddChild(
            ProProductPermissions.Edit,
            L("Permission:Edit")
        );

        productPermission.AddChild(
            ProProductPermissions.Delete,
            L("Permission:Delete")
        );

        productPermission.AddChild(
            ProProductPermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<ProductResource>(name);
    }
}
