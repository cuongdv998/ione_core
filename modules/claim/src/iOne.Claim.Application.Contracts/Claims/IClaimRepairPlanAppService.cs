using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace iOne.Claim.Claims;

public interface IClaimRepairPlanAppService : IApplicationService
{
    /// <summary>Tải dữ liệu khởi tạo: danh sách hạng mục từ giám định chi tiết, tỷ lệ khấu hao đã tính sẵn.</summary>
    Task<RepairPlanInitDataDto> GetInitDataAsync(Guid claimId, Guid workTaskId);

    /// <summary>Tab PASC chỉ cho hồ sơ sản phẩm VCX — dùng để ẩn tab trước khi gọi init.</summary>
    Task<RepairPlanPascEligibilityDto> GetPascEligibilityAsync(Guid claimId, Guid workTaskId);

    /// <summary>Danh sách garage (đối tác có partnerType = GARAGE).</summary>
    Task<List<RepairPlanGarageOptionDto>> GetGaragesAsync(string? search, int maxResultCount = 50);

    /// <summary>Lưu nháp phương án sửa chữa (status = draft).</summary>
    Task<RepairPlanSavedDto> SaveAsync(SaveRepairPlanInput input);

    /// <summary>Dữ liệu hiển thị popup trình duyệt (người duyệt, phân cấp, HSBT, SPBH).</summary>
    Task<RepairPlanSubmitInfoDto> GetSubmitInfoAsync(Guid claimId, Guid workTaskId);

    /// <summary>Trình duyệt phương án sửa chữa (status = new).</summary>
    Task<RepairPlanSavedDto> SubmitAsync(SaveRepairPlanInput input);

    Task<PagedResultDto<QuotationApprovalListRowDto>> GetApprovalListAsync(GetQuotationApprovalListInput input);

    Task ApproveQuotationApprovalAsync(ApproveQuotationApprovalInput input);

    Task RejectQuotationApprovalAsync(RejectQuotationApprovalInput input);

    Task ReassignQuotationApprovalAsync(ReassignQuotationApprovalInput input);
}
