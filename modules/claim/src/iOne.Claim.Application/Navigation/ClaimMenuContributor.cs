using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.UI.Navigation;
using iOne.Claim.Localization;
using iOne.Claim.Permissions;

namespace iOne.Claim.Navigation;

public class ClaimMenuContributor : IMenuContributor
{
    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name != StandardMenus.Main)
        {
            return;
        }

        var claimL = context.GetLocalizer<ClaimResource>();

        // Claim Menu
        var claimMenuItem = new ApplicationMenuItem(
            "Claim",
            claimL["Menu:Claim"],
            icon: "pi pi-fw pi-file"
        );
        context.Menu.AddItem(claimMenuItem);

        // Claim List Menu Item
        claimMenuItem.AddItem(new ApplicationMenuItem(
            "Claim.ClaimList",
            claimL["Menu:ClaimList"],
            url: "~/pages/claim/list",
            icon: "pi pi-fw pi-list"
        ).RequirePermissions(ClaimPermissions.Default));

        // Claim Task List Menu Item (danh sách yêu cầu được giao)
        claimMenuItem.AddItem(new ApplicationMenuItem(
            "Claim.ClaimTaskList",
            claimL["Menu:ClaimTaskList"],
            url: "~/pages/claim/task-list",
            icon: "pi pi-fw pi-tasks"
        ).RequirePermissions(ClaimPermissions.Default));

        // Onsite Assessment Task List Menu Item (danh sách giám định hiện trường)
        claimMenuItem.AddItem(new ApplicationMenuItem(
            "Claim.OnsiteAssessmentTaskList",
            claimL["Menu:OnsiteAssessmentTaskList"],
            url: "~/pages/claim/onsite-assessment-list",
            icon: "pi pi-fw pi-map-marker"
        ).RequirePermissions(ClaimPermissions.Default));

        // Detail Assessment Task List Menu Item (danh sách giám định chi tiết)
        claimMenuItem.AddItem(new ApplicationMenuItem(
            "Claim.DetailAssessmentTaskList",
            claimL["Menu:DetailAssessmentTaskList"],
            url: "~/pages/claim/detail-assessment-list",
            icon: "pi pi-fw pi-file-edit"
        ).RequirePermissions(ClaimPermissions.Default));

        claimMenuItem.AddItem(new ApplicationMenuItem(
            "Claim.QuotationApprovalList",
            claimL["Menu:QuotationApprovalList"],
            url: "~/pages/claim/quotation-approval-list",
            icon: "pi pi-fw pi-check-square"
        ).RequirePermissions(ClaimPermissions.QuotationApprovalList));

        await Task.CompletedTask;
    }
}
