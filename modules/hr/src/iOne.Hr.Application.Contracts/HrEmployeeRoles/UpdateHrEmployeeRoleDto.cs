using System.ComponentModel.DataAnnotations;
using iOne.HrEmployeeRoles;

namespace iOne.Hr.HrEmployeeRoles;

public class UpdateHrEmployeeRoleDto
{
    // ⚠️ QUAN TRỌNG: Không có Code property - Code không được phép sửa

    [Required(ErrorMessage = "HrEmployeeRole:NameRequired")]
    [StringLength(250, ErrorMessage = "HrEmployeeRole:NameMaxLength")]
    [Display(Name = "HrEmployeeRole:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "HrEmployeeRole:StatusRequired")]
    [Display(Name = "HrEmployeeRole:Status")]
    public HrEmployeeRoleStatus Status { get; set; }
}

