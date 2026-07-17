using iOne.Master.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Master.Permissions;

public class ResEventPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var resEventGroup = context.AddGroup(
            ResEventPermissions.GroupName,
            L("Permission:ResEvent")
        );

        var resEventPermission = resEventGroup.AddPermission(
            ResEventPermissions.Default,
            L("Permission:ResEvent")
        );

        resEventPermission.AddChild(
            ResEventPermissions.Create,
            L("Permission:Create")
        );

        resEventPermission.AddChild(
            ResEventPermissions.Edit,
            L("Permission:Edit")
        );

        resEventPermission.AddChild(
            ResEventPermissions.Delete,
            L("Permission:Delete")
        );

        resEventPermission.AddChild(
            ResEventPermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MasterResource>(name);
    }
}

