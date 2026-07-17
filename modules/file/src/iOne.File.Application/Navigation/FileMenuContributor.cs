using System.Threading.Tasks;
using Volo.Abp.UI.Navigation;
using iOne.File.Localization;

namespace iOne.File.Navigation;

public class FileMenuContributor : IMenuContributor
{
    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name != StandardMenus.Main)
        {
            return;
        }

        var fileL = context.GetLocalizer<FileResource>();

        // TODO: Add File menu items here
        // Example:
        // var fileMenuItem = new ApplicationMenuItem(
        //     "File",
        //     fileL["Menu:File"],
        //     icon: "pi pi-fw pi-file"
        // );
        // context.Menu.AddItem(fileMenuItem);

        await Task.CompletedTask;
    }
}

