using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Master.AdminConfigs;

public interface IAdminConfigAppService : ICrudAppService<
    AdminConfigDto,
    Guid,
    GetAdminConfigsInput,
    CreateAdminConfigDto,
    UpdateAdminConfigDto>
{
    /// <summary>
    /// Lấy danh sách AdminConfig cho dropdown, lọc theo code (ví dụ PARTY_IN_RELATIONSHIP, DRIVER_LICENSE_LEVEL).
    /// Chỉ cần đăng nhập.
    /// </summary>
    Task<List<AdminConfigSelectDto>> GetSelectListAsync(string? code = null);
}

