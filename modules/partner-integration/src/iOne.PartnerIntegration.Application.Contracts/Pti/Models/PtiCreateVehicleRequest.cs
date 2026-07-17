using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace iOne.PartnerIntegration.Pti.Models;

/// <summary>
/// PTI "Codebook" shape: integration code + display name (camelCase JSON).
/// </summary>
public class PtiCodebook
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = null!;

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;
}

/// <summary>
/// Request body for POST bep-pti-api/vehicle/create.
/// Field names are camelCase to match PTI JSON contract.
/// </summary>
public class PtiCreateVehicleRequest
{
    /// <summary>Mã kênh bán bảo hiểm. Default: KENH004</summary>
    [JsonPropertyName("channelCode")]
    public string ChannelCode { get; set; } = "KENH004";

    /// <summary>Id chủ động do iBDS truyền vào — dùng để tra cứu và đối soát với PTI</summary>
    [JsonPropertyName("transId")]
    public string? TransId { get; set; }

    [JsonPropertyName("vehicleInfo")]
    public PtiVehicleInfo? VehicleInfo { get; set; }

    [JsonPropertyName("ppcInfoSelection")]
    public List<PtiPpcInfoSelection>? PpcInfoSelection { get; set; }

    [JsonPropertyName("insurancePeriod")]
    public PtiInsurancePeriod? InsurancePeriod { get; set; }

    [JsonPropertyName("customerInfo")]
    public PtiCustomerInfo? CustomerInfo { get; set; }

    [JsonPropertyName("ownerInfo")]
    public PtiOwnerInfo? OwnerInfo { get; set; }

    [JsonPropertyName("beneficiaryInfo")]
    public PtiBeneficiaryInfo? BeneficiaryInfo { get; set; }

    [JsonPropertyName("billInfo")]
    public PtiBillInfo? BillInfo { get; set; }

    [JsonPropertyName("coverageLimit")]
    public PtiCoverageLimit? CoverageLimit { get; set; }
}

/// <summary>Thông tin xe — Bảng 2 (loại xe) và Bảng 3 (tình trạng xe)</summary>
public class PtiVehicleInfo
{
    /// <summary>Loại xe (Codebook)</summary>
    [JsonPropertyName("vehicleType")]
    public PtiCodebook VehicleType { get; set; } = null!;

    /// <summary>Tình trạng xe (Codebook)</summary>
    [JsonPropertyName("vehicleStatus")]
    public PtiCodebook VehicleStatus { get; set; } = null!;

    /// <summary>Biển số xe (bắt buộc khi xe đã lưu hành)</summary>
    [JsonPropertyName("licensePlate")]
    public string? LicensePlate { get; set; }

    /// <summary>Số khung</summary>
    [JsonPropertyName("frameNumber")]
    public string? FrameNumber { get; set; }

    /// <summary>Số máy (bắt buộc khi xe chưa lưu hành)</summary>
    [JsonPropertyName("engineNumber")]
    public string? EngineNumber { get; set; }
}

/// <summary>Thông tin gói bảo hiểm — Bảng 7</summary>
public class PtiPpcInfoSelection
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = null!;

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("isChoice")]
    public bool IsChoice { get; set; }

    [JsonPropertyName("assuranceType")]
    public string AssuranceType { get; set; } = null!;

    [JsonPropertyName("targetFeeValue")]
    public int TargetFeeValue { get; set; }
}

/// <summary>Thời hạn bảo hiểm</summary>
public class PtiInsurancePeriod
{
    /// <summary>Ngày bắt đầu — dd/MM/yyyy</summary>
    [JsonPropertyName("effectiveDate")]
    public string EffectiveDate { get; set; } = null!;

    /// <summary>Giờ bắt đầu — HH:mm</summary>
    [JsonPropertyName("effectiveHour")]
    public string EffectiveHour { get; set; } = null!;

    /// <summary>Ngày kết thúc — dd/MM/yyyy</summary>
    [JsonPropertyName("expiryDate")]
    public string ExpiryDate { get; set; } = null!;

    /// <summary>Giờ kết thúc — mặc định bằng giờ bắt đầu</summary>
    [JsonPropertyName("expiryHour")]
    public string ExpiryHour { get; set; } = null!;
}

/// <summary>Thông tin bên mua bảo hiểm</summary>
public class PtiCustomerInfo
{
    /// <summary>Loại khách hàng (Codebook)</summary>
    [JsonPropertyName("customerType")]
    public PtiCodebook CustomerType { get; set; } = null!;

    [JsonPropertyName("fullName")]
    public string FullName { get; set; } = null!;

    [JsonPropertyName("phone")]
    public string Phone { get; set; } = null!;

    [JsonPropertyName("address")]
    public string Address { get; set; } = null!;

    [JsonPropertyName("email")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Email { get; set; }

    /// <summary>Loại hình tổ chức (Codebook) — bắt buộc khi khách hàng là DN/PDN</summary>
    [JsonPropertyName("orgType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PtiCodebook? OrgType { get; set; }

    /// <summary>Mã số thuế — bắt buộc khi khách hàng là DN/PDN</summary>
    [JsonPropertyName("taxCode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TaxCode { get; set; }
}

/// <summary>Thông tin chủ xe</summary>
public class PtiOwnerInfo
{
    /// <summary>Có phải người mua hay không</summary>
    [JsonPropertyName("isCustomer")]
    public bool IsCustomer { get; set; }

    [JsonPropertyName("customerType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PtiCodebook? CustomerType { get; set; }

    [JsonPropertyName("fullName")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? FullName { get; set; }

    [JsonPropertyName("phone")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Phone { get; set; }

    [JsonPropertyName("address")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Address { get; set; }

    [JsonPropertyName("email")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Email { get; set; }

    [JsonPropertyName("orgType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PtiCodebook? OrgType { get; set; }

    [JsonPropertyName("taxCode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TaxCode { get; set; }
}

/// <summary>Thông tin người thụ hưởng bảo hiểm</summary>
public class PtiBeneficiaryInfo
{
    /// <summary>Có phải chủ xe hay không</summary>
    [JsonPropertyName("isOwner")]
    public bool IsOwner { get; set; }

    [JsonPropertyName("customerType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PtiCodebook? CustomerType { get; set; }

    [JsonPropertyName("fullName")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? FullName { get; set; }

    [JsonPropertyName("address")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Address { get; set; }

    [JsonPropertyName("taxCode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TaxCode { get; set; }
}

/// <summary>Thông tin xuất hóa đơn</summary>
public class PtiBillInfo
{
    [JsonPropertyName("isChoice")]
    public bool IsChoice { get; set; }

    /// <summary>Đối tượng xuất hóa đơn (Codebook). Bắt buộc khi IsChoice=true</summary>
    [JsonPropertyName("billActor")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PtiCodebook? BillActor { get; set; }

    [JsonPropertyName("email")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Email { get; set; }
}

public class PtiCoverageLimit
{
    [JsonPropertyName("assuranceRange")]
    public List<PtiAssuranceRange>? AssuranceRange { get; set; }
}

public class PtiAssuranceRange
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("value")]
    public List<PtiAssuranceValue>? Value { get; set; }

    [JsonPropertyName("unit")]
    public string? Unit { get; set; }

    [JsonPropertyName("isChoice")]
    public bool IsChoice { get; set; }

    [JsonPropertyName("numberPeople")]
    public int? NumberPeople { get; set; }
}

public class PtiAssuranceValue
{
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("isChoice")]
    public bool IsChoice { get; set; }
}
