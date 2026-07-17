namespace iOne.Policy.Policies;

/// <summary>
/// Kết quả OCR ảnh đăng ký/đăng kiểm: biển số xe, số khung, số máy.
/// </summary>
public class OcrImageResultDto
{
    /// <summary>Biển số xe</summary>
    public string? VehiclePlate { get; set; }

    /// <summary>Số khung</summary>
    public string? ChassisNumber { get; set; }

    /// <summary>Số máy</summary>
    public string? EngineNumber { get; set; }
}
