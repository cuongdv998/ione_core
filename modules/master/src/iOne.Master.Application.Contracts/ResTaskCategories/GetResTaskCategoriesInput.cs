using iOne.ResTaskCategories;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResTaskCategories;

public class GetResTaskCategoriesInput : PagedAndSortedResultRequestDto
{
    public string? Code { get; set; }

    public ResTaskCategoryBusinessType? BusinessType { get; set; }

    public string? Name { get; set; }

    public ResTaskCategoryStatus? Status { get; set; }
}
