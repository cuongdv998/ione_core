using System.ComponentModel.DataAnnotations;
using iOne.HrEmployeeLevels;

namespace iOne.Hr.HrEmployeeLevels;

public class CreateHrEmployeeLevelDto
{
    [Required(ErrorMessage = "HrEmployeeLevel:CodeRequired")]
    [StringLength(50, ErrorMessage = "HrEmployeeLevel:CodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "HrEmployeeLevel:CodeInvalidFormat")]
    [Display(Name = "HrEmployeeLevel:Code")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "HrEmployeeLevel:NameRequired")]
    [StringLength(250, ErrorMessage = "HrEmployeeLevel:NameMaxLength")]
    [Display(Name = "HrEmployeeLevel:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "HrEmployeeLevel:StatusRequired")]
    [Display(Name = "HrEmployeeLevel:Status")]
    public HrEmployeeLevelStatus Status { get; set; }

    [StringLength(500, ErrorMessage = "HrEmployeeLevel:DescriptionMaxLength")]
    [Display(Name = "HrEmployeeLevel:Description")]
    public string? Description { get; set; }
}

