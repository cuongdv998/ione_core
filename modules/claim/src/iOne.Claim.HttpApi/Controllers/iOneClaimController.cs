using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Claim.Claims;

namespace iOne.Claim.Controllers;

[RemoteService(Name = ClaimRemoteServiceConsts.RemoteServiceName)]
[Area(ClaimRemoteServiceConsts.ModuleName)]
[Route("api/claims")]
public class ClaimController : AbpControllerBase
{
    protected IClaimAppService AppService { get; }

    public ClaimController(IClaimAppService appService)
    {
        AppService = appService;
    }

    [HttpGet("list")]
    public virtual Task<PagedResultDto<ClaimDto>> GetListAsync([FromQuery] GetClaimsInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    public virtual Task<ClaimDetailDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost("{id}/snapshot-link")]
    public virtual Task<string> GetSnapshotLinkAsync(Guid id)
    {
        return AppService.GetSnapshotLinkAsync(id);
    }

    /// <summary>
    /// Giải mã snapshot code (từ URL) để lấy claim Id. Dùng khi xử lý link snapshot (firstNotifyOfLoss/incident/infor/{code}).
    /// </summary>
    [HttpGet("snapshot/{snapshotCode}/claim-id")]
    public virtual Task<Guid?> GetClaimIdFromSnapshotCodeAsync(string snapshotCode)
    {
        return AppService.GetClaimIdFromSnapshotCodeAsync(snapshotCode);
    }

    [HttpPost]
    public virtual Task<CreateClaimResponseDto> CreateAsync([FromBody] CreateClaimDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    public virtual Task<ClaimDetailDto> UpdateAsync(Guid id, [FromBody] UpdateClaimDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    /// <summary>
    /// Hủy yêu cầu bồi thường. Chỉ cho phép khi trạng thái là Nháp (Draft).
    /// </summary>
    [HttpPut("{id}/cancel")]
    public virtual Task<ClaimDetailDto> CancelAsync(Guid id)
    {
        return AppService.CancelAsync(id);
    }

    /// <summary>
    /// Gửi yêu cầu bồi thường đi giám định. Khởi tạo quy trình Elsa (CLAIM_ASSIGN).
    /// </summary>
    [HttpPost("{id}/submit-assessment")]
    public virtual Task SubmitForAssessmentAsync(Guid id)
    {
        return AppService.SubmitForAssessmentAsync(id);
    }
}

