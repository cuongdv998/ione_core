using System.ComponentModel.DataAnnotations;

namespace iOne.Payment.VnPay;

public class CreateVnPayRequestDto
{
    public string OrderId { get; set; } = string.Empty;

    [Required]
    public string OrderInfo { get; set; } = string.Empty;

    [Required]
    [Range(1000, double.MaxValue, ErrorMessage = "Số tiền tối thiểu là 1,000 VND")]
    public decimal Amount { get; set; }

    public string? BankCode { get; set; }

    public string? Locale { get; set; }

    public string IpAddress { get; set; } = string.Empty;
}
