using System.ComponentModel.DataAnnotations;
using iOne.HrDepartmentTypes;

namespace iOne.Hr.HrDepartmentTypes;

public class UpdateHrDepartmentTypeDto
{
    [Required(ErrorMessage = "HrDepartmentType:NameRequired")]
    [StringLength(250, ErrorMessage = "HrDepartmentType:NameMaxLength")]
    [Display(Name = "HrDepartmentType:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "HrDepartmentType:StatusRequired")]
    [Display(Name = "HrDepartmentType:Status")]
    public HrDepartmentTypeStatus Status { get; set; }
}

