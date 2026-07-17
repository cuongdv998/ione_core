using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Product.Permissions;
using iOne.Product.ProRules;

namespace iOne.Product.Controllers;

[RemoteService(Name = ProductRemoteServiceConsts.RemoteServiceName)]
[Area(ProductRemoteServiceConsts.ModuleName)]
[Route("api/product/pro-rules")]
//[Authorize]
public class ProRuleController : AbpControllerBase
{
    protected IProRuleAppService AppService { get; }

    public ProRuleController(IProRuleAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(ProProductPermissions.View)]
    public virtual Task<PagedResultDto<ProRuleDto>> GetListAsync(GetProRulesInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(ProProductPermissions.View)]
    public virtual Task<ProRuleDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(ProProductPermissions.Create)]
    public virtual Task<ProRuleDto> CreateAsync(CreateProRuleDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(ProProductPermissions.Edit)]
    public virtual Task<ProRuleDto> UpdateAsync(Guid id, UpdateProRuleDto input)
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
