namespace iOne.Policy.Policies;

/// <summary>
/// Một dòng loại xe cho combobox cấp đơn xe máy (InsurerDictionary: value = OwnCode, label từ ExtraData).
/// </summary>
public class MotorbikeInsurerVehicleTypeOptionDto
{
    /// <summary>Giá trị chọn trên form (= InsurerDictionary.OwnCode).</summary>
    public string SelectValue { get; set; } = null!;

    /// <summary>Mã gửi RiskObjectMotor.CarTypeCode (= OwnCode).</summary>
    public string CarTypeCode { get; set; } = null!;

    /// <summary>Nhãn hiển thị từ ExtraData (JSON name/displayName hoặc nguyên chuỗi).</summary>
    public string Label { get; set; } = null!;
}
