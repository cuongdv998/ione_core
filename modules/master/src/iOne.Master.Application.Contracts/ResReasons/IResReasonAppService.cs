using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResReasons;

public interface IResReasonAppService : ICrudAppService<
    ResReasonDto,
    Guid,
    GetResReasonsInput,
    CreateResReasonDto,
    UpdateResReasonDto>
{
    /// <summary>
    /// Lấy danh sách lý do cho dropdown theo mã nhóm (ví dụ INCIDENT_REASON). Chỉ cần đăng nhập.
    /// </summary>
    Task<List<ResReasonSelectDto>> GetSelectListByGroupCodeAsync(string code);
}
