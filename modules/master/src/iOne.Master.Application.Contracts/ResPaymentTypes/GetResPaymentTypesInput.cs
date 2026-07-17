using iOne.ResPaymentTypes;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResPaymentTypes;

public class GetResPaymentTypesInput : PagedAndSortedResultRequestDto
{
    public string? Code { get; set; }

    public string? Name { get; set; }

    public ResPaymentTypeStatus? Status { get; set; }
}
