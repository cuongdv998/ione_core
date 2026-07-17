using System.ComponentModel.DataAnnotations;
using iOne.ResBusinessAuthorities;

namespace iOne.Master.ResBusinessAuthorities;

public class CreateResBusinessAuthorityDto
{
    [Required(ErrorMessage = "ResBusinessAuthority:CodeRequired")]
    [StringLength(25, ErrorMessage = "ResBusinessAuthority:CodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "ResBusinessAuthority:CodeInvalid")]
    [Display(Name = "ResBusinessAuthority:Code")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "ResBusinessAuthority:BusinessCodeRequired")]
    [StringLength(50, ErrorMessage = "ResBusinessAuthority:BusinessCodeMaxLength")]
    [Display(Name = "ResBusinessAuthority:BusinessCode")]
    public string BusinessCode { get; set; } = null!;

    [Required(ErrorMessage = "ResBusinessAuthority:NameRequired")]
    [StringLength(250, ErrorMessage = "ResBusinessAuthority:NameMaxLength")]
    [Display(Name = "ResBusinessAuthority:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "ResBusinessAuthority:StatusRequired")]
    [Display(Name = "ResBusinessAuthority:Status")]
    public ResBusinessAuthorityStatus Status { get; set; } = ResBusinessAuthorityStatus.Active;
}
