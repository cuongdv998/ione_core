using System.ComponentModel.DataAnnotations;

namespace iOne.Payment.VnPay;

public class VnPayQueryDrRequestDto
{
    [Required]
    [MaxLength(100)]
    public string TxnRef { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string OrderInfo { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^\d{14}$")]
    public string TransactionDate { get; set; } = string.Empty;

    [MaxLength(15)]
    public string? TransactionNo { get; set; }

    [MaxLength(45)]
    public string? IpAddress { get; set; }

    [MaxLength(32)]
    public string? RequestId { get; set; }
}
