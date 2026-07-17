using System;
using Volo.Abp.Application.Dtos;

namespace iOne.Policy.PolicyCoverageLevels;

public class GetPolicyCoverageLevelsInput : PagedAndSortedResultRequestDto
{
    public Guid? PolicyCoverageId { get; set; }
    
    public Guid? CoverageLevelTypeId { get; set; }
    
    public Guid? CoverageLevelBasisId { get; set; }
    
    public string? AmountType { get; set; }
}
