using iOne.Policy.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Policy.Permissions;

public class PolicyVersionPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var policyVersionGroup = context.AddGroup(
            PolicyVersionPermissions.GroupName,
            L("Permission:PolicyVersion")
        );

        var policyVersionPermission = policyVersionGroup.AddPermission(
            PolicyVersionPermissions.Default,
            L("Permission:PolicyVersion")
        );

        policyVersionPermission.AddChild(
            PolicyVersionPermissions.Create,
            L("Permission:Create")
        );

        policyVersionPermission.AddChild(
            PolicyVersionPermissions.Edit,
            L("Permission:Edit")
        );

        policyVersionPermission.AddChild(
            PolicyVersionPermissions.Delete,
            L("Permission:Delete")
        );

        policyVersionPermission.AddChild(
            PolicyVersionPermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<PolicyResource>(name);
    }
}
