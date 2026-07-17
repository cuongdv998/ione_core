using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace iOne.PartnerIntegration.Pti.Models;

/// <summary>
/// Response from POST bep-pti-api/vehicle/create.
/// </summary>
public class PtiCreateVehicleResponse
{
    /// <summary>Mã id của đơn do PTI cấp — key để sửa đổi bổ sung. Không lưu vào Policy.InsurerPolicyNo; InsurerPolicyNo lưu transId từ request.</summary>
    [JsonPropertyName("taskId")]
    public long TaskId { get; set; }

    /// <summary>Số hợp đồng. Lưu vào PolicyContract.InsurerContractCode.</summary>
    [JsonPropertyName("contractNumber")]
    public string? ContractNumber { get; set; }

    /// <summary>Loại đơn — NEW cho đơn khởi tạo lần đầu, UPDATE cho sửa đổi bổ sung.</summary>
    [JsonPropertyName("issueType")]
    public PtiIssueType? IssueType { get; set; }

    [JsonPropertyName("paymentInfoDetail")]
    public PtiPaymentInfoDetail? PaymentInfoDetail { get; set; }
}

/// <summary>
/// PTI returns issueType as an object { code, name }, not a plain string.
/// </summary>
public class PtiIssueType
{
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

public class PtiPaymentInfoDetail
{
    /// <summary>PTI IBDS returns qrCode as an array of data-URL strings.</summary>
    [JsonPropertyName("qrCode")]
    public List<string>? QrCode { get; set; }

    /// <summary>Thời điểm hết hạn thanh toán — HH:mm dd/MM/yyyy</summary>
    [JsonPropertyName("paymentExpired")]
    public string? PaymentExpired { get; set; }

    /// <summary>Số tiền cần thanh toán</summary>
    [JsonPropertyName("paymentAmount")]
    public long PaymentAmount { get; set; }
}

/// <summary>
/// PTI error envelope returned when the HTTP call itself fails with a PTI-structured body.
/// </summary>
public class PtiErrorResponse
{
    [JsonPropertyName("statusCode")]
    public int? StatusCode { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("code")]
    public string? Code { get; set; }
}
