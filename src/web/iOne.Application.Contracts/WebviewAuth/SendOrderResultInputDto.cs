using System.ComponentModel.DataAnnotations;

namespace iOne.WebviewAuth;

public class SendOrderResultInputDto
{
    public string? TransId { get; set; }
    public string? OrderId { get; set; }
    public string? Product { get; set; }
    public decimal? Amount { get; set; }
    public string? CustomerCode { get; set; }
    public string? ContractNumber { get; set; }
    public string? VehicleOwner { get; set; }
    public string? LicensePlate { get; set; }
    public string? EffectivePeriod { get; set; }

    [Required]
    public string CertificateUrl { get; set; } = null!;
}
