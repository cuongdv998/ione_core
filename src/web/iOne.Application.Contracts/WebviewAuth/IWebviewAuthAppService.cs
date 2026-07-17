using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;

namespace iOne.WebviewAuth;

public interface IWebviewAuthAppService : IApplicationService
{
    [AllowAnonymous]
    Task<WebviewAuthResultDto> CreateAsync(WebviewAuthInputDto input);

    [AllowAnonymous]
    Task<object> SendOrderResultAsync(SendOrderResultInputDto input);
    
    Task<PartnerProductConfigDto> GetPartnerProductConfigAsync(string partnerCode, string productCode);

    Task<PartnerProductConfigDto> GetPartnerProductConfigV2Async(
        string insurerPartnerCode,
        string productLobCode,
        string objectTypeCode = null,
        string policyTypeCode = null);
}
