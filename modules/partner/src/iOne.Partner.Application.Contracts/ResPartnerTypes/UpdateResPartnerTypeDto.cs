using System.ComponentModel.DataAnnotations;
using iOne.ResPartnerTypes;

namespace iOne.Partner.ResPartnerTypes;

public class UpdateResPartnerTypeDto
{
    // ⚠️ QUAN TRỌNG: Không có Code property - Code không được phép sửa

    [Required(ErrorMessage = "ResPartnerType:NameRequired")]
    [StringLength(250, ErrorMessage = "ResPartnerType:NameMaxLength")]
    [Display(Name = "ResPartnerType:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "ResPartnerType:StatusRequired")]
    [Display(Name = "ResPartnerType:Status")]
    public ResPartnerTypeStatus Status { get; set; }
}

