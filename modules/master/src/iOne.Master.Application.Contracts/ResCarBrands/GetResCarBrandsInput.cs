using iOne.ResCarBrands;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResCarBrands;

public class GetResCarBrandsInput : PagedAndSortedResultRequestDto
{
    public string? Code { get; set; }
    
    public string? Name { get; set; }
    
    public ResCarBrandStatus? Status { get; set; }
}



