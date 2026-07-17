using iOne.Policy.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Policy.Permissions;

public class PolicyAmountPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var policyAmountGroup = context.AddGroup(
            PolicyAmountPermissions.GroupName,
            L("Permission:PolicyAmount")
        );

        var policyAmountPermission = policyAmountGroup.AddPermission(
            PolicyAmountPermissions.Default,
            L("Permission:PolicyAmount")
        );

        policyAmountPermission.AddChild(
            PolicyAmountPermissions.Create,
            L("Permission:Create")
        );

        policyAmountPermission.AddChild(
            PolicyAmountPermissions.Edit,
            L("Permission:Edit")
        );

        policyAmountPermission.AddChild(
            PolicyAmountPermissions.Delete,
            L("Permission:Delete")
        );

        policyAmountPermission.AddChild(
            PolicyAmountPermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<PolicyResource>(name);
    }
}
