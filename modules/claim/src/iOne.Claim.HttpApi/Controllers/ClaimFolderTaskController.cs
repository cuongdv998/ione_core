using System.Threading.Tasks;
using iOne.Claim.Claims;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace iOne.Claim.Controllers;

[RemoteService(Name = ClaimRemoteServiceConsts.RemoteServiceName)]
[Area(ClaimRemoteServiceConsts.ModuleName)]
[Route("api/claim-folder-tasks")]
public class ClaimFolderTaskController : AbpControllerBase
{
    protected IClaimFolderTaskAppService AppService { get; }

    public ClaimFolderTaskController(IClaimFolderTaskAppService appService)
    {
        AppService = appService;
    }

    [HttpGet("list")]
    public virtual Task<PagedResultDto<ClaimTaskDto>> GetListAsync([FromQuery] GetClaimTasksInput input)
    {
        return AppService.GetListAsync(input);
    }
}
