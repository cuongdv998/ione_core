using iOne.ResCurrencies;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResCurrencies;

public class GetResCurrenciesInput : PagedAndSortedResultRequestDto
{
    public string? Code { get; set; }
    
    public string? Name { get; set; }
    
    public ResCurrencyStatus? Status { get; set; }
}
