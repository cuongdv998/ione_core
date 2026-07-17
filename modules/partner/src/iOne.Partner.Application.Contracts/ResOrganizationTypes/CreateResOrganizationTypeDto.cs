using System.ComponentModel.DataAnnotations;
using iOne.ResOrganizationTypes;

namespace iOne.Partner.ResOrganizationTypes;

public class CreateResOrganizationTypeDto
{
    [Required(ErrorMessage = "ResOrganizationType:CodeRequired")]
    [StringLength(25, ErrorMessage = "ResOrganizationType:CodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "ResOrganizationType:CodeInvalidFormat")]
    [Display(Name = "ResOrganizationType:Code")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "ResOrganizationType:NameRequired")]
    [StringLength(250, ErrorMessage = "ResOrganizationType:NameMaxLength")]
    [Display(Name = "ResOrganizationType:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "ResOrganizationType:Status")]
    public ResOrganizationTypeStatus Status { get; set; } = ResOrganizationTypeStatus.Active;

    [Required(ErrorMessage = "ResOrganizationType:TypeRequired")]
    [Display(Name = "ResOrganizationType:Type")]
    public OrganizationTypeType Type { get; set; } = OrganizationTypeType.TC; // Mặc định là TC (Tổ chức)
}

