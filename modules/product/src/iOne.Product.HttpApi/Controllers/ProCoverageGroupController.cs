using System;
using System.Threading.Tasks;
using iOne.Product;
using iOne.Product.ProCoverageGroups;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace iOne.Product.Controllers;

[RemoteService(Name = ProductRemoteServiceConsts.RemoteServiceName)]
[Area(ProductRemoteServiceConsts.ModuleName)]
[Route("api/product/coverage-groups")]
[Authorize]
public class ProCoverageGroupController : AbpControllerBase
{
    protected IProCoverageGroupAppService AppService { get; }

    public ProCoverageGroupController(IProCoverageGroupAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    public virtual Task<PagedResultDto<ProCoverageGroupDto>> GetListAsync(GetProCoverageGroupsInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    public virtual Task<ProCoverageGroupDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    public virtual Task<ProCoverageGroupDto> CreateAsync(CreateProCoverageGroupDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    public virtual Task<ProCoverageGroupDto> UpdateAsync(Guid id, UpdateProCoverageGroupDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }
}

