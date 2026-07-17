using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Claim.Claims;
using Volo.Abp;

namespace iOne.Claim.Controllers;

[RemoteService(Name = ClaimRemoteServiceConsts.RemoteServiceName)]
[Area(ClaimRemoteServiceConsts.ModuleName)]
[Route("api/claim/claim-folders")]
public class ClaimFolderController : AbpControllerBase
{
    protected IClaimFolderAppService AppService { get; }

    public ClaimFolderController(IClaimFolderAppService appService)
    {
        AppService = appService;
    }

    [HttpGet("list")]
    public virtual Task<PagedResultDto<ClaimFolderListDto>> GetListAsync([FromQuery] GetClaimFoldersInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id:guid}")]
    public virtual Task<ClaimFolderDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    public virtual Task<ClaimFolderDto> CreateAsync([FromBody] CreateClaimFolderDto input)
    {
        return AppService.CreateAsync(input);
    }
}

