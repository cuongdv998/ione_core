using System.Threading.Tasks;
using Volo.Abp.UI.Navigation;
using Volo.Abp.Authorization.Permissions;
using iOne.Customer.Localization;
using iOne.Customer.Permissions;

namespace iOne.Customer.Navigation;

public class CustomerMenuContributor : IMenuContributor
{
    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name != StandardMenus.Main)
        {
            return;
        }

        var customerL = context.GetLocalizer<CustomerResource>();

        // Customer Menu
        var customerMenuItem = new ApplicationMenuItem(
            "Customer",
            customerL["Menu:Customer"],
            icon: "pi pi-fw pi-users"
        );
        context.Menu.AddItem(customerMenuItem);

        // Industries Menu Item
        customerMenuItem.AddItem(new ApplicationMenuItem(
            "Customer.Industries",
            customerL["Menu:Industries"],
            url: "~/pages/customer/industries",
            icon: "pi pi-fw pi-briefcase"
        ).RequirePermissions(ResIndustryPermissions.Default));

        // ResCustomer Menu Item
        customerMenuItem.AddItem(new ApplicationMenuItem(
            "Customer.ResCustomer",
            customerL["Menu:ResCustomer"],
            url: "~/pages/customer/res-customers",
            icon: "pi pi-fw pi-users"
        ).RequirePermissions(ResCustomerPermissions.Default));

        await Task.CompletedTask;
    }
}

