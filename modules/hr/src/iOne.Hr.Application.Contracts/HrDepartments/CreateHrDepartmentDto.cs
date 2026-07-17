using System;
using System.ComponentModel.DataAnnotations;
using iOne.HrDepartments;

namespace iOne.Hr.HrDepartments;

public class CreateHrDepartmentDto
{
    [Required(ErrorMessage = "Hr::HrDepartment:CodeRequired")]
    [StringLength(25, ErrorMessage = "Hr::HrDepartment:CodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "Hr::HrDepartment:CodeInvalid")]
    [Display(Name = "Hr::HrDepartment:Code")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "Hr::HrDepartment:NameRequired")]
    [StringLength(250, ErrorMessage = "Hr::HrDepartment:NameMaxLength")]
    [Display(Name = "Hr::HrDepartment:Name")]
    public string Name { get; set; } = null!;

    [StringLength(500, ErrorMessage = "Hr::HrDepartment:DescriptionMaxLength")]
    [Display(Name = "Hr::HrDepartment:Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Hr::HrDepartment:StatusRequired")]
    [Display(Name = "Hr::HrDepartment:Status")]
    public HrDepartmentStatus Status { get; set; } = HrDepartmentStatus.Active;

    [Required(ErrorMessage = "Hr::HrDepartment:DeptLevelRequired")]
    [Display(Name = "Hr::HrDepartment:DeptLevel")]
    public HrDepartmentLevel DeptLevel { get; set; } = HrDepartmentLevel.Dept;

    [Display(Name = "Hr::HrDepartment:ParentId")]
    public Guid? ParentId { get; set; }

    [Display(Name = "Hr::HrDepartment:OrgId")]
    public Guid? OrgId { get; set; }

    [Display(Name = "Hr::HrDepartment:TypeId")]
    public Guid? TypeId { get; set; }

    [Display(Name = "Hr::HrDepartment:ProvinceId")]
    public Guid? ProvinceId { get; set; }

    [Display(Name = "Hr::HrDepartment:WardId")]
    public Guid? WardId { get; set; }

    [StringLength(500, ErrorMessage = "Hr::HrDepartment:AddressMaxLength")]
    [Display(Name = "Hr::HrDepartment:Address")]
    public string? Address { get; set; }

    [StringLength(500, ErrorMessage = "Hr::HrDepartment:FullAddressMaxLength")]
    [Display(Name = "Hr::HrDepartment:FullAddress")]
    public string? FullAddress { get; set; }

    [Display(Name = "Hr::HrDepartment:BankId")]
    public Guid? BankId { get; set; }

    [StringLength(50, ErrorMessage = "Hr::HrDepartment:BankNoMaxLength")]
    [Display(Name = "Hr::HrDepartment:BankNo")]
    public string? BankNo { get; set; }
}

