using System.Threading.Tasks;
using Volo.Abp.UI.Navigation;
using iOne.Policy.Localization;
using iOne.Policy.Permissions;
using Volo.Abp.Authorization.Permissions;

namespace iOne.Policy.Navigation;

public class PolicyMenuContributor : IMenuContributor
{
    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name != StandardMenus.Main)
        {
            return;
        }

        var policyL = context.GetLocalizer<PolicyResource>();

        // Policy Menu
        var policyMenuItem = new ApplicationMenuItem(
            "Policy",
            policyL["Menu:PolicyGroup"],
            icon: "pi pi-fw pi-file-edit"
        );
        context.Menu.AddItem(policyMenuItem);

        policyMenuItem.AddItem(new ApplicationMenuItem(
            "Policy.Policy",
            policyL["Menu:Policy"],
            url: "~/pages/policy/policies",
            icon: "pi pi-fw pi-file"
        ).RequirePermissions(PolicyPermissions.Default));

        policyMenuItem.AddItem(new ApplicationMenuItem(
            "Policy.MotorbikePolicy",
            policyL["Menu:MotorbikePolicy"],
            url: "~/pages/policy/motorbike-policies",
            icon: "pi pi-fw pi-shield"
        ).RequirePermissions(PolicyPermissions.Default));

        policyMenuItem.AddItem(new ApplicationMenuItem(
            "Policy.RequestApproval",
            policyL["Menu:RequestApproval"],
            url: "~/pages/policy/request-approval",
            icon: "pi pi-fw pi-check-square"
        ).RequirePermissions(PolicyPermissions.RequestApproval.View));

        policyMenuItem.AddItem(new ApplicationMenuItem(
            "Policy.ContractList",
            policyL["Menu:ContractList"],
            url: "~/pages/policy/contract-list",
            icon: "pi pi-fw pi-list"
        ).RequirePermissions(PolicyContractPermissions.Default));

        await Task.CompletedTask;
    }
}

