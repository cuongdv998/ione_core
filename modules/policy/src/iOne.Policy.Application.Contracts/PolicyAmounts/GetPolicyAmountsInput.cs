using System;
using Volo.Abp.Application.Dtos;

namespace iOne.Policy.PolicyAmounts;

public class GetPolicyAmountsInput : PagedAndSortedResultRequestDto
{
    public Guid? PolicyId { get; set; }
    
    public Guid? PolicyVersionId { get; set; }
    
    public Guid? FeeItemId { get; set; }
    
    public string? PaymentStatus { get; set; }
}
