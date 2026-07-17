using iOne.HrDepartmentTypes;
using Volo.Abp.Application.Dtos;

namespace iOne.Hr.HrDepartmentTypes;

public class GetHrDepartmentTypesInput : PagedAndSortedResultRequestDto
{
    public string? Code { get; set; }
    
    public string? Name { get; set; }
    
    public HrDepartmentTypeStatus? Status { get; set; }
}

