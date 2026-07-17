using System;
using iOne.HrEmployees;
using Volo.Abp.Application.Dtos;

namespace iOne.Hr.HrEmployees;

public class GetHrEmployeesInput : PagedAndSortedResultRequestDto
{
    public string? Code { get; set; }
    public string? FullName { get; set; }
    public HrEmployeeStatus? Status { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? OrgId { get; set; }
    public Guid? PartnerId { get; set; }
    public Guid? PositionId { get; set; }
    public Guid? LevelId { get; set; }
    public string? RoleCode { get; set; }
}

