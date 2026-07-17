using System.ComponentModel.DataAnnotations;

namespace iOne.WebviewAuth;

public class WebviewAuthInputDto
{
    [Required]
    [StringLength(128)]
    public string PartnerCode { get; set; } = null!;

    [Required]
    [StringLength(2048)]
    public string PartnerToken { get; set; } = null!;
}
