using iOne.ResSequences;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResSequences;

public class GetResSequencesInput : PagedAndSortedResultRequestDto
{
    public string? Code { get; set; }
    
    public string? Name { get; set; }
    
    public ResSequenceType? Type { get; set; }
    
    public ResSequenceStatus? Status { get; set; }
}

