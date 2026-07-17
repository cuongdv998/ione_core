using iOne.Policy.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Policy.Permissions;

public class PolicyPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var policyGroup = context.AddGroup(
            PolicyPermissions.GroupName,
            L("Permission:Policy")
        );

        var policyPermission = policyGroup.AddPermission(
            PolicyPermissions.Default,
            L("Permission:Policy")
        );

        policyPermission.AddChild(
            PolicyPermissions.Create,
            L("Permission:Create")
        );

        policyPermission.AddChild(
            PolicyPermissions.Edit,
            L("Permission:Edit")
        );

        policyPermission.AddChild(
            PolicyPermissions.Delete,
            L("Permission:Delete")
        );

        policyPermission.AddChild(
            PolicyPermissions.View,
            L("Permission:View")
        );

        var requestApprovalPermission = policyGroup.AddPermission(
            PolicyPermissions.RequestApproval.Default,
            L("Permission:RequestApproval")
        );

        requestApprovalPermission.AddChild(
            PolicyPermissions.RequestApproval.View,
            L("Permission:View")
        );

        requestApprovalPermission.AddChild(
            PolicyPermissions.RequestApproval.Approve,
            L("Permission:Approve")
        );

        var terminateApprovalPermission = policyGroup.AddPermission(
            PolicyPermissions.TerminateApproval.Default,
            L("Permission:TerminateApproval")
        );

        terminateApprovalPermission.AddChild(
            PolicyPermissions.TerminateApproval.View,
            L("Permission:View")
        );

        terminateApprovalPermission.AddChild(
            PolicyPermissions.TerminateApproval.Approve,
            L("Permission:Approve")
        );

        var endorsementApprovalPermission = policyGroup.AddPermission(
            PolicyPermissions.EndorsementApproval.Default,
            L("Permission:EndorsementApproval")
        );

        endorsementApprovalPermission.AddChild(
            PolicyPermissions.EndorsementApproval.View,
            L("Permission:View")
        );

        endorsementApprovalPermission.AddChild(
            PolicyPermissions.EndorsementApproval.Approve,
            L("Permission:Approve")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<PolicyResource>(name);
    }
}
