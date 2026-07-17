using iOne.Master.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Master.Permissions;

public class ResReasonPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var resReasonGroup = context.AddGroup(
            ResReasonPermissions.GroupName,
            L("Permission:ResReason")
        );

        var resReasonPermission = resReasonGroup.AddPermission(
            ResReasonPermissions.Default,
            L("Permission:ResReason")
        );

        resReasonPermission.AddChild(
            ResReasonPermissions.Create,
            L("Permission:Create")
        );

        resReasonPermission.AddChild(
            ResReasonPermissions.Edit,
            L("Permission:Edit")
        );

        resReasonPermission.AddChild(
            ResReasonPermissions.Delete,
            L("Permission:Delete")
        );

        resReasonPermission.AddChild(
            ResReasonPermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MasterResource>(name);
    }
}
