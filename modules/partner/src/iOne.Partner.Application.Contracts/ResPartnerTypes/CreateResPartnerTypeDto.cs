using System.ComponentModel.DataAnnotations;
using iOne.ResPartnerTypes;

namespace iOne.Partner.ResPartnerTypes;

public class CreateResPartnerTypeDto
{
    [Required(ErrorMessage = "ResPartnerType:CodeRequired")]
    [StringLength(25, ErrorMessage = "ResPartnerType:CodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "ResPartnerType:CodeInvalidFormat")]
    [Display(Name = "ResPartnerType:Code")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "ResPartnerType:NameRequired")]
    [StringLength(250, ErrorMessage = "ResPartnerType:NameMaxLength")]
    [Display(Name = "ResPartnerType:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "ResPartnerType:StatusRequired")]
    [Display(Name = "ResPartnerType:Status")]
    public ResPartnerTypeStatus Status { get; set; }
}

