using Volo.Abp.Authorization;
using Volo.Abp.Modularity;
using Volo.Abp.Users;

namespace Volo.Abp.AuditLogging;

[DependsOn(
    typeof(AbpAuditLoggingDomainSharedModule),
    typeof(AbpUsersAbstractionModule),
    typeof(AbpAuthorizationModule)
    )]
public class AbpAuditLoggingApplicationContractsModule : AbpModule
{
}

