using iOne.HrEmployeeRoles;
using Volo.Abp.Application.Dtos;

namespace iOne.Hr.HrEmployeeRoles;

public class GetHrEmployeeRolesInput : PagedAndSortedResultRequestDto
{
    public string? Code { get; set; }
    
    public string? Name { get; set; }
    
    public HrEmployeeRoleStatus? Status { get; set; }
}

