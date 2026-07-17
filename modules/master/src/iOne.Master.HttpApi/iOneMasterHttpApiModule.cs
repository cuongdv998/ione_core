using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement.HttpApi;
using Volo.Abp.SettingManagement;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Account;
using Volo.Abp.Identity;
using Volo.Abp.AuditLogging;

namespace iOne.Master;

[DependsOn(
    typeof(iOneMasterApplicationContractsModule),
    typeof(iOneMasterApplicationModule), // Cần reference để access Assembly
    typeof(AbpPermissionManagementHttpApiModule),
    typeof(AbpSettingManagementHttpApiModule),
    typeof(AbpAccountHttpApiModule),
    typeof(AbpIdentityHttpApiModule),
    typeof(AbpFeatureManagementHttpApiModule),
    typeof(AbpAuditLoggingHttpApiModule)
    )]
public class iOneMasterHttpApiModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            // Create conventional controllers for module, but exclude services that have manual controllers
            options.ConventionalControllers.Create(
                typeof(iOneMasterApplicationModule).Assembly,
                opts =>
                {
                    opts.TypePredicate = type =>
                        type.Name != "ResCountryAppService" &&
                        type.Name != "ResCarBrandAppService" &&
                        type.Name != "ResCarLineAppService" &&
                        type.Name != "ResCarModelAppService" &&
                        type.Name != "ResCarTypeAppService" &&
                        type.Name != "ResCarCategoryAppService" &&
                        type.Name != "ResProvinceAppService" &&
                        type.Name != "ResWardAppService" &&
                        type.Name != "ResDocumentTypeAppService" &&
                        type.Name != "ResBankAppService" &&
                        type.Name != "ResObjectTypeAppService" &&
                        type.Name != "ResMotorClassAppService" &&
                        type.Name != "ResRiskAppService" &&
                        type.Name != "ResSequenceAppService" &&
                        type.Name != "ResUomAppService" &&
                        type.Name != "ResObjectItemTypeAppService" &&
                        type.Name != "ResObjectTypeItemAppService" &&
                        type.Name != "ResObjectItemDepreciationAppService" &&
                        type.Name != "ResCarGroupAppService" &&
                        type.Name != "InsurerDictionaryAppService" &&
                        type.Name != "AdminConfigAppService" &&
                        type.Name != "ResDocumentAppService" &&
                        type.Name != "ResEventAppService" &&
                        type.Name != "ResEventNotifyTemplateAppService" &&
                        type.Name != "ResUserDeviceAppService" &&
                        type.Name != "SystemEventNotifyAppService" &&
                        type.Name != "ResBusinessAuthorityAppService" &&
                        type.Name != "ResBusinessAssigneeAppService" &&
                        type.Name != "BusinessFlowAppService" &&
                        type.Name != "ResTaskCategoryAppService"; // Exclude AppService có manual controller
                });
        });
    }
}

