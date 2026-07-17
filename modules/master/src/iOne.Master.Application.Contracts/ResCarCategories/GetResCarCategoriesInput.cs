using System;
using iOne.ResCarCategories;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResCarCategories;

public class GetResCarCategoriesInput : PagedAndSortedResultRequestDto
{
    public Guid? CarBrandId { get; set; }
    
    public Guid? CarModelId { get; set; }
    
    public string? Code { get; set; }
    
    public string? Name { get; set; }
    
    public ResCarCategoryStatus? Status { get; set; }
}

