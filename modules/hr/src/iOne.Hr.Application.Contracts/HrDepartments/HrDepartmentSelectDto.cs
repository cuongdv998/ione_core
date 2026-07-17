using System;
using System.ComponentModel.DataAnnotations;

namespace iOne.Hr.HrDepartments;

public class HrDepartmentSelectDto
{
    [Display(Name = "Hr::HrDepartment:Id")]
    public Guid Id { get; set; }

    [Display(Name = "Hr::HrDepartment:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "Hr::HrDepartment:Name")]
    public string Name { get; set; } = null!;
}
