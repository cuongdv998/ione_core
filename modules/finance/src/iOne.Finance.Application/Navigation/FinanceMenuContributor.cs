using System.Threading.Tasks;
using Volo.Abp.UI.Navigation;
using iOne.Finance.Localization;

namespace iOne.Finance.Navigation;

public class FinanceMenuContributor : IMenuContributor
{
    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name != StandardMenus.Main)
        {
            return;
        }

        var financeL = context.GetLocalizer<FinanceResource>();

        // TODO: Add Finance menu items here
        // Example:
        // var financeMenuItem = new ApplicationMenuItem(
        //     "Finance",
        //     financeL["Menu:Finance"],
        //     icon: "pi pi-fw pi-money-bill"
        // );
        // context.Menu.AddItem(financeMenuItem);

        await Task.CompletedTask;
    }
}

