using iOne.Customer.Localization;
using iOne.Customer.Permissions;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Customer.Permissions;

public class ResIndustryPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var industryGroup = context.AddGroup(
            ResIndustryPermissions.GroupName,
            L("Permission:ResIndustry")
        );

        var industryPermission = industryGroup.AddPermission(
            ResIndustryPermissions.Default,
            L("Permission:ResIndustry")
        );

        industryPermission.AddChild(
            ResIndustryPermissions.Create,
            L("Permission:Create")
        );

        industryPermission.AddChild(
            ResIndustryPermissions.Edit,
            L("Permission:Edit")
        );

        industryPermission.AddChild(
            ResIndustryPermissions.Delete,
            L("Permission:Delete")
        );

        industryPermission.AddChild(
            ResIndustryPermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<CustomerResource>(name);
    }
}


