using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Claim.Claims;
using Volo.Abp;

namespace iOne.Claim.Controllers;

[RemoteService(Name = ClaimRemoteServiceConsts.RemoteServiceName)]
[Area(ClaimRemoteServiceConsts.ModuleName)]
[Route("api/claim/claim-stage-progress")]
public class ClaimStageProgressController : AbpControllerBase
{
    protected IClaimStageProgressAppService AppService { get; }

    public ClaimStageProgressController(IClaimStageProgressAppService appService)
    {
        AppService = appService;
    }

    [HttpGet("list")]
    public virtual Task<PagedResultDto<ClaimStageProgressDto>> GetListAsync([FromQuery] GetClaimStageProgressInput input)
    {
        return AppService.GetListAsync(input);
    }
}

