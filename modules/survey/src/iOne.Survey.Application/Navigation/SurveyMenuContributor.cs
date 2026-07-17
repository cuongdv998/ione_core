using System.Threading.Tasks;
using Volo.Abp.UI.Navigation;
using iOne.Survey.Localization;

namespace iOne.Survey.Navigation;

public class SurveyMenuContributor : IMenuContributor
{
    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name != StandardMenus.Main)
        {
            return;
        }

        var surveyL = context.GetLocalizer<SurveyResource>();

        // TODO: Add Survey menu items here
        // Example:
        // var surveyMenuItem = new ApplicationMenuItem(
        //     "Survey",
        //     surveyL["Menu:Survey"],
        //     icon: "pi pi-fw pi-check-square"
        // );
        // context.Menu.AddItem(surveyMenuItem);

        await Task.CompletedTask;
    }
}

