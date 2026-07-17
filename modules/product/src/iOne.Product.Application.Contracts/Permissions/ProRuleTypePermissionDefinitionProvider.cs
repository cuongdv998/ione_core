using iOne.Product.Localization;
using iOne.Product.Permissions;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Product.Permissions;

public class ProRuleTypePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var ruleTypeGroup = context.AddGroup(
            ProRuleTypePermissions.GroupName,
            L("Permission:ProRuleType")
        );

        var ruleTypePermission = ruleTypeGroup.AddPermission(
            ProRuleTypePermissions.Default,
            L("Permission:ProRuleType")
        );

        ruleTypePermission.AddChild(
            ProRuleTypePermissions.Create,
            L("Permission:Create")
        );

        ruleTypePermission.AddChild(
            ProRuleTypePermissions.Edit,
            L("Permission:Edit")
        );

        ruleTypePermission.AddChild(
            ProRuleTypePermissions.Delete,
            L("Permission:Delete")
        );

        ruleTypePermission.AddChild(
            ProRuleTypePermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<ProductResource>(name);
    }
}
