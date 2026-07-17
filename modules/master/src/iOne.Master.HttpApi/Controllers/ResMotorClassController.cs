using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Master.Permissions;
using iOne.Master.ResMotorClasses;
using Volo.Abp;

namespace iOne.Master.Controllers;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Area(MasterRemoteServiceConsts.ModuleName)]
[Route("api/master/motor-classes")]
//[Authorize]
public class ResMotorClassController : AbpControllerBase
{
    protected IResMotorClassAppService AppService { get; }

    public ResMotorClassController(IResMotorClassAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(ResMotorClassPermissions.View)]
    public virtual Task<PagedResultDto<ResMotorClassDto>> GetListAsync(GetResMotorClassesInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(ResMotorClassPermissions.View)]
    public virtual Task<ResMotorClassDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(ResMotorClassPermissions.Create)]
    public virtual Task<ResMotorClassDto> CreateAsync(CreateResMotorClassDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(ResMotorClassPermissions.Edit)]
    public virtual Task<ResMotorClassDto> UpdateAsync(Guid id, UpdateResMotorClassDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(ResMotorClassPermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }
}

