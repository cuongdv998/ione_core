using System;
using Volo.Abp.Application.Dtos;

namespace iOne.Policy.PolicyRiskMotors;

public class GetPolicyRiskMotorsInput : PagedAndSortedResultRequestDto
{
    public Guid? PolicyRiskObjectId { get; set; }
    
    public string? CarPlate { get; set; }
    
    public string? CarVin { get; set; }
}
