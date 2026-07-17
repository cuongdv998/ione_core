using System.ComponentModel.DataAnnotations;

namespace iOne.Partner.PaymentCallbacks;

public class PaymentCallbackRequestDto
{
    [Required]
    public string TransId { get; set; } = null!;

    [Required]
    public string PaymentResult { get; set; } = null!;

    public decimal PaymentAmount { get; set; }

    public CertInfo? Cert { get; set; }
}

public class CertInfo
{
    public string? CertificationNo { get; set; }

    public string? CertificationLink { get; set; }
}
