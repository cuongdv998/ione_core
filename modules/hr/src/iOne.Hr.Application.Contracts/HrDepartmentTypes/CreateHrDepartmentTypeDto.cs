using System.ComponentModel.DataAnnotations;
using iOne.HrDepartmentTypes;

namespace iOne.Hr.HrDepartmentTypes;

public class CreateHrDepartmentTypeDto
{
    [Required(ErrorMessage = "HrDepartmentType:CodeRequired")]
    [StringLength(50, ErrorMessage = "HrDepartmentType:CodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "HrDepartmentType:CodeInvalidFormat")]
    [Display(Name = "HrDepartmentType:Code")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "HrDepartmentType:NameRequired")]
    [StringLength(250, ErrorMessage = "HrDepartmentType:NameMaxLength")]
    [Display(Name = "HrDepartmentType:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "HrDepartmentType:StatusRequired")]
    [Display(Name = "HrDepartmentType:Status")]
    public HrDepartmentTypeStatus Status { get; set; }
}

