namespace iOne.AccountPaymentRequests;

public enum AccountPaymentRequestStatus
{
    Draft = 0,            // Nháp
    PendingApproval = 1,  // Chờ duyệt
    Approved = 2,         // Đã duyệt
    Rejected = 3,         // Từ chối duyệt
    Cancelled = 4         // Hủy
}
