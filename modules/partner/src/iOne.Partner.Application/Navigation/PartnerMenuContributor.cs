using System.Threading.Tasks;
using Volo.Abp.UI.Navigation;
using Volo.Abp.Authorization.Permissions;
using iOne.Partner.Localization;
using iOne.Partner.Permissions;

namespace iOne.Partner.Navigation;

public class PartnerMenuContributor : IMenuContributor
{
    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name != StandardMenus.Main)
        {
            return;
        }

        var partnerL = context.GetLocalizer<PartnerResource>();

        // Partner Menu
        var partnerMenuItem = new ApplicationMenuItem(
            "Partner",
            partnerL["Menu:Partner"],
            icon: "pi pi-fw pi-building-columns"
        );
        context.Menu.AddItem(partnerMenuItem);

        // ResPartnerType Menu Item
        partnerMenuItem.AddItem(new ApplicationMenuItem(
            "Partner.ResPartnerType",
            partnerL["Menu:ResPartnerType"],
            url: "~/pages/partner/partner-types",
            icon: "pi pi-fw pi-tag"
        ).RequirePermissions(ResPartnerTypePermissions.Default));

        // ResOrganizationType Menu Item
        partnerMenuItem.AddItem(new ApplicationMenuItem(
            "Partner.ResOrganizationType",
            partnerL["Menu:ResOrganizationType"],
            url: "~/pages/partner/res-organization-types",
            icon: "pi pi-fw pi-building"
        ).RequirePermissions(ResOrganizationTypePermissions.Default));

        // ResChannel Menu Item
        partnerMenuItem.AddItem(new ApplicationMenuItem(
            "Partner.ResChannel",
            partnerL["Menu:ResChannel"],
            url: "~/pages/partner/channels",
            icon: "pi pi-fw pi-sitemap"
        ).RequirePermissions(ResChannelPermissions.Default));

        // ResAgreementTerm Menu Item
        partnerMenuItem.AddItem(new ApplicationMenuItem(
            "Partner.ResAgreementTerm",
            partnerL["Menu:ResAgreementTerm"],
            url: "~/pages/partner/agreement-terms",
            icon: "pi pi-fw pi-file-edit"
        ).RequirePermissions(ResAgreementTermPermissions.Default));

        // ResPartner Menu Item
        partnerMenuItem.AddItem(new ApplicationMenuItem(
            "Partner.ResPartner",
            partnerL["Menu:ResPartner"],
            url: "~/pages/partner/res-partners",
            icon: "pi pi-fw pi-users"
        ).RequirePermissions(ResPartnerPermissions.Default));

        await Task.CompletedTask;
    }
}

