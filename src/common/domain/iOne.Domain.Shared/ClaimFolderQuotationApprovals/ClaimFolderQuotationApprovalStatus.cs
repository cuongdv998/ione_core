namespace iOne.ClaimFolderQuotationApprovals;

/// <summary>
/// Trạng thái phê duyệt PASC/báo giá (lưu DB dạng chuỗi: new, inprogress, pending_approval, …).
/// </summary>
public enum ClaimFolderQuotationApprovalStatus
{
    /// <summary>Mới</summary>
    New,
    /// <summary>Đang xử lý</summary>
    InProgress,
    /// <summary>Chờ duyệt</summary>
    Pending_Approval,
    /// <summary>Đã duyệt</summary>
    Approved,
    /// <summary>Từ chối duyệt</summary>
    Rejected,
    /// <summary>Hoàn thành</summary>
    Done,
    /// <summary>Hủy</summary>
    Cancelled
}
