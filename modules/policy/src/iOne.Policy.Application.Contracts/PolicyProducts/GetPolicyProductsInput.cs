using System;
using Volo.Abp.Application.Dtos;

namespace iOne.Policy.PolicyProducts;

public class GetPolicyProductsInput : PagedAndSortedResultRequestDto
{
    public Guid? PolicyVersionId { get; set; }
    
    public Guid? ProductId { get; set; }
}
