using iOne.ApiKeys;
using iOne.Authentication;
using iOne.AppMobile;
using iOne.Claim;
using iOne.Customer;
using iOne.EntityFrameworkCore;
using iOne.File;
using iOne.Finance;
using iOne.HealthChecks;
using iOne.Hr;
using iOne.Master;
using iOne.MultiTenancy;
using iOne.Partner;
using iOne.Policy;
using iOne.Product;
using iOne.Workflow;
using iOne.Report;
using iOne.Survey;
using iOne.Payment;
using iOne.Hubs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authorization;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Volo.Abp;
using Volo.Abp.Account;
using Volo.Abp.Account.Web;
using Volo.Abp.AspNetCore.MultiTenancy;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.AntiForgery;
using Volo.Abp.AspNetCore.Mvc.Libs;
using Volo.Abp.AspNetCore.Mvc.UI.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.AspNetCore.SignalR;
using Volo.Abp.Autofac;
using Volo.Abp.Identity;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict;
using Volo.Abp.Security.Claims;
using Volo.Abp.Studio;
using Volo.Abp.Studio.Client.AspNetCore;
using Volo.Abp.Swashbuckle;
using Volo.Abp.UI.Navigation.Urls;
using Volo.Abp.VirtualFileSystem;

namespace iOne;

[DependsOn(
    typeof(iOneHttpApiModule),
    typeof(AbpStudioClientAspNetCoreModule),
    typeof(AbpAspNetCoreMvcUiLeptonXLiteThemeModule),
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreMultiTenancyModule),
    typeof(AbpAspNetCoreSignalRModule),
    typeof(iOneApplicationModule),
    typeof(iOneEntityFrameworkCoreModule),
    typeof(AbpAccountWebOpenIddictModule),
    typeof(AbpSwashbuckleModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(iOneHrHttpApiModule),
    typeof(iOneHrApplicationModule),
    typeof(iOnePartnerHttpApiModule),
    typeof(iOnePartnerApplicationModule),
    typeof(iOneCustomerHttpApiModule),
    typeof(iOneCustomerApplicationModule),
    typeof(iOneProductHttpApiModule),
    typeof(iOneProductApplicationModule),
    typeof(iOnePolicyHttpApiModule),
    typeof(iOnePolicyApplicationModule),
    typeof(iOneClaimHttpApiModule),
    typeof(iOneClaimApplicationModule),
    typeof(iOneMasterHttpApiModule),
    typeof(iOneMasterApplicationModule),
    typeof(iOneFinanceHttpApiModule),
    typeof(iOneFinanceApplicationModule),
    typeof(iOneSurveyHttpApiModule),
    typeof(iOneSurveyApplicationModule),
    typeof(iOneFileHttpApiModule),
    typeof(iOneFileApplicationModule),
    typeof(iOneReportHttpApiModule),
    typeof(iOneReportApplicationModule),
    typeof(iOneWorkflowHttpApiModule),
    typeof(iOneWorkflowApplicationModule),
    typeof(iOneAppMobileApplicationModule),
    typeof(iOneAppMobileHttpApiModule),
    typeof(iOnePaymentHttpApiModule),
    typeof(iOnePaymentApplicationModule)
    )]
public class iOneHttpApiHostModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        PreConfigure<OpenIddictBuilder>(builder =>
        {
            builder.AddValidation(options =>
            {
                options.AddAudiences("iOne");
                options.UseLocalServer();
                options.UseAspNetCore();
            });
        });

        PreConfigure<OpenIddictServerBuilder>(serverBuilder =>
        {
            serverBuilder.SetAccessTokenLifetime(TimeSpan.FromHours(72));
        });

        if (!hostingEnvironment.IsDevelopment())
        {
            PreConfigure<AbpOpenIddictAspNetCoreOptions>(options =>
            {
                options.AddDevelopmentEncryptionAndSigningCertificate = false;
            });

            PreConfigure<OpenIddictServerBuilder>(serverBuilder =>
            {
                serverBuilder.AddProductionEncryptionAndSigningCertificate("openiddict.pfx", configuration["AuthServer:CertificatePassPhrase"]!);
                serverBuilder.SetIssuer(new Uri(configuration["AuthServer:Authority"]!));
            });
        }
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        var hostingEnvironment = context.Services.GetHostingEnvironment();

        context.Services.AddHttpContextAccessor();
        context.Services.AddHttpClient("AmioTokenVerificationClient", client =>
        {
            var baseUrl = configuration["WebviewAuth:PartnerVerification:BaseUrl"];
            if (!string.IsNullOrWhiteSpace(baseUrl))
            {
                client.BaseAddress = new Uri(baseUrl.TrimEnd('/'));
            }

            var timeoutSeconds = configuration.GetValue<int?>("WebviewAuth:PartnerVerification:TimeoutSeconds") ?? 30;
            client.Timeout = TimeSpan.FromSeconds(Math.Max(1, timeoutSeconds));
        });

        if (!configuration.GetValue<bool>("App:DisablePII"))
        {
            Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
            Microsoft.IdentityModel.Logging.IdentityModelEventSource.LogCompleteSecurityArtifact = true;
        }

        if (!configuration.GetValue<bool>("AuthServer:RequireHttpsMetadata"))
        {
            Configure<OpenIddictServerAspNetCoreOptions>(options =>
            {
                options.DisableTransportSecurityRequirement = true;
            });

            Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedProto;
            });
        }

        Configure<AbpAntiForgeryOptions>(options =>
        {
            options.AutoValidate = false;
        });

        /*fix for behind load balancer*/
        Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor |
                ForwardedHeaders.XForwardedProto;

            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

        Configure<AbpMvcLibsOptions>(options =>
        {
            options.CheckLibs = false;
        });
        ConfigureAuthentication(context);
        ConfigureUrls(configuration);
        ConfigureBundles();
        ConfigureConventionalControllers();
        ConfigureHealthChecks(context);
        ConfigureSwagger(context, configuration);
        ConfigureVirtualFileSystem(context);
        ConfigureCors(context, configuration);
    }

    private void ConfigureAuthentication(ServiceConfigurationContext context)
    {
        context.Services.ForwardIdentityAuthenticationForBearer(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);

        context.Services.AddAuthentication()
            .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
                ApiKeyAuthenticationOptions.DefaultScheme,
                options => { options.HeaderName = ApiKeyConstants.HeaderName; });

        // When X-Api-Key is present, forward to ApiKey scheme so the handler runs and authenticates the request.
        // Otherwise Bearer is used when Authorization header is present (via ForwardIdentityAuthenticationForBearer).
        context.Services.ConfigureApplicationCookie(options =>
        {
            options.ForwardDefaultSelector = ctx =>
            {
                if (!string.IsNullOrWhiteSpace(ctx.Request.Headers[ApiKeyConstants.HeaderName]) ||
                    !string.IsNullOrWhiteSpace(ctx.Request.Headers[ApiKeyConstants.HeaderNameAlternate]))
                    return ApiKeyAuthenticationOptions.DefaultScheme;

                var authorization = ctx.Request.Headers.Authorization;
                if (!string.IsNullOrWhiteSpace(authorization) && authorization.ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    return OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;

                return null;
            };

            options.Events.OnRedirectToLogin = ctx =>
            {
                if (ctx.Request.Path.StartsWithSegments("/api"))
                {
                    ctx.Response.StatusCode = 401;
                    return System.Threading.Tasks.Task.CompletedTask;
                }

                ctx.Response.Redirect(ctx.RedirectUri);
                return System.Threading.Tasks.Task.CompletedTask;
            };

            options.Events.OnRedirectToAccessDenied = ctx =>
            {
                if (ctx.Request.Path.StartsWithSegments("/api"))
                {
                    ctx.Response.StatusCode = 403;
                    return System.Threading.Tasks.Task.CompletedTask;
                }

                ctx.Response.Redirect(ctx.RedirectUri);
                return System.Threading.Tasks.Task.CompletedTask;
            };
        });

        context.Services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .AddAuthenticationSchemes(
                    OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme,
                    ApiKeyAuthenticationOptions.DefaultScheme)
                .RequireAuthenticatedUser()
                .Build();
        });

        context.Services.Configure<AbpClaimsPrincipalFactoryOptions>(options =>
        {
            options.IsDynamicClaimsEnabled = true;
        });
    }

    private void ConfigureUrls(IConfiguration configuration)
    {
        Configure<AppUrlOptions>(options =>
        {
            options.Applications["MVC"].RootUrl = configuration["App:SelfUrl"];
            options.Applications["Angular"].RootUrl = configuration["App:AngularUrl"];
            options.Applications["Angular"].Urls[AccountUrlNames.PasswordReset] = "account/reset-password";
            options.RedirectAllowedUrls.AddRange(configuration["App:RedirectAllowedUrls"]?.Split(',') ?? Array.Empty<string>());
        });
    }

    private void ConfigureBundles()
    {
        Configure<AbpBundlingOptions>(options =>
        {
            options.StyleBundles.Configure(
                LeptonXLiteThemeBundles.Styles.Global,
                bundle =>
                {
                    bundle.AddFiles("/global-styles.css");
                }
            );

            options.ScriptBundles.Configure(
                LeptonXLiteThemeBundles.Scripts.Global,
                bundle =>
                {
                    bundle.AddFiles("/global-scripts.js");
                }
            );
        });
    }


    private void ConfigureVirtualFileSystem(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();

        if (hostingEnvironment.IsDevelopment())
        {
            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.ReplaceEmbeddedByPhysical<iOneDomainSharedModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}common{Path.DirectorySeparatorChar}domain{Path.DirectorySeparatorChar}iOne.Domain.Shared"));
                options.FileSets.ReplaceEmbeddedByPhysical<AbpAccountWebOpenIddictModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}..{0}..{0}modules{0}Volo.Abp.Account{0}src{0}Volo.Abp.Account.Web.OpenIddict", Path.DirectorySeparatorChar)));
                options.FileSets.ReplaceEmbeddedByPhysical<iOneDomainModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}common{Path.DirectorySeparatorChar}domain{Path.DirectorySeparatorChar}iOne.Domain"));
                options.FileSets.ReplaceEmbeddedByPhysical<iOneApplicationContractsModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}iOne.Application.Contracts"));
                options.FileSets.ReplaceEmbeddedByPhysical<iOneApplicationModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}iOne.Application"));
                options.FileSets.ReplaceEmbeddedByPhysical<iOnePartnerApplicationContractsModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}modules{Path.DirectorySeparatorChar}partner{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}iOne.Partner.Application.Contracts"));
                options.FileSets.ReplaceEmbeddedByPhysical<iOnePartnerApplicationModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}modules{Path.DirectorySeparatorChar}partner{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}iOne.Partner.Application"));
                options.FileSets.ReplaceEmbeddedByPhysical<iOneCustomerApplicationContractsModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}modules{Path.DirectorySeparatorChar}customer{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}iOne.Customer.Application.Contracts"));
                options.FileSets.ReplaceEmbeddedByPhysical<iOneCustomerApplicationModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}modules{Path.DirectorySeparatorChar}customer{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}iOne.Customer.Application"));
                options.FileSets.ReplaceEmbeddedByPhysical<iOneProductApplicationContractsModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}modules{Path.DirectorySeparatorChar}product{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}iOne.Product.Application.Contracts"));
                options.FileSets.ReplaceEmbeddedByPhysical<iOneProductApplicationModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}modules{Path.DirectorySeparatorChar}product{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}iOne.Product.Application"));
                options.FileSets.ReplaceEmbeddedByPhysical<iOnePolicyApplicationContractsModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}modules{Path.DirectorySeparatorChar}policy{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}iOne.Policy.Application.Contracts"));
                options.FileSets.ReplaceEmbeddedByPhysical<iOnePolicyApplicationModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}modules{Path.DirectorySeparatorChar}policy{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}iOne.Policy.Application"));
                options.FileSets.ReplaceEmbeddedByPhysical<iOneClaimApplicationContractsModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}modules{Path.DirectorySeparatorChar}claim{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}iOne.Claim.Application.Contracts"));
                options.FileSets.ReplaceEmbeddedByPhysical<iOneClaimApplicationModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}modules{Path.DirectorySeparatorChar}claim{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}iOne.Claim.Application"));
                options.FileSets.ReplaceEmbeddedByPhysical<iOneMasterApplicationContractsModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}modules{Path.DirectorySeparatorChar}master{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}iOne.Master.Application.Contracts"));
                options.FileSets.ReplaceEmbeddedByPhysical<iOneMasterApplicationModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}modules{Path.DirectorySeparatorChar}master{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}iOne.Master.Application"));
                options.FileSets.ReplaceEmbeddedByPhysical<iOneFinanceApplicationContractsModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}modules{Path.DirectorySeparatorChar}finance{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}iOne.Finance.Application.Contracts"));
                options.FileSets.ReplaceEmbeddedByPhysical<iOneFinanceApplicationModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}modules{Path.DirectorySeparatorChar}finance{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}iOne.Finance.Application"));
                options.FileSets.ReplaceEmbeddedByPhysical<iOneSurveyApplicationContractsModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}modules{Path.DirectorySeparatorChar}survey{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}iOne.Survey.Application.Contracts"));
                options.FileSets.ReplaceEmbeddedByPhysical<iOneSurveyApplicationModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}modules{Path.DirectorySeparatorChar}survey{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}iOne.Survey.Application"));
                options.FileSets.ReplaceEmbeddedByPhysical<iOneFileApplicationContractsModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}modules{Path.DirectorySeparatorChar}file{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}iOne.File.Application.Contracts"));
                options.FileSets.ReplaceEmbeddedByPhysical<iOneFileApplicationModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}modules{Path.DirectorySeparatorChar}file{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}iOne.File.Application"));
                options.FileSets.ReplaceEmbeddedByPhysical<iOneReportApplicationContractsModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}modules{Path.DirectorySeparatorChar}report{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}iOne.Report.Application.Contracts"));
                options.FileSets.ReplaceEmbeddedByPhysical<iOneReportApplicationModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}modules{Path.DirectorySeparatorChar}report{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}iOne.Report.Application"));
                options.FileSets.ReplaceEmbeddedByPhysical<iOnePaymentApplicationContractsModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}modules{Path.DirectorySeparatorChar}payment{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}iOne.Payment.Application.Contracts"));
                options.FileSets.ReplaceEmbeddedByPhysical<iOnePaymentApplicationModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}modules{Path.DirectorySeparatorChar}payment{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}iOne.Payment.Application"));
            });
        }
    }

    private void ConfigureConventionalControllers()
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(typeof(iOneApplicationModule).Assembly);

        });
    }

    private static void ConfigureSwagger(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddAbpSwaggerGenWithOidc(
            configuration["AuthServer:Authority"]!,
            ["iOne"],
            [AbpSwaggerOidcFlows.AuthorizationCode],
            null,
            options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "iOne API", Version = "v1" });
                options.DocInclusionPredicate((docName, description) => true);
                options.CustomSchemaIds(type => type.FullName);
                options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.ApiKey,
                    In = ParameterLocation.Header,
                    Name = ApiKeyConstants.HeaderName,
                    Description = "API key for machine-to-machine or external system access. Format: prefix_secret"
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" }
                        },
                        Array.Empty<string>()
                    }
                });
            });
    }

    private void ConfigureCors(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder
                    .WithOrigins(
                        configuration["App:CorsOrigins"]?
                            .Split(",", StringSplitOptions.RemoveEmptyEntries)
                            .Select(o => o.Trim().RemovePostFix("/"))
                            .ToArray() ?? Array.Empty<string>()
                    )
                    .WithAbpExposedHeaders()
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
    }

    private void ConfigureHealthChecks(ServiceConfigurationContext context)
    {
        context.Services.AddiOneHealthChecks();
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();

        app.UseForwardedHeaders();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseAbpRequestLocalization();

        if (!env.IsDevelopment())
        {
            app.UseErrorPage();
        }

        app.UseCors();
        app.UseRouting();
        app.MapAbpStaticAssets();
        app.UseAbpStudioLink();
        app.UseAbpSecurityHeaders();
        app.UseAuthentication();
        app.UseAbpOpenIddictValidation();

        if (MultiTenancyConsts.IsEnabled)
        {
            app.UseMultiTenancy();
        }

        app.UseUnitOfWork();
        app.UseDynamicClaims();
        app.UseAuthorization();

        app.UseSwagger();
        app.UseAbpSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "iOne API");

            var configuration = context.ServiceProvider.GetRequiredService<IConfiguration>();
            options.OAuthClientId(configuration["AuthServer:SwaggerClientId"]);
        });
        app.UseAuditing();
        app.UseAbpSerilogEnrichers();
        app.UseConfiguredEndpoints();
    }
}
