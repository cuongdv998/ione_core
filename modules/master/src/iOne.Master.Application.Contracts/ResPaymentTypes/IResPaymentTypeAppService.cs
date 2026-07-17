using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResPaymentTypes;

public interface IResPaymentTypeAppService : ICrudAppService<
    ResPaymentTypeDto,
    Guid,
    GetResPaymentTypesInput,
    CreateResPaymentTypeDto,
    UpdateResPaymentTypeDto>
{
    /// <summary>
    /// Lấy danh sách loại thanh toán cho dropdown, sắp xếp theo name A-Z.
    /// </summary>
    Task<List<ResPaymentTypeSelectDto>> GetSelectListAsync();
}
