using System.ComponentModel.DataAnnotations;
using iOne.ResOrganizationTypes;

namespace iOne.Partner.ResOrganizationTypes;

public class UpdateResOrganizationTypeDto
{
    // ⚠️ QUAN TRỌNG: KHÔNG có Code field - không được sửa Code

    [Required(ErrorMessage = "ResOrganizationType:NameRequired")]
    [StringLength(250, ErrorMessage = "ResOrganizationType:NameMaxLength")]
    [Display(Name = "ResOrganizationType:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "ResOrganizationType:StatusRequired")]
    [Display(Name = "ResOrganizationType:Status")]
    public ResOrganizationTypeStatus Status { get; set; }

    [Required(ErrorMessage = "ResOrganizationType:TypeRequired")]
    [Display(Name = "ResOrganizationType:Type")]
    public OrganizationTypeType Type { get; set; }
}

