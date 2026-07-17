using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResWards;

public interface IResWardAppService : ICrudAppService<
    ResWardDto,
    Guid,
    GetResWardsInput,
    CreateResWardDto,
    UpdateResWardDto>
{
    /// <summary>
    /// Lấy danh sách phường/xã cho dropdown, có thể lọc theo provinceId. Chỉ cần đăng nhập.
    /// </summary>
    Task<List<ResWardSelectDto>> GetSelectListAsync(Guid? provinceId = null);
}

