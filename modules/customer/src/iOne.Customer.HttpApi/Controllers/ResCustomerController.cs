using System;
using System.Threading.Tasks;
using iOne.Customer;
using iOne.Customer.Permissions;
using iOne.Customer.ResCustomers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace iOne.Customer.Controllers;

[RemoteService(Name = CustomerRemoteServiceConsts.RemoteServiceName)]
[Area(CustomerRemoteServiceConsts.ModuleName)]
[Route("api/customer/res-customers")]
[Authorize]
public class ResCustomerController : AbpControllerBase
{
    protected IResCustomerAppService AppService { get; }

    public ResCustomerController(IResCustomerAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(ResCustomerPermissions.View)]
    public virtual Task<PagedResultDto<ResCustomerDto>> GetListAsync(GetResCustomersInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(ResCustomerPermissions.View)]
    public virtual Task<ResCustomerDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(ResCustomerPermissions.Create)]
    public virtual Task<ResCustomerDto> CreateAsync(CreateResCustomerDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(ResCustomerPermissions.Edit)]
    public virtual Task<ResCustomerDto> UpdateAsync(Guid id, UpdateResCustomerDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(ResCustomerPermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }
}

