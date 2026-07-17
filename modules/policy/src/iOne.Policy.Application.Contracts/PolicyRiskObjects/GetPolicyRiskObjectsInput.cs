using System;
using Volo.Abp.Application.Dtos;

namespace iOne.Policy.PolicyRiskObjects;

public class GetPolicyRiskObjectsInput : PagedAndSortedResultRequestDto
{
    public Guid? PolicyId { get; set; }
    
    public Guid? PolicyVersionId { get; set; }
    
    public Guid? ObjectTypeId { get; set; }
}
