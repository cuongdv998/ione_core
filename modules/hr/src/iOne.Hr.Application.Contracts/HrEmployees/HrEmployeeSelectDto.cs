using System;
using System.ComponentModel.DataAnnotations;

namespace iOne.Hr.HrEmployees;

public class HrEmployeeSelectDto
{
    [Display(Name = "Hr::HrEmployee:Id")]
    public Guid Id { get; set; }

    [Display(Name = "Hr::HrEmployee:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "Hr::HrEmployee:FullName")]
    public string FullName { get; set; } = null!;
}

