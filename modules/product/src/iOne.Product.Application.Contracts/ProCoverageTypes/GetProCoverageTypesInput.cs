using iOne.ProCoverageTypes;
using Volo.Abp.Application.Dtos;

namespace iOne.Product.ProCoverageTypes;

public class GetProCoverageTypesInput : PagedAndSortedResultRequestDto
{
    public string? Code { get; set; }

    public string? Name { get; set; }

    public ProCoverageTypeStatus? Status { get; set; }
}

