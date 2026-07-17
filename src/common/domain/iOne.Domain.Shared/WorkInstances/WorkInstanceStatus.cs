namespace iOne.WorkInstances;

public enum WorkInstanceStatus
{
    New = 0,           // mới tạo (chưa thực hiện)
    InProgress = 1,   // đang thực hiện
    Completed = 2,     // đã hoàn thành
    Accepted = 3,      // đã tiếp nhận
    Rejected = 4,      // từ chối
    Cancelled = 5,     // đã hủy
    WaitApprove = 6,   // chờ duyệt (đối với task cần duyệt khi hoàn thành)
    Approved = 7,      // đã duyệt (đối với task cần duyệt khi hoàn thành)
    Pending = 8,       // tạm dừng
    Return = 9         // trả lại
}
