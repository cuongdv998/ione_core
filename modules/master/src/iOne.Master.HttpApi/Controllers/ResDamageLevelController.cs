using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Master;
using iOne.Master.Permissions;
using iOne.Master.ResDamageLevels;
using Volo.Abp;

namespace iOne.Master.Controllers;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Area(MasterRemoteServiceConsts.ModuleName)]
[Route("api/master/damage-levels")]
//[Authorize]
public class ResDamageLevelController : AbpControllerBase
{
    protected IResDamageLevelAppService AppService { get; }

    public ResDamageLevelController(IResDamageLevelAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(ResDamageLevelPermissions.View)]
    public virtual Task<PagedResultDto<ResDamageLevelDto>> GetListAsync(GetResDamageLevelsInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(ResDamageLevelPermissions.View)]
    public virtual Task<ResDamageLevelDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(ResDamageLevelPermissions.Create)]
    public virtual Task<ResDamageLevelDto> CreateAsync(CreateResDamageLevelDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(ResDamageLevelPermissions.Edit)]
    public virtual Task<ResDamageLevelDto> UpdateAsync(Guid id, UpdateResDamageLevelDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(ResDamageLevelPermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }
}

