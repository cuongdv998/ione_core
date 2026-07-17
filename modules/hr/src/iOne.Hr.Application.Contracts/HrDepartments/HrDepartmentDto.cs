using System;
using System.ComponentModel.DataAnnotations;
using iOne.HrDepartments;
using Volo.Abp.Application.Dtos;

namespace iOne.Hr.HrDepartments;

public class HrDepartmentDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "Hr::HrDepartment:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "Hr::HrDepartment:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "Hr::HrDepartment:Description")]
    public string? Description { get; set; }

    [Display(Name = "Hr::HrDepartment:Status")]
    public HrDepartmentStatus Status { get; set; }

    [Display(Name = "Hr::HrDepartment:DeptLevel")]
    public HrDepartmentLevel DeptLevel { get; set; }

    [Display(Name = "Hr::HrDepartment:ParentId")]
    public Guid? ParentId { get; set; }

    [Display(Name = "Hr::HrDepartment:ParentName")]
    public string? ParentName { get; set; }

    [Display(Name = "Hr::HrDepartment:OrgId")]
    public Guid? OrgId { get; set; }

    [Display(Name = "Hr::HrDepartment:OrgName")]
    public string? OrgName { get; set; }

    [Display(Name = "Hr::HrDepartment:TypeId")]
    public Guid? TypeId { get; set; }

    [Display(Name = "Hr::HrDepartment:TypeName")]
    public string? TypeName { get; set; }

    [Display(Name = "Hr::HrDepartment:PartnerId")]
    public Guid PartnerId { get; set; }

    [Display(Name = "Hr::HrDepartment:ProvinceId")]
    public Guid? ProvinceId { get; set; }

    [Display(Name = "Hr::HrDepartment:ProvinceName")]
    public string? ProvinceName { get; set; }

    [Display(Name = "Hr::HrDepartment:WardId")]
    public Guid? WardId { get; set; }

    [Display(Name = "Hr::HrDepartment:WardName")]
    public string? WardName { get; set; }

    [Display(Name = "Hr::HrDepartment:Address")]
    public string? Address { get; set; }

    [Display(Name = "Hr::HrDepartment:FullAddress")]
    public string? FullAddress { get; set; }

    [Display(Name = "Hr::HrDepartment:BankId")]
    public Guid? BankId { get; set; }

    [Display(Name = "Hr::HrDepartment:BankName")]
    public string? BankName { get; set; }

    [Display(Name = "Hr::HrDepartment:BankNo")]
    public string? BankNo { get; set; }
}

