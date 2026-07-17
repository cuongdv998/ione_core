using iOne.HrEmployeePositions;
using Volo.Abp.Application.Dtos;

namespace iOne.Hr.HrEmployeePositions;

public class GetHrEmployeePositionsInput : PagedAndSortedResultRequestDto
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public HrEmployeePositionType? Type { get; set; }
    public HrEmployeePositionStatus? Status { get; set; }
}

