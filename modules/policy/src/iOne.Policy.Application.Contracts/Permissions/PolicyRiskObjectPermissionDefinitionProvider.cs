using iOne.Policy.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Policy.Permissions;

public class PolicyRiskObjectPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var policyRiskObjectGroup = context.AddGroup(
            PolicyRiskObjectPermissions.GroupName,
            L("Permission:PolicyRiskObject")
        );

        var policyRiskObjectPermission = policyRiskObjectGroup.AddPermission(
            PolicyRiskObjectPermissions.Default,
            L("Permission:PolicyRiskObject")
        );

        policyRiskObjectPermission.AddChild(
            PolicyRiskObjectPermissions.Create,
            L("Permission:Create")
        );

        policyRiskObjectPermission.AddChild(
            PolicyRiskObjectPermissions.Edit,
            L("Permission:Edit")
        );

        policyRiskObjectPermission.AddChild(
            PolicyRiskObjectPermissions.Delete,
            L("Permission:Delete")
        );

        policyRiskObjectPermission.AddChild(
            PolicyRiskObjectPermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<PolicyResource>(name);
    }
}
