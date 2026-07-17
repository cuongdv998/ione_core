using iOne.ResPaymentMethods;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResPaymentMethods;

public class GetResPaymentMethodsInput : PagedAndSortedResultRequestDto
{
    public string? Code { get; set; }
    
    public string? Name { get; set; }
    
    public ResPaymentMethodStatus? Status { get; set; }
}
