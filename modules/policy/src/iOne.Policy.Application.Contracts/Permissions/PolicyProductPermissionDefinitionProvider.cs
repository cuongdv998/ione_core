using iOne.Policy.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Policy.Permissions;

public class PolicyProductPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var policyProductGroup = context.AddGroup(
            PolicyProductPermissions.GroupName,
            L("Permission:PolicyProduct")
        );

        var policyProductPermission = policyProductGroup.AddPermission(
            PolicyProductPermissions.Default,
            L("Permission:PolicyProduct")
        );

        policyProductPermission.AddChild(
            PolicyProductPermissions.Create,
            L("Permission:Create")
        );

        policyProductPermission.AddChild(
            PolicyProductPermissions.Edit,
            L("Permission:Edit")
        );

        policyProductPermission.AddChild(
            PolicyProductPermissions.Delete,
            L("Permission:Delete")
        );

        policyProductPermission.AddChild(
            PolicyProductPermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<PolicyResource>(name);
    }
}
