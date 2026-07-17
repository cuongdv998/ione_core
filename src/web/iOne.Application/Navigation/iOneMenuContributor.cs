using System.Threading.Tasks;
using Volo.Abp.UI.Navigation;
using Volo.Abp.Identity;
using Volo.Abp.Authorization.Permissions;
using iOne.Localization;
using iOne.Permissions;

namespace iOne.Navigation;

public class iOneMenuContributor : IMenuContributor
{
    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name != StandardMenus.Main)
        {
            return;
        }

        var l = context.GetLocalizer<iOneResource>();

        // Administration Menu
        var administrationMenu = context.Menu.GetAdministration();

        administrationMenu.Icon = "pi pi-fw pi-cog";

        // Identity Management
        var identityMenuItem = new ApplicationMenuItem(
            "IdentityManagement",
            l["Menu:IdentityManagement"],
            icon: "pi pi-fw pi-id-card"
        );
        administrationMenu.AddItem(identityMenuItem);

        identityMenuItem.AddItem(new ApplicationMenuItem(
            "Identity.Users",
            l["Menu:Users"],
            url: "~/pages/users"
        ).RequirePermissions(IdentityPermissions.Users.Default));

        identityMenuItem.AddItem(new ApplicationMenuItem(
            "Identity.Roles",
            l["Menu:Roles"],
            url: "~/pages/roles"
        ).RequirePermissions(IdentityPermissions.Roles.Default));

        // System settings (trước Tra cứu tác động)
        administrationMenu.AddItem(new ApplicationMenuItem(
            "App.SystemSettings",
            l["Menu:SystemSettings"],
            url: "~/pages/system-settings",
            icon: "pi pi-fw pi-sliders-h"
        ).RequirePermissions(iOnePermissions.AppSetting.View));

        // Audit Logs
        administrationMenu.AddItem(new ApplicationMenuItem(
            "AuditLogs",
            l["Menu:AuditLogs"],
            url: "~/pages/audit-logs",
            icon: "pi pi-fw pi-history"
        ).RequirePermissions("AbpAuditLogging.AuditLogs"));

        await Task.CompletedTask;
    }
}

