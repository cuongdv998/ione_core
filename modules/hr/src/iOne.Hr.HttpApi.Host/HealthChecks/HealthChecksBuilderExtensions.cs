using System;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AspNetCore.Mvc;

namespace iOne.HealthChecks;

public static class HealthChecksBuilderExtensions
{
    public static void AddiOneHealthChecks(this IServiceCollection services, IConfiguration? configuration = null)
    {
        // Add your health checks here
        var healthChecksBuilder = services.AddHealthChecks();
        // Note: For module-specific health checks, you can add custom checks here
        // healthChecksBuilder.AddCheck<HrDatabaseCheck>("HR DbContext Check", tags: new string[] { "database" });

        services.ConfigureHealthCheckEndpoint("/health-status");

        if (configuration == null)
        {
            // Try to get configuration from service provider if available
            var serviceProvider = services.BuildServiceProvider();
            configuration = serviceProvider.GetService<IConfiguration>();
        }

        var healthCheckUrl = configuration?["App:HealthCheckUrl"];

        if (string.IsNullOrEmpty(healthCheckUrl))
        {
            healthCheckUrl = "/health-status";
        }

        var evaluationSeconds = configuration?.GetValue("App:HealthCheckEvaluationSeconds", 60) ?? 60;
        if (evaluationSeconds < 10)
        {
            evaluationSeconds = 10;
        }

        var healthChecksUiBuilder = services.AddHealthChecksUI(settings =>
        {
            settings.SetEvaluationTimeInSeconds(evaluationSeconds);
            settings.AddHealthCheckEndpoint("HR Health Status", configuration?["App:HealthUiCheckUrl"] ?? healthCheckUrl);
        });

        // Set your HealthCheck UI Storage here
        healthChecksUiBuilder.AddInMemoryStorage();

        services.MapHealthChecksUiEndpoints(options =>
        {
            options.UIPath = "/health-ui";
            options.ApiPath = "/health-api";
        });
    }

    private static IServiceCollection ConfigureHealthCheckEndpoint(this IServiceCollection services, string path)
    {
        services.Configure<AbpEndpointRouterOptions>(options =>
        {
            options.EndpointConfigureActions.Add(endpointContext =>
            {
                endpointContext.Endpoints.MapHealthChecks(
                    new PathString(path.StartsWith('/') ? path : $"/{path}"),
                    new HealthCheckOptions
                    {
                        Predicate = _ => true,
                        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                        AllowCachingResponses = false,
                    });
            });
        });

        return services;
    }

    private static IServiceCollection MapHealthChecksUiEndpoints(this IServiceCollection services, Action<global::HealthChecks.UI.Configuration.Options>? setupOption = null)
    {
        services.Configure<AbpEndpointRouterOptions>(routerOptions =>
        {
            routerOptions.EndpointConfigureActions.Add(endpointContext =>
            {
                endpointContext.Endpoints.MapHealthChecksUI(setupOption);
            });
        });

        return services;
    }
}

