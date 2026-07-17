using iOne.ResBanks;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResBanks;

public class GetResBanksInput : PagedAndSortedResultRequestDto
{
    public string? Code { get; set; }
    
    public string? Name { get; set; }
    
    public ResBankStatus? Status { get; set; }
}

