using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;

namespace iOne.Workflow;

[DependsOn(
    typeof(iOneWorkflowApplicationContractsModule),
    typeof(iOneWorkflowApplicationModule),
    typeof(AbpAspNetCoreMvcModule)
)]
public class iOneWorkflowHttpApiModule : AbpModule
{
}
