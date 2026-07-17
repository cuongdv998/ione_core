using System;
using System.Threading.Tasks;
using iOne.Customer;
using iOne.Customer.ResIndustries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace iOne.Customer.Controllers;

[RemoteService(Name = CustomerRemoteServiceConsts.RemoteServiceName)]
[Area(CustomerRemoteServiceConsts.ModuleName)]
[Route("api/customer/industries")]
[Authorize]
public class ResIndustryController : AbpControllerBase
{
    protected IResIndustryAppService AppService { get; }

    public ResIndustryController(IResIndustryAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    public virtual Task<PagedResultDto<ResIndustryDto>> GetListAsync(GetResIndustriesInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    public virtual Task<ResIndustryDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    public virtual Task<ResIndustryDto> CreateAsync(CreateResIndustryDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    public virtual Task<ResIndustryDto> UpdateAsync(Guid id, UpdateResIndustryDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }
}


