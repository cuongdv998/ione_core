using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Master;
using iOne.Master.Permissions;
using iOne.Master.BusinessFlows;

namespace iOne.Master.Controllers;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Area(MasterRemoteServiceConsts.ModuleName)]
[Route("api/master/business-flows")]
[Authorize]
public class BusinessFlowController : AbpControllerBase
{
    protected IBusinessFlowAppService AppService { get; }

    public BusinessFlowController(IBusinessFlowAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(BusinessFlowPermissions.View)]
    public virtual Task<PagedResultDto<BusinessFlowDto>> GetListAsync(GetBusinessFlowsInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(BusinessFlowPermissions.View)]
    public virtual Task<BusinessFlowDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(BusinessFlowPermissions.Create)]
    public virtual Task<BusinessFlowDto> CreateAsync(CreateBusinessFlowDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(BusinessFlowPermissions.Edit)]
    public virtual Task<BusinessFlowDto> UpdateAsync(Guid id, UpdateBusinessFlowDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(BusinessFlowPermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }
}
