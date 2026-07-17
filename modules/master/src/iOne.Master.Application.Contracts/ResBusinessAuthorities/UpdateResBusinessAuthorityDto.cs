using System.ComponentModel.DataAnnotations;
using iOne.ResBusinessAuthorities;

namespace iOne.Master.ResBusinessAuthorities;

public class UpdateResBusinessAuthorityDto
{
    // Code và BusinessCode không được phép sửa

    [Required(ErrorMessage = "ResBusinessAuthority:NameRequired")]
    [StringLength(250, ErrorMessage = "ResBusinessAuthority:NameMaxLength")]
    [Display(Name = "ResBusinessAuthority:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "ResBusinessAuthority:StatusRequired")]
    [Display(Name = "ResBusinessAuthority:Status")]
    public ResBusinessAuthorityStatus Status { get; set; }
}
