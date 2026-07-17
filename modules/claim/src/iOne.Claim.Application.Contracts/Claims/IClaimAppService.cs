using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace iOne.Claim.Claims;

public interface IClaimAppService : IApplicationService
{
    Task<PagedResultDto<ClaimDto>> GetListAsync(GetClaimsInput input);

    Task<CreateClaimResponseDto> CreateAsync(CreateClaimDto input);

    Task<ClaimDetailDto> GetAsync(Guid id);

    /// <summary>
    /// Cập nhật yêu cầu bồi thường. Chỉ cho phép khi trạng thái là Nháp (Draft).
    /// </summary>
    Task<ClaimDetailDto> UpdateAsync(Guid id, UpdateClaimDto input);

    /// <summary>
    /// Hủy yêu cầu bồi thường. Chỉ cho phép khi trạng thái là Nháp (Draft). Chuyển status sang Đã hủy (Called).
    /// </summary>
    Task<ClaimDetailDto> CancelAsync(Guid id);

    /// <summary>
    /// Lấy (và nếu cần thì tự sinh) snapshot link cho khách hàng để bổ sung thông tin.
    /// Trả về URL hoàn chỉnh để FE có thể copy cho khách hàng.
    /// </summary>
    Task<string> GetSnapshotLinkAsync(Guid id);

    /// <summary>
    /// Gửi yêu cầu bồi thường đi giám định. Kiểm tra đã giao đơn vị giám định và người giám định,
    /// sau đó khởi tạo quy trình Elsa (CLAIM_ASSIGN) để giao công việc.
    /// </summary>
    Task SubmitForAssessmentAsync(Guid id);

    /// <summary>
    /// Giải mã chuỗi snapshot (lấy từ URL) về claim Id. Trả về true nếu giải mã thành công.
    /// </summary>
    bool TryDecryptSnapshotCodeToClaimId(string snapshotCode, out Guid? claimId);

    /// <summary>
    /// Giải mã chuỗi snapshot (lấy từ URL) về claim Id. Trả về claim Id hoặc null nếu không hợp lệ. Dùng cho API khi xử lý URL snapshot.
    /// </summary>
    Task<Guid?> GetClaimIdFromSnapshotCodeAsync(string snapshotCode);
}
