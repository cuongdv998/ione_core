using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace iOne.Workflow;

[DependsOn(
    typeof(iOneWorkflowApplicationContractsModule)
)]
public class iOneWorkflowApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        Configure<ElsaWorkflowOptions>(configuration.GetSection(ElsaWorkflowOptions.SectionName));

        context.Services.AddHttpClient("Elsa", (sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ElsaWorkflowOptions>>().Value;
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });
    }
}
