namespace iOne.Claims;

public enum ProcessClaimType
{
    Own = 0,        // Broker xử lý
    Insurer = 1     // Bảo hiểm gốc xử lý (cần theo dõi tiến độ)
}
