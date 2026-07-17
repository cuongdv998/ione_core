using System.ComponentModel.DataAnnotations;
using iOne.ResTaskCategories;

namespace iOne.Master.ResTaskCategories;

public class UpdateResTaskCategoryDto
{
    // Code và BusinessType không được phép sửa

    [Required(ErrorMessage = "ResTaskCategory:NameRequired")]
    [StringLength(250, ErrorMessage = "ResTaskCategory:NameMaxLength")]
    [Display(Name = "ResTaskCategory:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "ResTaskCategory:StatusRequired")]
    [Display(Name = "ResTaskCategory:Status")]
    public ResTaskCategoryStatus Status { get; set; }
}
