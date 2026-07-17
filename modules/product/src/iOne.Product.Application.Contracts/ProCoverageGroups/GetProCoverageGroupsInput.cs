using iOne.ProCoverageGroups;
using Volo.Abp.Application.Dtos;

namespace iOne.Product.ProCoverageGroups;

public class GetProCoverageGroupsInput : PagedAndSortedResultRequestDto
{
    public string? Code { get; set; }
    
    public string? Name { get; set; }
    
    public ProCoverageGroupStatus? Status { get; set; }
}

