using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Customer.ResCustomers;

public interface IResCustomerAppService : ICrudAppService<
    ResCustomerDto,
    Guid,
    GetResCustomersInput,
    CreateResCustomerDto,
    UpdateResCustomerDto>
{
    Task<List<ResCustomerDto>> GetListByIdsAsync(List<Guid> ids);
}

