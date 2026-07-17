using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.UI.Navigation;
using iOne.Master.Localization;
using iOne.Master.Permissions;
using iOne.Policy.Permissions;

namespace iOne.Master.Navigation;

public class MasterMenuContributor : IMenuContributor
{
    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name != StandardMenus.Main)
        {
            return;
        }

        var masterL = context.GetLocalizer<MasterResource>();

        // Master Menu
        var masterMenuItem = new ApplicationMenuItem(
            "Master",
            masterL["Menu:Master"],
            icon: "pi pi-fw pi-cog"
        );
        context.Menu.AddItem(masterMenuItem);

        // Danh mục Địa bàn (Territory) - submenu: Quốc gia, Tỉnh/Thành, Phường/Xã
        var territoryMenuItem = new ApplicationMenuItem(
            "Master.Territory",
            masterL["Menu:Territory"],
            icon: "pi pi-fw pi-map"
        );
        territoryMenuItem.AddItem(new ApplicationMenuItem(
            "Master.ResCountry",
            masterL["Menu:ResCountry"],
            url: "~/pages/master/countries",
            icon: "pi pi-fw pi-globe"
        ).RequirePermissions(ResCountryPermissions.Default));
        territoryMenuItem.AddItem(new ApplicationMenuItem(
            "Master.ResProvince",
            masterL["Menu:ResProvince"],
            url: "~/pages/master/provinces",
            icon: "pi pi-fw pi-map"
        ).RequirePermissions(ResProvincePermissions.Default));
        territoryMenuItem.AddItem(new ApplicationMenuItem(
            "Master.ResWard",
            masterL["Menu:ResWard"],
            url: "~/pages/master/wards",
            icon: "pi pi-fw pi-map-marker"
        ).RequirePermissions(ResWardPermissions.Default));
        masterMenuItem.AddItem(territoryMenuItem);

        // Danh mục Cấu hình (Configuration) - submenu: Loại tài liệu, Cấu hình chung, Nhóm lý do, Lý do, Cấu hình mã tự sinh, Nhóm đơn vị tính, Đơn vị tính
        var configMenuItem = new ApplicationMenuItem(
            "Master.Config",
            masterL["Menu:Config"],
            icon: "pi pi-fw pi-cog"
        );
        configMenuItem.AddItem(new ApplicationMenuItem(
            "Master.ResDocumentType",
            masterL["Menu:ResDocumentType"],
            url: "~/pages/master/document-types",
            icon: "pi pi-fw pi-file"
        ).RequirePermissions(ResDocumentTypePermissions.Default));
        configMenuItem.AddItem(new ApplicationMenuItem(
            "Master.AdminConfig",
            masterL["Menu:AdminConfig"],
            url: "~/pages/master/admin-configs",
            icon: "pi pi-fw pi-cog"
        ).RequirePermissions(AdminConfigPermissions.Default));
        configMenuItem.AddItem(new ApplicationMenuItem(
            "Master.ResReasonGroup",
            masterL["Menu:ResReasonGroup"],
            url: "~/pages/master/res-reason-groups",
            icon: "pi pi-fw pi-list"
        ).RequirePermissions(ResReasonGroupPermissions.Default));
        configMenuItem.AddItem(new ApplicationMenuItem(
            "Master.ResReason",
            masterL["Menu:ResReason"],
            url: "~/pages/master/res-reasons",
            icon: "pi pi-fw pi-tag"
        ).RequirePermissions(ResReasonPermissions.Default));
        configMenuItem.AddItem(new ApplicationMenuItem(
            "Master.ResSequence",
            masterL["Menu:ResSequence"],
            url: "~/pages/master/res-sequences",
            icon: "pi pi-fw pi-cog"
        ).RequirePermissions(ResSequencePermissions.Default));
        configMenuItem.AddItem(new ApplicationMenuItem(
            "Master.ResUomClass",
            masterL["Menu:ResUomClass"],
            url: "~/pages/master/uom-classes",
            icon: "pi pi-fw pi-th"
        ).RequirePermissions(ResUomClassPermissions.Default));
        configMenuItem.AddItem(new ApplicationMenuItem(
            "Master.ResUom",
            masterL["Menu:ResUom"],
            url: "~/pages/master/res-uoms",
            icon: "pi pi-fw pi-th"
        ).RequirePermissions(ResUomPermissions.Default));
        masterMenuItem.AddItem(configMenuItem);

        // Danh mục Xe (Vehicle) - submenu: Hãng xe, Dòng xe, Nhóm xe, Model xe, Loại xe, Phiên bản xe, Phân loại động cơ
        var vehicleMenuItem = new ApplicationMenuItem(
            "Master.Vehicle",
            masterL["Menu:Vehicle"],
            icon: "pi pi-fw pi-car"
        );
        vehicleMenuItem.AddItem(new ApplicationMenuItem(
            "Master.ResCarBrand",
            masterL["Menu:ResCarBrand"],
            url: "~/pages/master/car-brands",
            icon: "pi pi-fw pi-car"
        ).RequirePermissions(ResCarBrandPermissions.Default));
        vehicleMenuItem.AddItem(new ApplicationMenuItem(
            "Master.ResCarLine",
            masterL["Menu:ResCarLine"],
            url: "~/pages/master/car-lines",
            icon: "pi pi-fw pi-car"
        ).RequirePermissions(ResCarLinePermissions.Default));
        vehicleMenuItem.AddItem(new ApplicationMenuItem(
            "Master.ResCarGroup",
            masterL["Menu:ResCarGroup"],
            url: "~/pages/master/car-groups",
            icon: "pi pi-fw pi-car"
        ).RequirePermissions(ResCarGroupPermissions.Default));
        vehicleMenuItem.AddItem(new ApplicationMenuItem(
            "Master.ResCarModel",
            masterL["Menu:ResCarModel"],
            url: "~/pages/master/car-models",
            icon: "pi pi-fw pi-car"
        ).RequirePermissions(ResCarModelPermissions.Default));
        vehicleMenuItem.AddItem(new ApplicationMenuItem(
            "Master.ResCarType",
            masterL["Menu:ResCarType"],
            url: "~/pages/master/car-types",
            icon: "pi pi-fw pi-car"
        ).RequirePermissions(ResCarTypePermissions.Default));
        vehicleMenuItem.AddItem(new ApplicationMenuItem(
            "Master.ResCarCategory",
            masterL["Menu:ResCarCategory"],
            url: "~/pages/master/car-categories",
            icon: "pi pi-fw pi-car"
        ).RequirePermissions(ResCarCategoryPermissions.Default));
        vehicleMenuItem.AddItem(new ApplicationMenuItem(
            "Master.ResMotorClass",
            masterL["Menu:ResMotorClass"],
            url: "~/pages/master/motor-classes",
            icon: "pi pi-fw pi-cog"
        ).RequirePermissions(ResMotorClassPermissions.Default));
        masterMenuItem.AddItem(vehicleMenuItem);

        // Danh mục Thiết lập tài chính (Financial Setup) - submenu: Ngân hàng, Tiền tệ, Khoản phí, Hình thức thanh toán
        var financialMenuItem = new ApplicationMenuItem(
            "Master.Financial",
            masterL["Menu:Financial"],
            icon: "pi pi-fw pi-dollar"
        );
        financialMenuItem.AddItem(new ApplicationMenuItem(
            "Master.ResBank",
            masterL["Menu:ResBank"],
            url: "~/pages/master/banks",
            icon: "pi pi-fw pi-building"
        ).RequirePermissions(ResBankPermissions.Default));
        financialMenuItem.AddItem(new ApplicationMenuItem(
            "Master.ResCurrency",
            masterL["Menu:ResCurrency"],
            url: "~/pages/master/res-currencies",
            icon: "pi pi-fw pi-dollar"
        ).RequirePermissions(ResCurrencyPermissions.Default));
        financialMenuItem.AddItem(new ApplicationMenuItem(
            "Master.ResFeeItem",
            masterL["Menu:ResFeeItem"],
            url: "~/pages/master/res-fee-items",
            icon: "pi pi-fw pi-money-bill"
        ).RequirePermissions(ResFeeItemPermissions.Default));
        financialMenuItem.AddItem(new ApplicationMenuItem(
            "Master.ResPaymentMethod",
            masterL["Menu:ResPaymentMethod"],
            url: "~/pages/master/res-payment-methods",
            icon: "pi pi-fw pi-credit-card"
        ).RequirePermissions(ResPaymentMethodPermissions.Default));
        financialMenuItem.AddItem(new ApplicationMenuItem(
            "Master.ResPaymentType",
            masterL["Menu:ResPaymentType"],
            url: "~/pages/master/res-payment-types",
            icon: "pi pi-fw pi-tags"
        ).RequirePermissions(ResPaymentTypePermissions.Default));
        masterMenuItem.AddItem(financialMenuItem);

        // Danh mục Đối tượng bảo hiểm (Insurance Object) - submenu: Loại đối tượng, Rủi ro, Mapping bảo hiểm gốc, Mức độ tổn thất, Loại hạng mục, Hạng mục theo đối tượng, Khấu hao, Loại đơn
        var insuranceObjectMenuItem = new ApplicationMenuItem(
            "Master.InsuranceObject",
            masterL["Menu:InsuranceObject"],
            icon: "pi pi-fw pi-shield"
        );
        insuranceObjectMenuItem.AddItem(new ApplicationMenuItem(
            "Master.ResObjectType",
            masterL["Menu:ResObjectType"],
            url: "~/pages/master/object-types",
            icon: "pi pi-fw pi-tag"
        ).RequirePermissions(ResObjectTypePermissions.Default));
        insuranceObjectMenuItem.AddItem(new ApplicationMenuItem(
            "Master.ResRisk",
            masterL["Menu:ResRisk"],
            url: "~/pages/master/risks",
            icon: "pi pi-fw pi-exclamation-triangle"
        ).RequirePermissions(ResRiskPermissions.Default));
        insuranceObjectMenuItem.AddItem(new ApplicationMenuItem(
            "Master.InsurerDictionary",
            masterL["Menu:InsurerDictionary"],
            url: "~/pages/master/insurer-dictionaries",
            icon: "pi pi-fw pi-book"
        ).RequirePermissions(InsurerDictionaryPermissions.Default));
        insuranceObjectMenuItem.AddItem(new ApplicationMenuItem(
            "Master.ResDamageLevel",
            masterL["Menu:ResDamageLevel"],
            url: "~/pages/master/damage-levels",
            icon: "pi pi-fw pi-wrench"
        ).RequirePermissions(ResDamageLevelPermissions.Default));
        insuranceObjectMenuItem.AddItem(new ApplicationMenuItem(
            "Master.ResObjectItemType",
            masterL["Menu:ResObjectItemType"],
            url: "~/pages/master/res-object-item-types",
            icon: "pi pi-fw pi-tag"
        ).RequirePermissions(ResObjectItemTypePermissions.Default));
        insuranceObjectMenuItem.AddItem(new ApplicationMenuItem(
            "Master.ResObjectTypeItem",
            masterL["Menu:ResObjectTypeItem"],
            url: "~/pages/master/res-object-type-items",
            icon: "pi pi-fw pi-box"
        ).RequirePermissions(ResObjectTypeItemPermissions.Default));
        insuranceObjectMenuItem.AddItem(new ApplicationMenuItem(
            "Master.ResObjectItemDepreciation",
            masterL["Menu:ResObjectItemDepreciation"],
            url: "~/pages/master/res-object-item-depreciations",
            icon: "pi pi-fw pi-percentage"
        ).RequirePermissions(ResObjectItemDepreciationPermissions.Default));
        insuranceObjectMenuItem.AddItem(new ApplicationMenuItem(
            "Master.PolicyType",
            masterL["Menu:PolicyType"],
            url: "~/pages/policy/policy-types",
            icon: "pi pi-fw pi-tag"
        ).RequirePermissions(PolicyTypePermissions.Default));
        masterMenuItem.AddItem(insuranceObjectMenuItem);

        // Danh mục Tin nhắn (Message) - submenu: Kênh ứng dụng, Sự kiện, Thiết bị di động, Tra cứu tin nhắn
        var messageMenuItem = new ApplicationMenuItem(
            "Master.Message",
            masterL["Menu:Message"],
            icon: "pi pi-fw pi-envelope"
        );
        messageMenuItem.AddItem(new ApplicationMenuItem(
            "Master.ResAppChannel",
            masterL["Menu:ResAppChannel"],
            url: "~/pages/master/res-app-channels",
            icon: "pi pi-fw pi-mobile"
        ).RequirePermissions(ResAppChannelPermissions.Default));
        messageMenuItem.AddItem(new ApplicationMenuItem(
            "Master.ResEvent",
            masterL["Menu:ResEvent"],
            url: "~/pages/master/events",
            icon: "pi pi-fw pi-calendar"
        ).RequirePermissions(ResEventPermissions.Default));
        messageMenuItem.AddItem(new ApplicationMenuItem(
            "Master.ResUserDevice",
            masterL["Menu:ResUserDevice"],
            url: "~/pages/master/user-devices",
            icon: "pi pi-fw pi-mobile"
        ).RequirePermissions(ResUserDevicePermissions.Default));
        messageMenuItem.AddItem(new ApplicationMenuItem(
            "Master.SystemEventNotify",
            masterL["Menu:SystemEventNotify"],
            url: "~/pages/master/system-event-notifies",
            icon: "pi pi-fw pi-envelope"
        ).RequirePermissions(SystemEventNotifyPermissions.Default));
        masterMenuItem.AddItem(messageMenuItem);

        // Danh mục Quy trình (Process) - submenu
        var processMenuItem = new ApplicationMenuItem(
            "Master.Process",
            masterL["Menu:Process"],
            icon: "pi pi-fw pi-sitemap"
        );
        processMenuItem.AddItem(new ApplicationMenuItem(
            "Master.ResBusinessAuthority",
            masterL["Menu:ResBusinessAuthority"],
            url: "~/pages/master/res-business-authorities",
            icon: "pi pi-fw pi-shield"
        ).RequirePermissions(ResBusinessAuthorityPermissions.Default));
        processMenuItem.AddItem(new ApplicationMenuItem(
            "Master.BusinessFlow",
            masterL["Menu:BusinessFlow"],
            url: "~/pages/master/business-flows",
            icon: "pi pi-fw pi-sitemap"
        ).RequirePermissions(BusinessFlowPermissions.Default));
        processMenuItem.AddItem(new ApplicationMenuItem(
            "Master.ResBusinessAssignee",
            masterL["Menu:ResBusinessAssignee"],
            url: "~/pages/master/res-business-assignees",
            icon: "pi pi-fw pi-users"
        ).RequirePermissions(ResBusinessAssigneePermissions.Default));
        processMenuItem.AddItem(new ApplicationMenuItem(
            "Master.ResTaskCategory",
            masterL["Menu:ResTaskCategory"],
            url: "~/pages/master/res-task-categories",
            icon: "pi pi-fw pi-list"
        ).RequirePermissions(ResTaskCategoryPermissions.Default));
        masterMenuItem.AddItem(processMenuItem);

        await Task.CompletedTask;
    }
}

