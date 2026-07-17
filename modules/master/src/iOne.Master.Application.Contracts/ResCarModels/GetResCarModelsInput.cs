using System;
using iOne.ResCarBrands;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResCarModels;

public class GetResCarModelsInput : PagedAndSortedResultRequestDto
{
    public Guid? CarBrandId { get; set; }
    
    public string? Code { get; set; }
    
    public string? Name { get; set; }
    
    public ResCarBrandStatus? Status { get; set; }
}



