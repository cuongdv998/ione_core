using iOne.Master.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Master.Permissions;

public class ResCarLinePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var resCarLineGroup = context.AddGroup(
            ResCarLinePermissions.GroupName,
            L("Permission:ResCarLine")
        );

        var resCarLinePermission = resCarLineGroup.AddPermission(
            ResCarLinePermissions.Default,
            L("Permission:ResCarLine")
        );

        resCarLinePermission.AddChild(
            ResCarLinePermissions.Create,
            L("Permission:Create")
        );

        resCarLinePermission.AddChild(
            ResCarLinePermissions.Edit,
            L("Permission:Edit")
        );

        resCarLinePermission.AddChild(
            ResCarLinePermissions.Delete,
            L("Permission:Delete")
        );

        resCarLinePermission.AddChild(
            ResCarLinePermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MasterResource>(name);
    }
}


