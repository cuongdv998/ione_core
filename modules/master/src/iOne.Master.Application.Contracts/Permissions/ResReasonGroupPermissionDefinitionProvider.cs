using iOne.Master.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Master.Permissions;

public class ResReasonGroupPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var resReasonGroupGroup = context.AddGroup(
            ResReasonGroupPermissions.GroupName,
            L("Permission:ResReasonGroup")
        );

        var resReasonGroupPermission = resReasonGroupGroup.AddPermission(
            ResReasonGroupPermissions.Default,
            L("Permission:ResReasonGroup")
        );

        resReasonGroupPermission.AddChild(
            ResReasonGroupPermissions.Create,
            L("Permission:Create")
        );

        resReasonGroupPermission.AddChild(
            ResReasonGroupPermissions.Edit,
            L("Permission:Edit")
        );

        resReasonGroupPermission.AddChild(
            ResReasonGroupPermissions.Delete,
            L("Permission:Delete")
        );

        resReasonGroupPermission.AddChild(
            ResReasonGroupPermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MasterResource>(name);
    }
}
