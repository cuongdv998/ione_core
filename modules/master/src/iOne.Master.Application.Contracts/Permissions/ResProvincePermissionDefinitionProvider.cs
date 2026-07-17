using iOne.Master.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Master.Permissions;

public class ResProvincePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var resProvinceGroup = context.AddGroup(
            ResProvincePermissions.GroupName,
            L("Permission:ResProvince")
        );

        var resProvincePermission = resProvinceGroup.AddPermission(
            ResProvincePermissions.Default,
            L("Permission:ResProvince")
        );

        resProvincePermission.AddChild(
            ResProvincePermissions.Create,
            L("Permission:Create")
        );

        resProvincePermission.AddChild(
            ResProvincePermissions.Edit,
            L("Permission:Edit")
        );

        resProvincePermission.AddChild(
            ResProvincePermissions.Delete,
            L("Permission:Delete")
        );

        resProvincePermission.AddChild(
            ResProvincePermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MasterResource>(name);
    }
}

