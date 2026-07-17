using System;
using System.Collections.Generic;
using iOne.HrDepartments;

namespace iOne.Hr.HrDepartments;

public class HrDepartmentTreeDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public HrDepartmentStatus Status { get; set; }
    public HrDepartmentLevel DeptLevel { get; set; }
    public Guid? ParentId { get; set; }
    public Guid? OrgId { get; set; }
    public string? OrgName { get; set; }
    public Guid? TypeId { get; set; }
    public string? TypeName { get; set; }
    public Guid? ProvinceId { get; set; }
    public string? ProvinceName { get; set; }
    public Guid? WardId { get; set; }
    public string? WardName { get; set; }
    public Guid? BankId { get; set; }
    public string? BankName { get; set; }
    public string? BankNo { get; set; }
    public List<HrDepartmentTreeDto> Children { get; set; } = new List<HrDepartmentTreeDto>();
}

