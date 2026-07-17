using iOne.Policy.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Policy.Permissions;

public class PolicyContractPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var policyContractGroup = context.AddGroup(
            PolicyContractPermissions.GroupName,
            L("Permission:PolicyContract")
        );

        var policyContractPermission = policyContractGroup.AddPermission(
            PolicyContractPermissions.Default,
            L("Permission:PolicyContract")
        );

        policyContractPermission.AddChild(
            PolicyContractPermissions.Create,
            L("Permission:Create")
        );

        policyContractPermission.AddChild(
            PolicyContractPermissions.Edit,
            L("Permission:Edit")
        );

        policyContractPermission.AddChild(
            PolicyContractPermissions.Delete,
            L("Permission:Delete")
        );

        policyContractPermission.AddChild(
            PolicyContractPermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<PolicyResource>(name);
    }
}
