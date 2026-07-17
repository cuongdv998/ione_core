using iOne.HrEmployeeLevels;
using Volo.Abp.Application.Dtos;

namespace iOne.Hr.HrEmployeeLevels;

public class GetHrEmployeeLevelsInput : PagedAndSortedResultRequestDto
{
    public string? Code { get; set; }
    
    public string? Name { get; set; }
    
    public HrEmployeeLevelStatus? Status { get; set; }
}

