using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResProvinces;

public interface IResProvinceAppService : ICrudAppService<
    ResProvinceDto,
    Guid,
    GetResProvincesInput,
    CreateResProvinceDto,
    UpdateResProvinceDto>
{
    /// <summary>
    /// Lấy danh sách tỉnh/thành cho dropdown. Chỉ cần đăng nhập.
    /// </summary>
    Task<List<ResProvinceSelectDto>> GetSelectListAsync();
}

