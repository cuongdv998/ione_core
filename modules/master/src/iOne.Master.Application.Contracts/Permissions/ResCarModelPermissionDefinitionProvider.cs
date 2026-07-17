using iOne.Master.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Master.Permissions;

public class ResCarModelPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var resCarModelGroup = context.AddGroup(
            ResCarModelPermissions.GroupName,
            L("Permission:ResCarModel")
        );

        var resCarModelPermission = resCarModelGroup.AddPermission(
            ResCarModelPermissions.Default,
            L("Permission:ResCarModel")
        );

        resCarModelPermission.AddChild(
            ResCarModelPermissions.Create,
            L("Permission:Create")
        );

        resCarModelPermission.AddChild(
            ResCarModelPermissions.Edit,
            L("Permission:Edit")
        );

        resCarModelPermission.AddChild(
            ResCarModelPermissions.Delete,
            L("Permission:Delete")
        );

        resCarModelPermission.AddChild(
            ResCarModelPermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MasterResource>(name);
    }
}



