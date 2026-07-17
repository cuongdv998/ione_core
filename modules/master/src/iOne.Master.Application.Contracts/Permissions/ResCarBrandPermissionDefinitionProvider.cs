using iOne.Master.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Master.Permissions;

public class ResCarBrandPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var resCarBrandGroup = context.AddGroup(
            ResCarBrandPermissions.GroupName,
            L("Permission:ResCarBrand")
        );

        var resCarBrandPermission = resCarBrandGroup.AddPermission(
            ResCarBrandPermissions.Default,
            L("Permission:ResCarBrand")
        );

        resCarBrandPermission.AddChild(
            ResCarBrandPermissions.Create,
            L("Permission:Create")
        );

        resCarBrandPermission.AddChild(
            ResCarBrandPermissions.Edit,
            L("Permission:Edit")
        );

        resCarBrandPermission.AddChild(
            ResCarBrandPermissions.Delete,
            L("Permission:Delete")
        );

        resCarBrandPermission.AddChild(
            ResCarBrandPermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MasterResource>(name);
    }
}



