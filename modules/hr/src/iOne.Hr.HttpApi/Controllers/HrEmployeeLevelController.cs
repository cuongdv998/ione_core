using System;
using System.Threading.Tasks;
using iOne.Hr;
using iOne.Hr.HrEmployeeLevels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace iOne.Hr.Controllers;

[RemoteService(Name = HrRemoteServiceConsts.RemoteServiceName)]
[Area(HrRemoteServiceConsts.ModuleName)]
[Route("api/hr/employee-levels")]
[Authorize]
public class HrEmployeeLevelController : AbpControllerBase
{
    protected IHrEmployeeLevelAppService AppService { get; }

    public HrEmployeeLevelController(IHrEmployeeLevelAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    public virtual Task<PagedResultDto<HrEmployeeLevelDto>> GetListAsync(GetHrEmployeeLevelsInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    public virtual Task<HrEmployeeLevelDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    public virtual Task<HrEmployeeLevelDto> CreateAsync(CreateHrEmployeeLevelDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    public virtual Task<HrEmployeeLevelDto> UpdateAsync(Guid id, UpdateHrEmployeeLevelDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }
}

