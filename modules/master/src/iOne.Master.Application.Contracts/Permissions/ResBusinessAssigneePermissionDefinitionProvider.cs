using iOne.Master.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Master.Permissions;

public class ResBusinessAssigneePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var resBusinessAssigneeGroup = context.AddGroup(
            ResBusinessAssigneePermissions.GroupName,
            L("Permission:ResBusinessAssignee")
        );

        var resBusinessAssigneePermission = resBusinessAssigneeGroup.AddPermission(
            ResBusinessAssigneePermissions.Default,
            L("Permission:ResBusinessAssignee")
        );

        resBusinessAssigneePermission.AddChild(
            ResBusinessAssigneePermissions.Create,
            L("Permission:Create")
        );

        resBusinessAssigneePermission.AddChild(
            ResBusinessAssigneePermissions.Edit,
            L("Permission:Edit")
        );

        resBusinessAssigneePermission.AddChild(
            ResBusinessAssigneePermissions.Delete,
            L("Permission:Delete")
        );

        resBusinessAssigneePermission.AddChild(
            ResBusinessAssigneePermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MasterResource>(name);
    }
}
