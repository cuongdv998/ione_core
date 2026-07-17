using System.Threading.Tasks;
using Volo.Abp.UI.Navigation;
using iOne.Payment.Localization;

namespace iOne.Payment.Navigation;

public class PaymentMenuContributor : IMenuContributor
{
    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name != StandardMenus.Main)
        {
            return;
        }

        var paymentL = context.GetLocalizer<PaymentResource>();

        // TODO: Add Payment menu items here
        // Example:
        // var paymentMenuItem = new ApplicationMenuItem(
        //     "Payment",
        //     paymentL["Menu:Payment"],
        //     icon: "pi pi-fw pi-money-bill"
        // );
        // context.Menu.AddItem(paymentMenuItem);

        await Task.CompletedTask;
    }
}

