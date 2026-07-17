using System;
using System.Threading.Tasks;
using iOne.Hr;
using iOne.Hr.HrDepartmentTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace iOne.Hr.Controllers;

[RemoteService(Name = HrRemoteServiceConsts.RemoteServiceName)]
[Area(HrRemoteServiceConsts.ModuleName)]
[Route("api/hr/department-types")]
[Authorize]
public class HrDepartmentTypeController : AbpControllerBase
{
    protected IHrDepartmentTypeAppService AppService { get; }

    public HrDepartmentTypeController(IHrDepartmentTypeAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    public virtual Task<PagedResultDto<HrDepartmentTypeDto>> GetListAsync(GetHrDepartmentTypesInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    public virtual Task<HrDepartmentTypeDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    public virtual Task<HrDepartmentTypeDto> CreateAsync(CreateHrDepartmentTypeDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    public virtual Task<HrDepartmentTypeDto> UpdateAsync(Guid id, UpdateHrDepartmentTypeDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }
}

