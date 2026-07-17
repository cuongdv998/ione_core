using iOne.Master.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Master.Permissions;

public class ResWardPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var resWardGroup = context.AddGroup(
            ResWardPermissions.GroupName,
            L("Permission:ResWard")
        );

        var resWardPermission = resWardGroup.AddPermission(
            ResWardPermissions.Default,
            L("Permission:ResWard")
        );

        resWardPermission.AddChild(
            ResWardPermissions.Create,
            L("Permission:Create")
        );

        resWardPermission.AddChild(
            ResWardPermissions.Edit,
            L("Permission:Edit")
        );

        resWardPermission.AddChild(
            ResWardPermissions.Delete,
            L("Permission:Delete")
        );

        resWardPermission.AddChild(
            ResWardPermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MasterResource>(name);
    }
}

