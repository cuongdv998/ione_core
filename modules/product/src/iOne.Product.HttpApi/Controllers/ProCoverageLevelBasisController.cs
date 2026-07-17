using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Product.Permissions;
using iOne.Product.ProCoverageLevelBasis;

namespace iOne.Product.Controllers;

[RemoteService(Name = ProductRemoteServiceConsts.RemoteServiceName)]
[Area(ProductRemoteServiceConsts.ModuleName)]
[Route("api/product/coverage-level-basis")]
//[Authorize]
public class ProCoverageLevelBasisController : AbpControllerBase
{
    protected IProCoverageLevelBasisAppService AppService { get; }

    public ProCoverageLevelBasisController(IProCoverageLevelBasisAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(ProProductPermissions.View)]
    public virtual Task<PagedResultDto<ProCoverageLevelBasisDto>> GetListAsync(GetProCoverageLevelBasisInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(ProProductPermissions.View)]
    public virtual Task<ProCoverageLevelBasisDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(ProProductPermissions.Create)]
    public virtual Task<ProCoverageLevelBasisDto> CreateAsync(CreateProCoverageLevelBasisDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(ProProductPermissions.Edit)]
    public virtual Task<ProCoverageLevelBasisDto> UpdateAsync(Guid id, UpdateProCoverageLevelBasisDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(ProProductPermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }
}
