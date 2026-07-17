using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Master;
using iOne.Master.Permissions;
using iOne.Master.ResRisks;
using Volo.Abp;

namespace iOne.Master.Controllers;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Area(MasterRemoteServiceConsts.ModuleName)]
[Route("api/master/risks")]
//[Authorize]
public class ResRiskController : AbpControllerBase
{
    protected IResRiskAppService AppService { get; }

    public ResRiskController(IResRiskAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(ResRiskPermissions.View)]
    public virtual Task<PagedResultDto<ResRiskDto>> GetListAsync(GetResRisksInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(ResRiskPermissions.View)]
    public virtual Task<ResRiskDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(ResRiskPermissions.Create)]
    public virtual Task<ResRiskDto> CreateAsync(CreateResRiskDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(ResRiskPermissions.Edit)]
    public virtual Task<ResRiskDto> UpdateAsync(Guid id, UpdateResRiskDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(ResRiskPermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }
}

