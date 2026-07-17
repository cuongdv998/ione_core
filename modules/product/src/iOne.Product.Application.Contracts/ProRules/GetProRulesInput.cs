using System;
using iOne.ProRules;
using Volo.Abp.Application.Dtos;

namespace iOne.Product.ProRules;

public class GetProRulesInput : PagedAndSortedResultRequestDto
{
    public string? ApplyTo { get; set; }

    public Guid? ApplyToId { get; set; }

    public Guid? RuleTypeId { get; set; }

    public string? Code { get; set; }

    public string? Name { get; set; }

    public ProRuleStatus? Status { get; set; }
}
