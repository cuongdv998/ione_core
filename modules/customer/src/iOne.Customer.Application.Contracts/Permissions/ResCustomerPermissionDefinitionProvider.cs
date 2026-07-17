using iOne.Customer.Localization;
using iOne.Customer.Permissions;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Customer.Permissions;

public class ResCustomerPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var resCustomerGroup = context.AddGroup(
            ResCustomerPermissions.GroupName,
            L("Permission:ResCustomer")
        );

        var resCustomerPermission = resCustomerGroup.AddPermission(
            ResCustomerPermissions.Default,
            L("Permission:ResCustomer")
        );

        resCustomerPermission.AddChild(
            ResCustomerPermissions.Create,
            L("Permission:Create")
        );

        resCustomerPermission.AddChild(
            ResCustomerPermissions.Edit,
            L("Permission:Edit")
        );

        resCustomerPermission.AddChild(
            ResCustomerPermissions.Delete,
            L("Permission:Delete")
        );

        resCustomerPermission.AddChild(
            ResCustomerPermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<CustomerResource>(name);
    }
}

