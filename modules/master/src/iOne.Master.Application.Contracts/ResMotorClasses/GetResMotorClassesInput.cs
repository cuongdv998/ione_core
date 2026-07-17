using iOne.ResMotorClasses;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResMotorClasses;

public class GetResMotorClassesInput : PagedAndSortedResultRequestDto
{
    public string? Code { get; set; }
    
    public string? Name { get; set; }
    
    public ResMotorClassStatus? Status { get; set; }
}

