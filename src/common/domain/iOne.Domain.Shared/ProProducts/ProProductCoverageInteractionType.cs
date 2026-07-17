namespace iOne.ProProducts;

/// <summary>
/// Loại ràng buộc giữa các phạm vi bảo hiểm
/// </summary>
public enum ProProductCoverageInteractionType
{
    Dependency,     // Phụ thuộc (chọn B thì bắt buộc phải chọn A)
    Incompatible,   // Không tương thích (chọn B thì không được chọn A)
    Exclusive       // Loại trừ nhau (chỉ chọn A hoặc B)
}
