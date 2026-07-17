using iOne.Master.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Master.Permissions;

public class BusinessFlowPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var businessFlowGroup = context.AddGroup(
            BusinessFlowPermissions.GroupName,
            L("Permission:BusinessFlow")
        );

        var businessFlowPermission = businessFlowGroup.AddPermission(
            BusinessFlowPermissions.Default,
            L("Permission:BusinessFlow")
        );

        businessFlowPermission.AddChild(
            BusinessFlowPermissions.Create,
            L("Permission:Create")
        );

        businessFlowPermission.AddChild(
            BusinessFlowPermissions.Edit,
            L("Permission:Edit")
        );

        businessFlowPermission.AddChild(
            BusinessFlowPermissions.Delete,
            L("Permission:Delete")
        );

        businessFlowPermission.AddChild(
            BusinessFlowPermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MasterResource>(name);
    }
}
