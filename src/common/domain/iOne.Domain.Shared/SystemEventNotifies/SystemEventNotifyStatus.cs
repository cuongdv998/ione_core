namespace iOne.SystemEventNotifies;

public enum SystemEventNotifyStatus
{
    Pending = 0,   // Chờ gửi
    Sent = 1,      // Đã gửi
    Fail = 2,      // Lỗi
    Read = 3,      // Đã đọc
    Deactive = 4   // Đã xóa / Ngừng (dùng khi soft delete)
}
