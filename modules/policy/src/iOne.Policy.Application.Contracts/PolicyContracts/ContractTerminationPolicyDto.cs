using System;

namespace iOne.Policy.PolicyContracts;

/// <summary>
/// DTO for a policy in the contract termination modal.
/// </summary>
public class ContractTerminationPolicyDto
{
    public Guid PolicyId { get; set; }

    /// <summary>
    /// Số đơn BH (from Policy.PolicyNo)
    /// </summary>
    public string PolicyNo { get; set; } = null!;

    /// <summary>
    /// Số GCN BH (from PolicyCertificate.CertificateNo)
    /// </summary>
    public string? CertificateNo { get; set; }

    /// <summary>
    /// Danh sách sản phẩm (from PolicyProduct join ProProduct, separated by semicolon)
    /// </summary>
    public string? Products { get; set; }

    /// <summary>
    /// Ngày hiệu lực (from PolicyVersion, format HH:mm dd/MM/yyyy)
    /// </summary>
    public string EffectDate { get; set; } = null!;

    /// <summary>
    /// Ngày hết hạn (from PolicyVersion, format HH:mm dd/MM/yyyy)
    /// </summary>
    public string ExpireDate { get; set; } = null!;

    /// <summary>
    /// Phí bảo hiểm (from PolicyVersion.PremiumTotal, VN currency format)
    /// </summary>
    public decimal PremiumTotal { get; set; }

    /// <summary>
    /// Phí cần hoàn (from CalculateRefundAmountAsync)
    /// </summary>
    public decimal RefundAmount { get; set; }

    /// <summary>
    /// Phí sẽ hoàn (default = RefundAmount, editable by user)
    /// </summary>
    public decimal ActualRefundAmount { get; set; }
}
