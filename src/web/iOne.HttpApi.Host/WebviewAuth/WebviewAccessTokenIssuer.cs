using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using iOne.WebviewAuth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Security.Claims;

namespace iOne.WebviewAuth;

[ExposeServices(typeof(IWebviewAccessTokenIssuer))]
public class WebviewAccessTokenIssuer : IWebviewAccessTokenIssuer, ITransientDependency
{
    private const int AccessTokenLifetimeSeconds = 1800;

    private readonly IdentityUserManager _userManager;
    private readonly IUserClaimsPrincipalFactory<Volo.Abp.Identity.IdentityUser> _userClaimsPrincipalFactory;
    private readonly IAbpClaimsPrincipalFactory _abpClaimsPrincipalFactory;
    private readonly IOptionsMonitor<OpenIddictServerOptions> _serverOptions;
    private readonly IConfiguration _configuration;
    private readonly ICurrentTenant _currentTenant;

    public WebviewAccessTokenIssuer(
        IdentityUserManager userManager,
        IUserClaimsPrincipalFactory<Volo.Abp.Identity.IdentityUser> userClaimsPrincipalFactory,
        IAbpClaimsPrincipalFactory abpClaimsPrincipalFactory,
        IOptionsMonitor<OpenIddictServerOptions> serverOptions,
        IConfiguration configuration,
        ICurrentTenant currentTenant)
    {
        _userManager = userManager;
        _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
        _abpClaimsPrincipalFactory = abpClaimsPrincipalFactory;
        _serverOptions = serverOptions;
        _configuration = configuration;
        _currentTenant = currentTenant;
    }

    public async Task<WebviewAuthResultDto> IssueAsync(Guid identityUserId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.GetByIdAsync(identityUserId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found for token issuance.");
        }

        ClaimsPrincipal principal;
        using (_currentTenant.Change(user.TenantId))
        {
            principal = await _userClaimsPrincipalFactory.CreateAsync(user);
            principal = await _abpClaimsPrincipalFactory.CreateDynamicAsync(principal);

            // Anonymous webview requests have no tenant on ICurrentTenant; Identity role resolution
            // and tenant claim must match the user's tenant so permission checks see role/user grants.
            var identity = principal.Identities.First();
            if (identity.FindFirst(AbpClaimTypes.UserId) == null)
            {
                identity.AddClaim(new System.Security.Claims.Claim(AbpClaimTypes.UserId, user.Id.ToString()));
            }

            if (user.TenantId.HasValue && identity.FindFirst(AbpClaimTypes.TenantId) == null)
            {
                identity.AddClaim(new System.Security.Claims.Claim(AbpClaimTypes.TenantId, user.TenantId.Value.ToString()));
            }

            var roleNames = await _userManager.GetRolesAsync(user);
            foreach (var roleName in roleNames)
            {
                if (!identity.HasClaim(AbpClaimTypes.Role, roleName))
                {
                    identity.AddClaim(new System.Security.Claims.Claim(AbpClaimTypes.Role, roleName));
                }
            }
        }

        var clientId = _configuration["OpenIddict:Applications:iOne_App:ClientId"];
        if (string.IsNullOrWhiteSpace(clientId))
        {
            clientId = "iOne_App";
        }

        principal.SetScopes(
            OpenIddictConstants.Scopes.OpenId,
            OpenIddictConstants.Scopes.Profile,
            OpenIddictConstants.Scopes.Email,
            OpenIddictConstants.Scopes.Phone,
            OpenIddictConstants.Scopes.Roles,
            OpenIddictConstants.Scopes.Address,
            "iOne");

        principal.SetResources("iOne");
        principal.SetAudiences("iOne");
        principal.SetAccessTokenLifetime(TimeSpan.FromSeconds(AccessTokenLifetimeSeconds));

        var options = _serverOptions.CurrentValue;
        var signingCredentials = options.SigningCredentials.FirstOrDefault()
            ?? throw new InvalidOperationException("OpenIddict has no signing credentials configured.");
        var encryptingCredentials = options.DisableAccessTokenEncryption
            ? null
            : options.EncryptionCredentials.FirstOrDefault();

        // In Development, iOneHttpApiHostModule does not call SetIssuer on OpenIddictServerBuilder
        // (only non-dev does). Tokens must still use the same iss as validation — match AuthServer:Authority.
        var issuer = options.Issuer?.AbsoluteUri?.TrimEnd('/')
            ?? _configuration["AuthServer:Authority"]?.Trim().TrimEnd('/')
            ?? _configuration["App:SelfUrl"]?.Trim().TrimEnd('/');
        if (string.IsNullOrEmpty(issuer))
        {
            throw new InvalidOperationException(
                "OpenIddict issuer is not configured. Set AuthServer:Authority (or App:SelfUrl), or configure OpenIddict server issuer.");
        }

        var handler = new JsonWebTokenHandler
        {
            SetDefaultTimesOnTokenCreation = false
        };

        var subjectIdentity = principal.Identities.FirstOrDefault()
            ?? throw new InvalidOperationException("Claims principal has no identity.");
        var subject = new ClaimsIdentity(subjectIdentity.Claims, subjectIdentity.AuthenticationType, subjectIdentity.NameClaimType, subjectIdentity.RoleClaimType);
        subject.AddClaim(new System.Security.Claims.Claim(OpenIddictConstants.Claims.ClientId, clientId));

        var now = DateTime.UtcNow;
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = issuer,
            Audience = "iOne",
            IssuedAt = now,
            Expires = now.AddSeconds(AccessTokenLifetimeSeconds),
            SigningCredentials = signingCredentials,
            EncryptingCredentials = encryptingCredentials,
            Subject = subject,
            // OpenIddict 3+ / RFC 9068: access tokens must declare typ "at+jwt". JsonWebTokenHandler defaults to "JWT",
            // which makes OpenIddict.Validation reject the token (ID2089: not of the expected type).
            AdditionalHeaderClaims = new Dictionary<string, object>
            {
                { JwtHeaderParameterNames.Typ, "at+jwt" }
            }
        };

        var token = handler.CreateToken(descriptor);
        if (string.IsNullOrEmpty(token))
        {
            throw new InvalidOperationException("Failed to create access token.");
        }

        return new WebviewAuthResultDto
        {
            AccessToken = token,
            TokenType = "Bearer",
            ExpiresIn = AccessTokenLifetimeSeconds
        };
    }
}
