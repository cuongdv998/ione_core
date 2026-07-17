using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Master.Permissions;
using iOne.Master.ResSequences;

namespace iOne.Master.Controllers;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Area(MasterRemoteServiceConsts.ModuleName)]
[Route("api/master/res-sequences")]
[Authorize]
public class ResSequenceController : AbpControllerBase
{
    protected IResSequenceAppService AppService { get; }

    public ResSequenceController(IResSequenceAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(ResSequencePermissions.View)]
    public virtual Task<PagedResultDto<ResSequenceDto>> GetListAsync(GetResSequencesInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(ResSequencePermissions.View)]
    public virtual Task<ResSequenceDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(ResSequencePermissions.Create)]
    public virtual Task<ResSequenceDto> CreateAsync(CreateResSequenceDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(ResSequencePermissions.Edit)]
    public virtual Task<ResSequenceDto> UpdateAsync(Guid id, UpdateResSequenceDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(ResSequencePermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }

    [HttpPost("get-next")]
    [Authorize(ResSequencePermissions.View)]
    public virtual Task<GetNextSequenceOutput> GetNextSequenceAsync(GetNextSequenceInput input)
    {
        return AppService.GetNextSequenceAsync(input);
    }
}

