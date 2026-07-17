using iOne.Policy.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Policy.Permissions;

public class PolicyCoveragePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var policyCoverageGroup = context.AddGroup(
            PolicyCoveragePermissions.GroupName,
            L("Permission:PolicyCoverage")
        );

        var policyCoveragePermission = policyCoverageGroup.AddPermission(
            PolicyCoveragePermissions.Default,
            L("Permission:PolicyCoverage")
        );

        policyCoveragePermission.AddChild(
            PolicyCoveragePermissions.Create,
            L("Permission:Create")
        );

        policyCoveragePermission.AddChild(
            PolicyCoveragePermissions.Edit,
            L("Permission:Edit")
        );

        policyCoveragePermission.AddChild(
            PolicyCoveragePermissions.Delete,
            L("Permission:Delete")
        );

        policyCoveragePermission.AddChild(
            PolicyCoveragePermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<PolicyResource>(name);
    }
}
