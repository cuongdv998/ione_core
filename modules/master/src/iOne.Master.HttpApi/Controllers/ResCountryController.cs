using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Master.Permissions;
using iOne.Master.ResCountries;

namespace iOne.Master.Controllers;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Area(MasterRemoteServiceConsts.ModuleName)]
[Route("api/master/countries")]
[Authorize]
public class ResCountryController : AbpControllerBase
{
    protected IResCountryAppService AppService { get; }

    public ResCountryController(IResCountryAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(ResCountryPermissions.View)]
    public virtual Task<PagedResultDto<ResCountryDto>> GetListAsync(GetResCountriesInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(ResCountryPermissions.View)]
    public virtual Task<ResCountryDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(ResCountryPermissions.Create)]
    public virtual Task<ResCountryDto> CreateAsync(CreateResCountryDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(ResCountryPermissions.Edit)]
    public virtual Task<ResCountryDto> UpdateAsync(Guid id, UpdateResCountryDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(ResCountryPermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }
}

