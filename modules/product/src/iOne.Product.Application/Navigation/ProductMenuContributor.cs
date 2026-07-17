using System.Threading.Tasks;
using Volo.Abp.UI.Navigation;
using iOne.Product.Localization;
using iOne.Product.Permissions;
using Volo.Abp.Authorization.Permissions;

namespace iOne.Product.Navigation;

public class ProductMenuContributor : IMenuContributor
{
    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name != StandardMenus.Main)
        {
            return;
        }

        var productL = context.GetLocalizer<ProductResource>();

        // Product Menu
        var productMenuItem = new ApplicationMenuItem(
            "Product",
            productL["Menu:Product"],
            icon: "pi pi-fw pi-box"
        );
        context.Menu.AddItem(productMenuItem);

        productMenuItem.AddItem(new ApplicationMenuItem(
            "Product.ProLineOfBusiness",
            productL["Menu:ProLineOfBusiness"],
            url: "~/pages/product/line-of-businesses",
            icon: "pi pi-fw pi-sitemap"
        ).RequirePermissions(ProLineOfBusinessPermissions.Default));

        productMenuItem.AddItem(new ApplicationMenuItem(
            "Product.ProCoverageType",
            productL["Menu:ProCoverageType"],
            url: "~/pages/product/coverage-types",
            icon: "pi pi-fw pi-shield"
        ).RequirePermissions(ProCoverageTypePermissions.Default));

        productMenuItem.AddItem(new ApplicationMenuItem(
            "Product.ProCoverageGroup",
            productL["Menu:ProCoverageGroup"],
            url: "~/pages/product/pro-coverage-groups",
            icon: "pi pi-fw pi-table"
        ).RequirePermissions(ProCoverageGroupPermissions.Default));
        
        productMenuItem.AddItem(new ApplicationMenuItem(
            "Product.ProProductCategory",
            productL["Menu:ProProductCategory"],
            url: "~/pages/product/pro-product-categories",
            icon: "pi pi-fw pi-table"
        ).RequirePermissions(ProProductCategoryPermissions.Default));
        
        productMenuItem.AddItem(new ApplicationMenuItem(
            "Product.ProProductType",
            productL["Menu:ProProductType"],
            url: "~/pages/product/pro-product-types",
            icon: "pi pi-fw pi-tags"
        ).RequirePermissions(ProProductTypePermissions.Default));
        
        productMenuItem.AddItem(new ApplicationMenuItem(
            "Product.ProProduct",
            productL["Menu:ProProduct"],
            url: "~/pages/product/pro-products",
            icon: "pi pi-fw pi-shopping-bag"
        ).RequirePermissions(ProProductPermissions.Default));
        
        productMenuItem.AddItem(new ApplicationMenuItem(
            "Product.ProAttribute",
            productL["Menu:ProAttribute"],
            url: "~/pages/product/pro-attributes",
            icon: "pi pi-fw pi-cog"
        ).RequirePermissions(ProAttributePermissions.Default));
        
        productMenuItem.AddItem(new ApplicationMenuItem(
            "Product.ProCoverage",
            productL["Menu:ProCoverage"],
            url: "~/pages/product/pro-coverages",
            icon: "pi pi-fw pi-shield"
        ).RequirePermissions(ProCoveragePermissions.Default));
        
        productMenuItem.AddItem(new ApplicationMenuItem(
            "Product.ProTableRate",
            productL["Menu:ProTableRate"],
            url: "~/pages/product/pro-table-rates",
            icon: "pi pi-fw pi-table"
        ).RequirePermissions(ProTableRatePermissions.Default));
        
        productMenuItem.AddItem(new ApplicationMenuItem(
            "Product.ResTax",
            productL["Menu:ResTax"],
            url: "~/pages/product/res-taxes",
            icon: "pi pi-fw pi-percentage"
        ).RequirePermissions(ResTaxPermissions.Default));
        
        await Task.CompletedTask;
    }
}

