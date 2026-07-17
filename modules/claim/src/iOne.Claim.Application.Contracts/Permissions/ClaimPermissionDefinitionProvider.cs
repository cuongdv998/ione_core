using iOne.Claim.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Claim.Permissions;

public class ClaimPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var claimGroup = context.AddGroup(
            ClaimPermissions.GroupName,
            L("Permission:Claim")
        );

        var claimPermission = claimGroup.AddPermission(
            ClaimPermissions.Default,
            L("Permission:Claim")
        );

        claimPermission.AddChild(
            ClaimPermissions.Create,
            L("Permission:Create")
        );

        claimPermission.AddChild(
            ClaimPermissions.Edit,
            L("Permission:Edit")
        );

        claimPermission.AddChild(
            ClaimPermissions.Delete,
            L("Permission:Delete")
        );

        claimPermission.AddChild(
            ClaimPermissions.View,
            L("Permission:View")
        );

        claimPermission.AddChild(
            ClaimPermissions.QuotationApprovalList,
            L("Permission:QuotationApprovalList")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<ClaimResource>(name);
    }
}
