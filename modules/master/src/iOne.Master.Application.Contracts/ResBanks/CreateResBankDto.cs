using System.ComponentModel.DataAnnotations;
using iOne.ResBanks;

namespace iOne.Master.ResBanks;

public class CreateResBankDto
{
    [Required(ErrorMessage = "ResBank:CodeRequired")]
    [StringLength(25, ErrorMessage = "ResBank:CodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "ResBank:CodeInvalid")]
    [Display(Name = "ResBank:Code")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "ResBank:NameRequired")]
    [StringLength(250, ErrorMessage = "ResBank:NameMaxLength")]
    [Display(Name = "ResBank:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "ResBank:StatusRequired")]
    [Display(Name = "ResBank:Status")]
    public ResBankStatus Status { get; set; } = ResBankStatus.Active;
}

