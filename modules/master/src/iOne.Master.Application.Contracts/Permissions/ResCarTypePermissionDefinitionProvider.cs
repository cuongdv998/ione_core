using iOne.Master.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Master.Permissions;

public class ResCarTypePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var resCarTypeGroup = context.AddGroup(
            ResCarTypePermissions.GroupName,
            L("Permission:ResCarType")
        );

        var resCarTypePermission = resCarTypeGroup.AddPermission(
            ResCarTypePermissions.Default,
            L("Permission:ResCarType")
        );

        resCarTypePermission.AddChild(
            ResCarTypePermissions.Create,
            L("Permission:Create")
        );

        resCarTypePermission.AddChild(
            ResCarTypePermissions.Edit,
            L("Permission:Edit")
        );

        resCarTypePermission.AddChild(
            ResCarTypePermissions.Delete,
            L("Permission:Delete")
        );

        resCarTypePermission.AddChild(
            ResCarTypePermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MasterResource>(name);
    }
}

