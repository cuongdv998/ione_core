using System;
using System.Linq;
using System.Threading.Tasks;
using iOne.HrEmployees;
using iOne.ResPartners;
using iOne.ProLineOfBusinesses;
using iOne.ProProducts;
using iOne.ResOrganizationTypes;
using iOne.ResObjectTypes;
using iOne.PolicyTypes;
using iOne.AdminConfigs;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using iOne.Permissions;

namespace iOne.WebviewAuth;

public class WebviewAuthAppService : iOneAppService, IWebviewAuthAppService
{
    private readonly IPartnerWebviewTokenValidator _partnerWebviewTokenValidator;
    private readonly IWebviewAccessTokenIssuer _webviewAccessTokenIssuer;
    private readonly IRepository<ResPartner, Guid> _resPartnerRepository;
    private readonly IRepository<HrEmployee, Guid> _hrEmployeeRepository;
    private readonly IRepository<ProProduct, Guid> _proProductRepository;
    private readonly IRepository<ProLineOfBusiness, Guid> _proLineOfBusinessRepository;
    private readonly IRepository<ResOrganizationType, Guid> _resOrganizationTypeRepository;
    private readonly IRepository<ResObjectType, Guid> _resObjectTypeRepository;
    private readonly IRepository<PolicyType, Guid> _policyTypeRepository;
    private readonly IRepository<AdminConfig, Guid> _adminConfigRepository;
    private readonly IdentityUserManager _identityUserManager;

    public WebviewAuthAppService(
        IPartnerWebviewTokenValidator partnerWebviewTokenValidator,
        IWebviewAccessTokenIssuer webviewAccessTokenIssuer,
        IRepository<ResPartner, Guid> resPartnerRepository,
        IRepository<HrEmployee, Guid> hrEmployeeRepository,
        IRepository<ProProduct, Guid> proProductRepository,
        IRepository<ProLineOfBusiness, Guid> proLineOfBusinessRepository,
        IRepository<ResOrganizationType, Guid> resOrganizationTypeRepository,
        IRepository<ResObjectType, Guid> resObjectTypeRepository,
        IRepository<PolicyType, Guid> policyTypeRepository,
        IRepository<AdminConfig, Guid> adminConfigRepository,
        IdentityUserManager identityUserManager)
    {
        _partnerWebviewTokenValidator = partnerWebviewTokenValidator;
        _webviewAccessTokenIssuer = webviewAccessTokenIssuer;
        _resPartnerRepository = resPartnerRepository;
        _hrEmployeeRepository = hrEmployeeRepository;
        _proProductRepository = proProductRepository;
        _proLineOfBusinessRepository = proLineOfBusinessRepository;
        _resOrganizationTypeRepository = resOrganizationTypeRepository;
        _resObjectTypeRepository = resObjectTypeRepository;
        _policyTypeRepository = policyTypeRepository;
        _adminConfigRepository = adminConfigRepository;
        _identityUserManager = identityUserManager;
    }

    [AllowAnonymous]
    public virtual Task<object> SendOrderResultAsync(SendOrderResultInputDto input)
    {
        throw new NotImplementedException();
    }

    [AllowAnonymous]
    public virtual async Task<WebviewAuthResultDto> CreateAsync(WebviewAuthInputDto input)
    {
        var partnerCode = input.PartnerCode.Trim();
        var partnerToken = input.PartnerToken;

        var validationResult = await _partnerWebviewTokenValidator.ValidateAsync(partnerCode, partnerToken);
        if (!validationResult.Success)
        {
            throw new AbpAuthorizationException("Partner token validation failed.");
        }

        var partnerQuery = await _resPartnerRepository.GetQueryableAsync();
        var partner = await AsyncExecuter.FirstOrDefaultAsync(
            partnerQuery.Where(x => x.Code == partnerCode));

        if (partner == null)
        {
            throw new EntityNotFoundException(typeof(ResPartner), partnerCode);
        }

        var employeeQuery = await _hrEmployeeRepository.GetQueryableAsync();
        var employee = await AsyncExecuter.FirstOrDefaultAsync(
            employeeQuery
                .Where(x =>
                    x.PartnerId == partner.Id
                    && x.UserId != null
                    && x.Status == HrEmployeeStatus.Active)
                .OrderBy(x => x.CreationTime)
                .ThenBy(x => x.Id));

        if (employee == null)
        {
            throw new EntityNotFoundException(typeof(HrEmployee), partnerCode);
        }

        var userId = employee.UserId!.Value;
        var user = await _identityUserManager.FindByIdAsync(userId.ToString());
        if (user == null || !user.IsActive)
        {
            throw new EntityNotFoundException(typeof(IdentityUser), userId);
        }

        var result = await _webviewAccessTokenIssuer.IssueAsync(userId);
        result.Data = new WebviewAuthCustomerData
        {
            CustomerCode = validationResult.CustomerCode,
            CustomerName = validationResult.CustomerName,
            CustomerPhone = validationResult.CustomerPhone,
            CustomerEmail = validationResult.CustomerEmail,
            CustomerAddress = validationResult.CustomerAddress
        };

        return result;
    }

    [Authorize(iOnePermissions.WebviewAuth.Default)]
    public virtual async Task<PartnerProductConfigDto> GetPartnerProductConfigAsync(string partnerCode, string productCode)
    {
        var adminConfigQuery = await _adminConfigRepository.GetQueryableAsync();
        var adminConfigs = await AsyncExecuter.ToListAsync(
            adminConfigQuery.Where(x => x.Code == "WEBVIEW_CONFIG"));

        string GetConfigValue(string subCode)
        {
            var config = adminConfigs.FirstOrDefault(x => x.SubCode == subCode);
            return config?.Value;
        }
        
        var orgTypeCode = GetConfigValue("ORG_TYPE_CODE");
        var policyTypeCode = GetConfigValue("POLICY_TYPE_CODE");
        var objectTypeCode = GetConfigValue("OBJECT_TYPE_CODE");

        var partnerQuery = await _resPartnerRepository.GetQueryableAsync();
        var partner = await AsyncExecuter.FirstOrDefaultAsync(
            partnerQuery.Where(x => x.Code == partnerCode));

        if (partner == null)
        {
            throw new EntityNotFoundException(typeof(ResPartner), partnerCode);
        }

        var productQuery = await _proProductRepository.GetQueryableAsync();
        var product = await AsyncExecuter.FirstOrDefaultAsync(
            productQuery.Where(x => x.Code == productCode));

        if (product == null)
        {
            throw new EntityNotFoundException(typeof(ProProduct), productCode ?? string.Empty);
        }

        var orgTypeQuery = await _resOrganizationTypeRepository.GetQueryableAsync();
        var orgType = await AsyncExecuter.FirstOrDefaultAsync(
            orgTypeQuery.Where(x => x.Code == orgTypeCode));

        if (orgType == null)
        {
            throw new EntityNotFoundException(typeof(ResOrganizationType), orgTypeCode ?? string.Empty);
        }

        var objTypeQuery = await _resObjectTypeRepository.GetQueryableAsync();
        var objType = await AsyncExecuter.FirstOrDefaultAsync(
            objTypeQuery.Where(x => x.Code == objectTypeCode));

        if (objType == null)
        {
            throw new EntityNotFoundException(typeof(ResObjectType), objectTypeCode ?? string.Empty);
        }

        var policyTypeQuery = await _policyTypeRepository.GetQueryableAsync();
        var policyType = await AsyncExecuter.FirstOrDefaultAsync(
            policyTypeQuery.Where(x => x.Code == policyTypeCode));

        if (policyType == null)
        {
            throw new EntityNotFoundException(typeof(PolicyType), policyTypeCode ?? string.Empty);
        }

        var amioPartner = await AsyncExecuter.FirstOrDefaultAsync(
            partnerQuery.Where(x => x.Code.ToLower() == "amio"));

        Guid? sellerId = null;
        if (amioPartner != null)
        {
            var employeeQuery = await _hrEmployeeRepository.GetQueryableAsync();
            var amioEmployee = await AsyncExecuter.FirstOrDefaultAsync(
                employeeQuery
                    .Where(x =>
                        x.PartnerId == amioPartner.Id
                        && x.Status == HrEmployeeStatus.Active)
                    .OrderBy(x => x.CreationTime)
                    .ThenBy(x => x.Id));

            sellerId = amioEmployee?.Id;
        }

        return new PartnerProductConfigDto
        {
            InsurerId = partner.Id,
            ProductLobId = product.LobId,
            ChannelId = amioPartner?.ChannelId ?? Guid.Empty,
            OrganizationTypeId = orgType?.Id ?? Guid.Empty,
            ObjectTypeId = objType?.Id ?? Guid.Empty,
            PolicyTypeId = policyType?.Id ?? Guid.Empty,
            SellerId = sellerId
        };
    }

    [Authorize(iOnePermissions.WebviewAuth.Default)]
    public virtual async Task<PartnerProductConfigDto> GetPartnerProductConfigV2Async(
        string insurerPartnerCode,
        string productLobCode,
        string objectTypeCode = null,
        string policyTypeCode = null)
    {
        var partnerQuery = await _resPartnerRepository.GetQueryableAsync();
        var partner = await AsyncExecuter.FirstOrDefaultAsync(
            partnerQuery.Where(x => x.Code == insurerPartnerCode));

        if (partner == null)
        {
            throw new EntityNotFoundException(typeof(ResPartner), insurerPartnerCode);
        }

        var lobQuery = await _proLineOfBusinessRepository.GetQueryableAsync();
        var lob = await AsyncExecuter.FirstOrDefaultAsync(
            lobQuery.Where(x => x.Code == productLobCode));

        if (lob == null)
        {
            throw new EntityNotFoundException(typeof(ProLineOfBusiness), productLobCode);
        }

        var orgTypeQuery = await _resOrganizationTypeRepository.GetQueryableAsync();
        var orgType = await AsyncExecuter.FirstOrDefaultAsync(
            orgTypeQuery.Where(x => x.Code == "CN"));

        var objTypeQuery = await _resObjectTypeRepository.GetQueryableAsync();
        var objType = await AsyncExecuter.FirstOrDefaultAsync(
            objTypeQuery.Where(x => x.Code == (objectTypeCode ?? "XM")));

        var policyTypeQuery = await _policyTypeRepository.GetQueryableAsync();
        var policyType = await AsyncExecuter.FirstOrDefaultAsync(
            policyTypeQuery.Where(x => x.Code == (policyTypeCode ?? "BHG")));

        var amioPartner = await AsyncExecuter.FirstOrDefaultAsync(
            partnerQuery.Where(x => x.Code.ToLower() == "amio"));

        Guid? sellerId = null;
        if (amioPartner != null)
        {
            var employeeQuery = await _hrEmployeeRepository.GetQueryableAsync();
            var amioEmployee = await AsyncExecuter.FirstOrDefaultAsync(
                employeeQuery
                    .Where(x =>
                        x.PartnerId == amioPartner.Id
                        && x.Status == HrEmployeeStatus.Active)
                    .OrderBy(x => x.CreationTime)
                    .ThenBy(x => x.Id));

            sellerId = amioEmployee?.Id;
        }

        return new PartnerProductConfigDto
        {
            InsurerId = partner.Id,
            ProductLobId = lob.Id,
            ChannelId = amioPartner?.ChannelId ?? Guid.Empty,
            OrganizationTypeId = orgType?.Id ?? Guid.Empty,
            ObjectTypeId = objType?.Id ?? Guid.Empty,
            PolicyTypeId = policyType?.Id ?? Guid.Empty,
            SellerId = sellerId
        };
    }

}
