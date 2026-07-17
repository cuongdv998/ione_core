using iOne.Policy.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Policy.Permissions;

public class PolicyCoverageLevelPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var policyCoverageLevelGroup = context.AddGroup(
            PolicyCoverageLevelPermissions.GroupName,
            L("Permission:PolicyCoverageLevel")
        );

        var policyCoverageLevelPermission = policyCoverageLevelGroup.AddPermission(
            PolicyCoverageLevelPermissions.Default,
            L("Permission:PolicyCoverageLevel")
        );

        policyCoverageLevelPermission.AddChild(
            PolicyCoverageLevelPermissions.Create,
            L("Permission:Create")
        );

        policyCoverageLevelPermission.AddChild(
            PolicyCoverageLevelPermissions.Edit,
            L("Permission:Edit")
        );

        policyCoverageLevelPermission.AddChild(
            PolicyCoverageLevelPermissions.Delete,
            L("Permission:Delete")
        );

        policyCoverageLevelPermission.AddChild(
            PolicyCoverageLevelPermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<PolicyResource>(name);
    }
}
