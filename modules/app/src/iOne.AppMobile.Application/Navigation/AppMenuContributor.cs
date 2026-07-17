using System.Threading.Tasks;
using Volo.Abp.UI.Navigation;
using iOne.AppMobile.Localization;

namespace iOne.AppMobile.Navigation;

public class AppMobileMenuContributor : IMenuContributor
{
    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name != StandardMenus.Main)
        {
            return;
        }

        var appMobileL = context.GetLocalizer<AppMobileResource>();

        // TODO: Add AppMobile menu items here
        // Example:
        // var appMobileMenuItem = new ApplicationMenuItem(
        //     "AppMobile",
        //     appMobileL["Menu:AppMobile"],
        //     icon: "pi pi-fw pi-check-square"
        // );
        // context.Menu.AddItem(appMobileMenuItem);

        await Task.CompletedTask;
    }
}
