using iOne.ProCoverageLevelTypes;
using Volo.Abp.Application.Dtos;

namespace iOne.Product.ProCoverageLevelTypes;

public class GetProCoverageLevelTypesInput : PagedAndSortedResultRequestDto
{
    public string? Code { get; set; }
    
    public string? Name { get; set; }
    
    public ProCoverageLevelTypeStatus? Status { get; set; }
}
