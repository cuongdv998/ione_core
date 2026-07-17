using iOne.ResTaxes;
using Volo.Abp.Application.Dtos;

namespace iOne.Product.ResTaxes;

public class GetResTaxesInput : PagedAndSortedResultRequestDto
{
    public string? Code { get; set; }

    public string? Name { get; set; }

    public ResTaxStatus? Status { get; set; }
}

