using iOne.Master.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Master.Permissions;

public class ResCarGroupPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var resCarGroupGroup = context.AddGroup(
            ResCarGroupPermissions.GroupName,
            L("Permission:ResCarGroup")
        );

        var resCarGroupPermission = resCarGroupGroup.AddPermission(
            ResCarGroupPermissions.Default,
            L("Permission:ResCarGroup")
        );

        resCarGroupPermission.AddChild(
            ResCarGroupPermissions.Create,
            L("Permission:Create")
        );

        resCarGroupPermission.AddChild(
            ResCarGroupPermissions.Edit,
            L("Permission:Edit")
        );

        resCarGroupPermission.AddChild(
            ResCarGroupPermissions.Delete,
            L("Permission:Delete")
        );

        resCarGroupPermission.AddChild(
            ResCarGroupPermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MasterResource>(name);
    }
}
