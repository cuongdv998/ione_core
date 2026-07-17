using iOne.Report.Localization;
using iOne.Report.Permissions;
using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.UI.Navigation;

namespace iOne.Report.Navigation;

public class ReportMenuContributor : IMenuContributor
{
    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name != StandardMenus.Main)
        {
            return;
        }

        var reportL = context.GetLocalizer<ReportResource>();

        // TODO: Add Report menu items here
        // Example:
        var reportMenuItem = new ApplicationMenuItem(
            "Report",
            reportL["Menu:Report"],
            icon: "pi pi-fw pi-chart-bar"
        );
        context.Menu.AddItem(reportMenuItem);

        reportMenuItem.AddItem(new ApplicationMenuItem("Report.ReportTemplate", reportL["Menu:ReportTemplate"], url: "~/pages/report/template", icon: "pi pi-fw pi-sitemap").RequirePermissions(ReportTemplatePermissions.Default));
        //reportMenuItem.AddItem(new ApplicationMenuItem("Report.ReportTemplateSql", reportL["Menu:ReportTemplate"], url: "~/pages/report/sql", icon: "pi pi-fw pi-sitemap").RequirePermissions(ReportTemplatePermissions.Default));
        //reportMenuItem.AddItem(new ApplicationMenuItem("Report.ReportTemplateParameter", reportL["Menu:ReportTemplate"], url: "~/pages/report/parameter", icon: "pi pi-fw pi-sitemap").RequirePermissions(ReportTemplatePermissions.Default));

        await Task.CompletedTask;
    }
}

