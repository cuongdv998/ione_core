namespace iOne.Claims;

public enum ClaimStatus
{
    Draft = 0,              // Nháp
    PendingReceive = 1,     // Chờ xử lý
    InProgress = 2,         // Đang xử lý
    Closed = 3,             // Đã đóng
    Called = 4              // Đã hủy
}
