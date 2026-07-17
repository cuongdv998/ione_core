using iOne.ProCoverageLevelBases;
using Volo.Abp.Application.Dtos;

namespace iOne.Product.ProCoverageLevelBasis;

public class GetProCoverageLevelBasisInput : PagedAndSortedResultRequestDto
{
    public string? Code { get; set; }
    
    public string? Name { get; set; }
    
    public ProCoverageLevelBasisStatus? Status { get; set; }
}
