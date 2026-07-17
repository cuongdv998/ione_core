namespace iOne.ClaimStages;

public enum ClaimStageStatus
{
    New = 0,            // mới
    InProgress = 1,     // đang xử lý
    PendingApproval = 2,// chờ duyệt
    Approved = 3,       // đã duyệt
    Rejected = 4,       // từ chối duyệt
    Done = 5,           // hoàn thành
    Cancelled = 6       // hủy
}

