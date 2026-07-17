using System.ComponentModel.DataAnnotations;
using iOne.ResTaskCategories;

namespace iOne.Master.ResTaskCategories;

public class CreateResTaskCategoryDto
{
    [Required(ErrorMessage = "ResTaskCategory:BusinessTypeRequired")]
    [Display(Name = "ResTaskCategory:BusinessType")]
    public ResTaskCategoryBusinessType BusinessType { get; set; }

    [Required(ErrorMessage = "ResTaskCategory:CodeRequired")]
    [StringLength(50, ErrorMessage = "ResTaskCategory:CodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "ResTaskCategory:CodeInvalid")]
    [Display(Name = "ResTaskCategory:Code")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "ResTaskCategory:NameRequired")]
    [StringLength(250, ErrorMessage = "ResTaskCategory:NameMaxLength")]
    [Display(Name = "ResTaskCategory:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "ResTaskCategory:StatusRequired")]
    [Display(Name = "ResTaskCategory:Status")]
    public ResTaskCategoryStatus Status { get; set; } = ResTaskCategoryStatus.Active;
}
