using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Claim.Claims;
using Volo.Abp;

namespace iOne.Claim.Controllers;

[RemoteService(Name = ClaimRemoteServiceConsts.RemoteServiceName)]
[Area(ClaimRemoteServiceConsts.ModuleName)]
[Route("api/claim/claim-sent-messages")]
public class ClaimSentMessageController : AbpControllerBase
{
    protected IClaimSentMessageAppService AppService { get; }

    public ClaimSentMessageController(IClaimSentMessageAppService appService)
    {
        AppService = appService;
    }

    [HttpGet("list")]
    public virtual Task<PagedResultDto<ClaimSentMessageDto>> GetListAsync([FromQuery] GetClaimSentMessagesInput input)
    {
        return AppService.GetListAsync(input);
    }
}

