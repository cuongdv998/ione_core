using System.Threading.Tasks;
using Volo.Abp.UI.Navigation;
using Volo.Abp.Authorization.Permissions;
using iOne.Hr.Localization;
using iOne.Hr.Permissions;

namespace iOne.Hr.Navigation;

public class HrMenuContributor : IMenuContributor
{
    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name != StandardMenus.Main)
        {
            return;
        }

        var hrL = context.GetLocalizer<HrResource>();

        // HR Menu
        var hrMenuItem = new ApplicationMenuItem(
            "HR",
            hrL["Menu:HR"],
            icon: "pi pi-fw pi-briefcase"
        );
        context.Menu.AddItem(hrMenuItem);

        hrMenuItem.AddItem(new ApplicationMenuItem(
            "HR.DepartmentTypes",
            hrL["Menu:DepartmentTypes"],
            url: "~/pages/hr/department-types",
            icon: "pi pi-fw pi-building"
        ).RequirePermissions(HrDepartmentTypePermissions.Default));

        hrMenuItem.AddItem(new ApplicationMenuItem(
            "HR.Departments",
            hrL["Menu:Departments"],
            url: "~/pages/hr/departments",
            icon: "pi pi-fw pi-sitemap"
        ).RequirePermissions(HrDepartmentPermissions.Default));

        hrMenuItem.AddItem(new ApplicationMenuItem(
            "HR.EmployeeRoles",
            hrL["Menu:EmployeeRoles"],
            url: "~/pages/hr/employee-roles",
            icon: "pi pi-fw pi-user-edit"
        ).RequirePermissions(HrEmployeeRolePermissions.Default));

        hrMenuItem.AddItem(new ApplicationMenuItem(
            "HR.EmployeeLevels",
            hrL["Menu:EmployeeLevels"],
            url: "~/pages/hr/employee-levels",
            icon: "pi pi-fw pi-sort-amount-up"
        ).RequirePermissions(HrEmployeeLevelPermissions.Default));

        hrMenuItem.AddItem(new ApplicationMenuItem(
            "HR.EmployeePositions",
            hrL["Menu:EmployeePositions"],
            url: "~/pages/hr/employee-positions",
            icon: "pi pi-fw pi-briefcase"
        ).RequirePermissions(HrEmployeePositionPermissions.Default));

        hrMenuItem.AddItem(new ApplicationMenuItem(
            "HR.Employees",
            hrL["Menu:Employees"],
            url: "~/pages/hr/employees",
            icon: "pi pi-fw pi-users"
        ).RequirePermissions(HrEmployeePermissions.Default));

        await Task.CompletedTask;
    }
}

