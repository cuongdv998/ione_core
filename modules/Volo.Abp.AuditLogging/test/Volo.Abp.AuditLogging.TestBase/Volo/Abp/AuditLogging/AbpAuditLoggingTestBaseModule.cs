//using Microsoft.Extensions.DependencyInjection;
//using Volo.Abp.Modularity;

//namespace Volo.Abp.AuditLogging;

//[DependsOn(
//    typeof(AbpAuditLoggingDomainModule))]
//public class AbpAuditLoggingTestBaseModule : AbpModule
//{
//    public override void OnApplicationInitialization(ApplicationInitializationContext context)
//    {
//        SeedTestData(context);
//    }

//    private static void SeedTestData(ApplicationInitializationContext context)
//    {
//        using (var scope = context.ServiceProvider.CreateScope())
//        {
//            scope.ServiceProvider
//                .GetRequiredService<AuditingTestDataBuilder>()
//                .Build();
//        }
//    }
//}
