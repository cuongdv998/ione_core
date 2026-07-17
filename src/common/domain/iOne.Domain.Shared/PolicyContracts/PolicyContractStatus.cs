namespace iOne.PolicyContracts;

public enum PolicyContractStatus
{
    Quotation = 0,    // Báo giá
    Draft = 1,        // Nháp
    Active = 2,       // Đang hiệu lực
    Expired = 3,      // Hết hạn
    Terminated = 4,   // Đã chấm dứt
    Cancelled = 5     // Đã hủy
}
