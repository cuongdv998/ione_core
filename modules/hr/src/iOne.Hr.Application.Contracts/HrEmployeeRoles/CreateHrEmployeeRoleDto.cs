using System.ComponentModel.DataAnnotations;
using iOne.HrEmployeeRoles;

namespace iOne.Hr.HrEmployeeRoles;

public class CreateHrEmployeeRoleDto
{
    [Required(ErrorMessage = "HrEmployeeRole:CodeRequired")]
    [StringLength(50, ErrorMessage = "HrEmployeeRole:CodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "HrEmployeeRole:CodeInvalidFormat")]
    [Display(Name = "HrEmployeeRole:Code")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "HrEmployeeRole:NameRequired")]
    [StringLength(250, ErrorMessage = "HrEmployeeRole:NameMaxLength")]
    [Display(Name = "HrEmployeeRole:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "HrEmployeeRole:StatusRequired")]
    [Display(Name = "HrEmployeeRole:Status")]
    public HrEmployeeRoleStatus Status { get; set; }
}

