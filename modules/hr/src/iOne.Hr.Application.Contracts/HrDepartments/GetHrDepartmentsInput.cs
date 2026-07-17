using System;
using iOne.HrDepartments;
using Volo.Abp.Application.Dtos;

namespace iOne.Hr.HrDepartments;

public class GetHrDepartmentsInput : PagedAndSortedResultRequestDto
{
    public string? Code { get; set; }
    
    public string? Name { get; set; }
    
    public HrDepartmentStatus? Status { get; set; }

    public HrDepartmentLevel? DeptLevel { get; set; }

    public Guid? ParentId { get; set; }

    public Guid? OrgId { get; set; }

    public Guid? TypeId { get; set; }

    public Guid? ProvinceId { get; set; }

    public Guid? WardId { get; set; }

    public Guid? BankId { get; set; }
}

