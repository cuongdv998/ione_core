using iOne.Product.Localization;
using iOne.Product.Permissions;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Product.Permissions;

public class ProTableRatePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var tableRateGroup = context.AddGroup(
            ProTableRatePermissions.GroupName,
            L("Permission:ProTableRate")
        );

        var tableRatePermission = tableRateGroup.AddPermission(
            ProTableRatePermissions.Default,
            L("Permission:ProTableRate")
        );

        tableRatePermission.AddChild(
            ProTableRatePermissions.Create,
            L("Permission:Create")
        );

        tableRatePermission.AddChild(
            ProTableRatePermissions.Edit,
            L("Permission:Edit")
        );

        tableRatePermission.AddChild(
            ProTableRatePermissions.Delete,
            L("Permission:Delete")
        );

        tableRatePermission.AddChild(
            ProTableRatePermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<ProductResource>(name);
    }
}
