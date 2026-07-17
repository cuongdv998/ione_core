using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResPaymentMethods;

public interface IResPaymentMethodAppService : ICrudAppService<
    ResPaymentMethodDto,
    Guid,
    GetResPaymentMethodsInput,
    CreateResPaymentMethodDto,
    UpdateResPaymentMethodDto>
{
    /// <summary>
    /// Lấy danh sách hình thức thanh toán cho dropdown, sắp xếp theo name A-Z.
    /// </summary>
    Task<List<ResPaymentMethodSelectDto>> GetSelectListAsync();
}
