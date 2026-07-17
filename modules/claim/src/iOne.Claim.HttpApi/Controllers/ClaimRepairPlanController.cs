using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using iOne.Claim.Claims;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;

namespace iOne.Claim.Controllers;

[RemoteService(Name = ClaimRemoteServiceConsts.RemoteServiceName)]
[Area(ClaimRemoteServiceConsts.ModuleName)]
[Route("api/claim-repair-plan")]
public class ClaimRepairPlanController : AbpControllerBase
{
    protected IClaimRepairPlanAppService AppService { get; }

    public ClaimRepairPlanController(IClaimRepairPlanAppService appService)
    {
        AppService = appService;
    }

    /// <summary>Tải dữ liệu khởi tạo: hạng mục giám định chi tiết + tỷ lệ khấu hao.</summary>
    [HttpGet("{claimId:guid}/init")]
    public virtual Task<RepairPlanInitDataDto> GetInitDataAsync(Guid claimId, [FromQuery] Guid workTaskId)
    {
        return AppService.GetInitDataAsync(claimId, workTaskId);
    }

    /// <summary>True nếu hồ sơ gắn sản phẩm loại VCX (vật chất xe) — ẩn tab PASC khi false.</summary>
    [HttpGet("{claimId:guid}/pasc-eligibility")]
    public virtual Task<RepairPlanPascEligibilityDto> GetPascEligibilityAsync(Guid claimId, [FromQuery] Guid workTaskId)
    {
        return AppService.GetPascEligibilityAsync(claimId, workTaskId);
    }

    /// <summary>Danh sách garage để autocomplete.</summary>
    [HttpGet("garages")]
    public virtual Task<List<RepairPlanGarageOptionDto>> GetGaragesAsync(
        [FromQuery] string? search,
        [FromQuery] int maxResultCount = 50)
    {
        return AppService.GetGaragesAsync(search, maxResultCount);
    }

    /// <summary>Dữ liệu hiển thị popup trình duyệt (người duyệt, phân cấp, HSBT, SPBH).</summary>
    [HttpGet("{claimId:guid}/submit-info")]
    public virtual Task<RepairPlanSubmitInfoDto> GetSubmitInfoAsync(Guid claimId, [FromQuery] Guid workTaskId)
    {
        return AppService.GetSubmitInfoAsync(claimId, workTaskId);
    }

    /// <summary>Lưu nháp phương án sửa chữa.</summary>
    [HttpPost("save")]
    public virtual Task<RepairPlanSavedDto> SaveAsync([FromBody] SaveRepairPlanInput input)
    {
        return AppService.SaveAsync(input);
    }

    /// <summary>Trình duyệt phương án sửa chữa.</summary>
    [HttpPost("submit")]
    public virtual Task<RepairPlanSavedDto> SubmitAsync([FromBody] SaveRepairPlanInput input)
    {
        return AppService.SubmitAsync(input);
    }

    /// <summary>Danh sách duyệt PASC.</summary>
    [HttpGet("approval-list")]
    public virtual Task<PagedResultDto<QuotationApprovalListRowDto>> GetApprovalListAsync([FromQuery] GetQuotationApprovalListInput input)
    {
        return AppService.GetApprovalListAsync(input);
    }

    [HttpPost("approve")]
    public virtual Task ApproveQuotationApprovalAsync([FromBody] ApproveQuotationApprovalInput input)
    {
        return AppService.ApproveQuotationApprovalAsync(input);
    }

    [HttpPost("reject")]
    public virtual Task RejectQuotationApprovalAsync([FromBody] RejectQuotationApprovalInput input)
    {
        return AppService.RejectQuotationApprovalAsync(input);
    }

    [HttpPost("reassign")]
    public virtual Task ReassignQuotationApprovalAsync([FromBody] ReassignQuotationApprovalInput input)
    {
        return AppService.ReassignQuotationApprovalAsync(input);
    }
}
