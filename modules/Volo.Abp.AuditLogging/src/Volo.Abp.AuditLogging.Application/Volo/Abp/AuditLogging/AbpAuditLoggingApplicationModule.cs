using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace Volo.Abp.AuditLogging;

[DependsOn(
    typeof(AbpAuditLoggingDomainModule),
    typeof(AbpAuditLoggingApplicationContractsModule),
    typeof(Volo.Abp.AutoMapper.AbpAutoMapperModule)
    )]
public class AbpAuditLoggingApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAutoMapperObjectMapper<AbpAuditLoggingApplicationModule>();

        Configure<Volo.Abp.AutoMapper.AbpAutoMapperOptions>(options =>
        {
            options.AddProfile<AbpAuditLoggingApplicationModuleAutoMapperProfile>(validate: true);
        });
    }
}

