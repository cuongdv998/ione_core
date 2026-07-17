using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Product.Permissions;
using iOne.Product.ProCoverageLevelTypes;

namespace iOne.Product.Controllers;

[RemoteService(Name = ProductRemoteServiceConsts.RemoteServiceName)]
[Area(ProductRemoteServiceConsts.ModuleName)]
[Route("api/product/coverage-level-types")]
//[Authorize]
public class ProCoverageLevelTypeController : AbpControllerBase
{
    protected IProCoverageLevelTypeAppService AppService { get; }

    public ProCoverageLevelTypeController(IProCoverageLevelTypeAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(ProCoveragePermissions.View)]
    public virtual Task<PagedResultDto<ProCoverageLevelTypeDto>> GetListAsync(GetProCoverageLevelTypesInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(ProCoveragePermissions.View)]
    public virtual Task<ProCoverageLevelTypeDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(ProCoveragePermissions.Create)]
    public virtual Task<ProCoverageLevelTypeDto> CreateAsync(CreateProCoverageLevelTypeDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(ProCoveragePermissions.Edit)]
    public virtual Task<ProCoverageLevelTypeDto> UpdateAsync(Guid id, UpdateProCoverageLevelTypeDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(ProCoveragePermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }
}
