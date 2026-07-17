using System;
using System.Threading.Tasks;
using iOne.Hr;
using iOne.Hr.HrEmployeePositions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace iOne.Hr.Controllers;

[RemoteService(Name = HrRemoteServiceConsts.RemoteServiceName)]
[Area(HrRemoteServiceConsts.ModuleName)]
[Route("api/hr/employee-positions")]
[Authorize]
public class HrEmployeePositionController : AbpControllerBase
{
    protected IHrEmployeePositionAppService AppService { get; }

    public HrEmployeePositionController(IHrEmployeePositionAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(iOne.Hr.Permissions.HrEmployeePositionPermissions.View)]
    public virtual Task<PagedResultDto<HrEmployeePositionDto>> GetListAsync(GetHrEmployeePositionsInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(iOne.Hr.Permissions.HrEmployeePositionPermissions.View)]
    public virtual Task<HrEmployeePositionDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(iOne.Hr.Permissions.HrEmployeePositionPermissions.Create)]
    public virtual Task<HrEmployeePositionDto> CreateAsync(CreateHrEmployeePositionDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(iOne.Hr.Permissions.HrEmployeePositionPermissions.Edit)]
    public virtual Task<HrEmployeePositionDto> UpdateAsync(Guid id, UpdateHrEmployeePositionDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(iOne.Hr.Permissions.HrEmployeePositionPermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }
}

