using iOne.ResCountries;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResCountries;

public class GetResCountriesInput : PagedAndSortedResultRequestDto
{
    public string? Code { get; set; }
    
    public string? Name { get; set; }
    
    public ResCountryStatus? Status { get; set; }
}

