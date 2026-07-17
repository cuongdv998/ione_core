using iOne.Product.Localization;
using iOne.Product.Permissions;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Product.Permissions;

public class ProAttributePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var proAttributeGroup = context.AddGroup(
            ProAttributePermissions.GroupName,
            L("Permission:ProAttribute")
        );

        var proAttributePermission = proAttributeGroup.AddPermission(
            ProAttributePermissions.Default,
            L("Permission:ProAttribute")
        );

        proAttributePermission.AddChild(
            ProAttributePermissions.Create,
            L("Permission:Create")
        );

        proAttributePermission.AddChild(
            ProAttributePermissions.Edit,
            L("Permission:Edit")
        );

        proAttributePermission.AddChild(
            ProAttributePermissions.Delete,
            L("Permission:Delete")
        );

        proAttributePermission.AddChild(
            ProAttributePermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<ProductResource>(name);
    }
}
