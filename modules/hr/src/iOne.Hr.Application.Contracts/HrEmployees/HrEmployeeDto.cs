using System;
using System.ComponentModel.DataAnnotations;
using iOne.HrEmployees;
using Volo.Abp.Application.Dtos;

namespace iOne.Hr.HrEmployees;

public class HrEmployeeDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "Hr::HrEmployee:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "Hr::HrEmployee:FullName")]
    public string FullName { get; set; } = null!;

    [Display(Name = "Hr::HrEmployee:Status")]
    public HrEmployeeStatus Status { get; set; }

    [Display(Name = "Hr::HrEmployee:PositionId")]
    public Guid? PositionId { get; set; }

    [Display(Name = "Hr::HrEmployee:PositionName")]
    public string? PositionName { get; set; }

    [Display(Name = "Hr::HrEmployee:LevelId")]
    public Guid? LevelId { get; set; }

    [Display(Name = "Hr::HrEmployee:LevelName")]
    public string? LevelName { get; set; }

    [Display(Name = "Hr::HrEmployee:PartnerId")]
    public Guid? PartnerId { get; set; }

    [Display(Name = "Hr::HrEmployee:PartnerName")]
    public string? PartnerName { get; set; }

    [Display(Name = "Hr::HrEmployee:OrgId")]
    public Guid? OrgId { get; set; }

    [Display(Name = "Hr::HrEmployee:OrgName")]
    public string? OrgName { get; set; }

    [Display(Name = "Hr::HrEmployee:DepartmentId")]
    public Guid DepartmentId { get; set; }

    [Display(Name = "Hr::HrEmployee:DepartmentName")]
    public string? DepartmentName { get; set; }

    [Display(Name = "Hr::HrEmployee:IsManager")]
    public bool? IsManager { get; set; }

    [Display(Name = "Hr::HrEmployee:ManagerId")]
    public Guid? ManagerId { get; set; }

    [Display(Name = "Hr::HrEmployee:ManagerName")]
    public string? ManagerName { get; set; }

    [Display(Name = "Hr::HrEmployee:ProvinceId")]
    public Guid? ProvinceId { get; set; }

    [Display(Name = "Hr::HrEmployee:ProvinceName")]
    public string? ProvinceName { get; set; }

    [Display(Name = "Hr::HrEmployee:WardId")]
    public Guid? WardId { get; set; }

    [Display(Name = "Hr::HrEmployee:WardName")]
    public string? WardName { get; set; }

    [Display(Name = "Hr::HrEmployee:Address")]
    public string? Address { get; set; }

    [Display(Name = "Hr::HrEmployee:FullAddress")]
    public string? FullAddress { get; set; }

    [Display(Name = "Hr::HrEmployee:Phone")]
    public string? Phone { get; set; }

    [Display(Name = "Hr::HrEmployee:Email")]
    public string? Email { get; set; }

    [Display(Name = "Hr::HrEmployee:UserId")]
    public Guid? UserId { get; set; }

    [Display(Name = "Hr::HrEmployee:UserName")]
    public string? UserName { get; set; }
}

