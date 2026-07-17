using iOne.Product.Localization;
using iOne.Product.Permissions;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Product.Permissions;

public class ProLineOfBusinessPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var lineOfBusinessGroup = context.AddGroup(
            ProLineOfBusinessPermissions.GroupName,
            L("Permission:ProLineOfBusiness")
        );

        var lineOfBusinessPermission = lineOfBusinessGroup.AddPermission(
            ProLineOfBusinessPermissions.Default,
            L("Permission:ProLineOfBusiness")
        );

        lineOfBusinessPermission.AddChild(
            ProLineOfBusinessPermissions.Create,
            L("Permission:Create")
        );

        lineOfBusinessPermission.AddChild(
            ProLineOfBusinessPermissions.Edit,
            L("Permission:Edit")
        );

        lineOfBusinessPermission.AddChild(
            ProLineOfBusinessPermissions.Delete,
            L("Permission:Delete")
        );

        lineOfBusinessPermission.AddChild(
            ProLineOfBusinessPermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<ProductResource>(name);
    }
}




