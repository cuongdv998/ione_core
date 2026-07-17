using System;
using System.ComponentModel.DataAnnotations;
using iOne.HrEmployees;

namespace iOne.Hr.HrEmployees;

public class UpdateHrEmployeeDto
{
    [Required]
    [Display(Name = "Hr::HrEmployee:FullName")]
    [MaxLength(50)]
    public string FullName { get; set; } = null!;

    [Required]
    [Display(Name = "Hr::HrEmployee:Status")]
    public HrEmployeeStatus Status { get; set; }

    [Display(Name = "Hr::HrEmployee:PositionId")]
    public Guid? PositionId { get; set; }

    [Display(Name = "Hr::HrEmployee:LevelId")]
    public Guid? LevelId { get; set; }

    [Display(Name = "Hr::HrEmployee:PartnerId")]
    public Guid? PartnerId { get; set; }

    [Display(Name = "Hr::HrEmployee:OrgId")]
    public Guid? OrgId { get; set; }

    [Required]
    [Display(Name = "Hr::HrEmployee:DepartmentId")]
    public Guid DepartmentId { get; set; }

    [Display(Name = "Hr::HrEmployee:IsManager")]
    public bool? IsManager { get; set; }

    [Display(Name = "Hr::HrEmployee:ManagerId")]
    public Guid? ManagerId { get; set; }

    [Display(Name = "Hr::HrEmployee:ProvinceId")]
    public Guid? ProvinceId { get; set; }

    [Display(Name = "Hr::HrEmployee:WardId")]
    public Guid? WardId { get; set; }

    [Display(Name = "Hr::HrEmployee:Address")]
    [MaxLength(250)]
    public string? Address { get; set; }

    [Display(Name = "Hr::HrEmployee:FullAddress")]
    [MaxLength(500)]
    public string? FullAddress { get; set; }

    [Display(Name = "Hr::HrEmployee:Phone")]
    [MaxLength(15)]
    public string? Phone { get; set; }

    [Display(Name = "Hr::HrEmployee:Email")]
    [MaxLength(50)]
    public string? Email { get; set; }

    [Display(Name = "Hr::HrEmployee:UserId")]
    public Guid? UserId { get; set; }
}

