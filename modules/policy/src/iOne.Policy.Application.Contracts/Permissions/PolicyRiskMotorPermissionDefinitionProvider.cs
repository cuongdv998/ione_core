using iOne.Policy.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Policy.Permissions;

public class PolicyRiskMotorPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var policyRiskMotorGroup = context.AddGroup(
            PolicyRiskMotorPermissions.GroupName,
            L("Permission:PolicyRiskMotor")
        );

        var policyRiskMotorPermission = policyRiskMotorGroup.AddPermission(
            PolicyRiskMotorPermissions.Default,
            L("Permission:PolicyRiskMotor")
        );

        policyRiskMotorPermission.AddChild(
            PolicyRiskMotorPermissions.Create,
            L("Permission:Create")
        );

        policyRiskMotorPermission.AddChild(
            PolicyRiskMotorPermissions.Edit,
            L("Permission:Edit")
        );

        policyRiskMotorPermission.AddChild(
            PolicyRiskMotorPermissions.Delete,
            L("Permission:Delete")
        );

        policyRiskMotorPermission.AddChild(
            PolicyRiskMotorPermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<PolicyResource>(name);
    }
}
