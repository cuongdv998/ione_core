using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace iOne.Claim.Claims;

// ──────────────────────────────────────────────
// Init Data
// ──────────────────────────────────────────────

public class RepairPlanInitDataDto
{
    public List<RepairPlanObjectTypeDto> ObjectTypes { get; set; } = new();
    public List<RepairPlanAssessmentItemDto> Items { get; set; } = new();
    /// <summary>Danh sách đối tượng bảo hiểm của hồ sơ (ClaimFolderIncidentObject + ResObjectType.Name).</summary>
    public List<RepairPlanIncidentObjectDto> IncidentObjects { get; set; } = new();
    /// <summary>Dữ liệu phương án đã lưu trước đó (lấy từ ClaimFolderQuotationApproval.Data).</summary>
    public string? SavedData { get; set; }
    /// <summary>Trạng thái PASC/ghi nhận báo giá (chuỗi cột DB: new, pending_approval, approved, …).</summary>
    public string? QuotationApprovalStatus { get; set; }

    /// <summary>
    /// True khi hồ sơ bồi thường gắn sản phẩm loại VCX (bảo hiểm vật chất xe).
    /// Khi false, Items/IncidentObjects rỗng và SavedData/QuotationApprovalStatus không trả về cho UI.
    /// </summary>
    public bool IsPascProductEligible { get; set; }
}

/// <summary>Kết quả kiểm tra hiển thị tab PASC (GET pasc-eligibility).</summary>
public class RepairPlanPascEligibilityDto
{
    public bool Eligible { get; set; }
}

public class RepairPlanObjectTypeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
}

public class RepairPlanIncidentObjectDto
{
    public Guid Id { get; set; }
    public Guid ObjectTypeId { get; set; }
    public string ObjectTypeName { get; set; } = null!;
    /// <summary>Biển số xe (nếu có).</summary>
    public string? CarPlate { get; set; }
}

public class RepairPlanAssessmentItemDto
{
    public Guid Id { get; set; }
    public Guid ItemId { get; set; }
    public string ItemName { get; set; } = null!;
    /// <summary>Phương án: Sửa chữa / Thay thế / …</summary>
    public string PlanName { get; set; } = null!;
    /// <summary>ID RESCLAIMPLAN trên hạng mục — dùng đồng bộ claim_folder_item_plan.</summary>
    public Guid? ClaimPlanId { get; set; }
    public decimal Quantity { get; set; }
    /// <summary>ID của ResObjectTypeItem — dùng để tra khấu hao.</summary>
    public Guid ObjectTypeItemId { get; set; }
    /// <summary>Tỉ lệ khấu hao vật tư (%) — được tính sẵn tại backend (từ ClaimFolderItem.DepreciationPercent).</summary>
    public double? DepreciationPercent { get; set; }
}

// ──────────────────────────────────────────────
// Garage list
// ──────────────────────────────────────────────

public class RepairPlanGarageOptionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
}

// ──────────────────────────────────────────────
// Submit dialog / approvers
// ──────────────────────────────────────────────

public class RepairPlanApproverDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
}

public class RepairPlanSubmitInfoDto
{
    public string SubmitterName { get; set; } = null!;
    public string FolderNo { get; set; } = null!;
    public string ProductName { get; set; } = null!;
    /// <summary>Phân cấp — tên cấu hình res_business_authority.</summary>
    public string BusinessAuthorityName { get; set; } = null!;
    public List<RepairPlanApproverDto> Approvers { get; set; } = new();
    /// <summary>Gợi ý người duyệt (random trong danh sách nếu có).</summary>
    public Guid? SuggestedApproverId { get; set; }
}

// ──────────────────────────────────────────────
// Save / Submit
// ──────────────────────────────────────────────

public class SaveRepairPlanInput
{
    public Guid ClaimId { get; set; }
    public Guid? ClaimFolderId { get; set; }
    /// <summary>
    /// Work task hiện tại (VD giám định chi tiết). Gửi khi lưu nháp để gán đúng ClaimFolderId;
    /// khi trình duyệt vẫn bắt buộc để resolve hồ sơ.
    /// </summary>
    public Guid? WorkTaskId { get; set; }
    public Guid? PartnerId { get; set; }
    public decimal ClaimAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal DepreciationAmount { get; set; }
    public decimal ExpenseAmount { get; set; }
    public decimal? AssessmentAmount { get; set; }
    public decimal? LossPreventionAmount { get; set; }
    public decimal? RescueAmount { get; set; }
    public decimal? OtherAmount { get; set; }
    public DateTime? SubmittedDate { get; set; }
    /// <summary>Toàn bộ dữ liệu form JSON (để phục vụ màn Duyệt PASC).</summary>
    public string? Data { get; set; }
    /// <summary>Người được chọn để duyệt (HrEmployee.Id).</summary>
    public Guid? ApproverId { get; set; }
    /// <summary>Ghi chú trình duyệt.</summary>
    public string? Description { get; set; }
}

// ──────────────────────────────────────────────
// Response
// ──────────────────────────────────────────────

public class RepairPlanSavedDto
{
    public Guid Id { get; set; }
    public string Status { get; set; } = null!;
}

// ──────────────────────────────────────────────
// Quotation (PASC) approval list / actions
// ──────────────────────────────────────────────

/// <summary>Filter + paging for danh sách duyệt PASC (theo assignee work_task).</summary>
public class GetQuotationApprovalListInput : PagedAndSortedResultRequestDto
{
    public Guid? LobId { get; set; }
    public Guid? InsurerId { get; set; }
    public Guid? ProcessDeptId { get; set; }
    public Guid? ClaimId { get; set; }
    /// <summary>new | approved | rejected — lọc theo nhóm trạng thái UI.</summary>
    public string? ApprovalStatus { get; set; }
    public string? CarPlate { get; set; }
    public string? Vin { get; set; }
    public string? EngineNumber { get; set; }
    public string? FolderNo { get; set; }
    public Guid? ReporterId { get; set; }
}

public class QuotationApprovalListRowDto
{
    public Guid WorkTaskId { get; set; }
    public Guid ClaimId { get; set; }
    public Guid ClaimFolderId { get; set; }
    public string ClaimCode { get; set; } = null!;
    public string FolderNo { get; set; } = null!;
    public string? InsurerCode { get; set; }
    public string? LobName { get; set; }
    public string? ProductName { get; set; }
    public string? CarPlate { get; set; }
    public string? ReporterName { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public DateTime? IncidentDate { get; set; }
    public string? OnLocation { get; set; }
    public decimal TotalPascAmount { get; set; }
    /// <summary>WorkTask.Status — giống cột DB (enum.ToString().ToLowerInvariant(): new, inprogress, completed, …).</summary>
    public string WorkTaskStatus { get; set; } = null!;
    /// <summary>new | approved | rejected — hiển thị & lọc.</summary>
    public string ApprovalUiStatus { get; set; } = null!;
    /// <summary>Work task giám định chi tiết (BusinessKey = ClaimFolder.Id) để hiển thị tab trong màn duyệt PASC.</summary>
    public Guid? DetailAssessmentWorkTaskId { get; set; }
}

public class ApproveQuotationApprovalInput
{
    public Guid WorkTaskId { get; set; }
    public string? Comment { get; set; }
}

public class RejectQuotationApprovalInput
{
    public Guid WorkTaskId { get; set; }
    public Guid ReasonId { get; set; }
    public string? Comment { get; set; }
}

public class ReassignQuotationApprovalInput
{
    public Guid WorkTaskId { get; set; }
    public Guid NewAssigneeId { get; set; }
    /// <summary>Lý do điều chuyển (res_reason).</summary>
    public Guid ReasonId { get; set; }
    /// <summary>Mô tả chi tiết lý do điều chuyển.</summary>
    public string? ReasonDescription { get; set; }
    public string? Comment { get; set; }
}
