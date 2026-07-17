using System;
using Volo.Abp.Application.Dtos;

namespace iOne.Policy.PolicyCoverages;

public class GetPolicyCoveragesInput : PagedAndSortedResultRequestDto
{
    public Guid? PolicyProductId { get; set; }
    
    public Guid? CoverageId { get; set; }
    
    public Guid? CoverageParentId { get; set; }
}
