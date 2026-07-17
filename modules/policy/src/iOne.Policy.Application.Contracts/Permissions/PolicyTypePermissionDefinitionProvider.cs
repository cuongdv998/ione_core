using iOne.Policy.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Policy.Permissions;

public class PolicyTypePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var policyTypeGroup = context.AddGroup(
            PolicyTypePermissions.GroupName,
            L("Permission:PolicyType")
        );

        var policyTypePermission = policyTypeGroup.AddPermission(
            PolicyTypePermissions.Default,
            L("Permission:PolicyType")
        );

        policyTypePermission.AddChild(
            PolicyTypePermissions.Create,
            L("Permission:Create")
        );

        policyTypePermission.AddChild(
            PolicyTypePermissions.Edit,
            L("Permission:Edit")
        );

        policyTypePermission.AddChild(
            PolicyTypePermissions.Delete,
            L("Permission:Delete")
        );

        policyTypePermission.AddChild(
            PolicyTypePermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<PolicyResource>(name);
    }
}
