using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Master.Permissions;
using iOne.Master.ResBanks;

namespace iOne.Master.Controllers;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Area(MasterRemoteServiceConsts.ModuleName)]
[Route("api/master/banks")]
[Authorize]
public class ResBankController : AbpControllerBase
{
    protected IResBankAppService AppService { get; }

    public ResBankController(IResBankAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(ResBankPermissions.View)]
    public virtual Task<PagedResultDto<ResBankDto>> GetListAsync(GetResBanksInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(ResBankPermissions.View)]
    public virtual Task<ResBankDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(ResBankPermissions.Create)]
    public virtual Task<ResBankDto> CreateAsync(CreateResBankDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(ResBankPermissions.Edit)]
    public virtual Task<ResBankDto> UpdateAsync(Guid id, UpdateResBankDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(ResBankPermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }
}

