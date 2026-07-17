using iOne.ProAttributes;
using Volo.Abp.Application.Dtos;

namespace iOne.Product.ProAttributes;

public class GetProAttributesInput : PagedAndSortedResultRequestDto
{
    public string? Code { get; set; }

    public string? Name { get; set; }

    public ProAttributeStatus? Status { get; set; }
}
