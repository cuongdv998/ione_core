using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace iOne.Policy.PolicyCoverageLevels;

public class PolicyCoverageLevelDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "PolicyCoverageLevel:PolicyCoverageId")]
    public Guid PolicyCoverageId { get; set; }

    [Display(Name = "PolicyCoverageLevel:CoverageLevelTypeId")]
    public Guid CoverageLevelTypeId { get; set; }

    [Display(Name = "PolicyCoverageLevel:CoverageLevelBasisId")]
    public Guid CoverageLevelBasisId { get; set; }

    [Display(Name = "PolicyCoverageLevel:ConditionScript")]
    public string? ConditionScript { get; set; }

    [Display(Name = "PolicyCoverageLevel:ComputeScript")]
    public string? ComputeScript { get; set; }

    [Display(Name = "PolicyCoverageLevel:AmountType")]
    public string AmountType { get; set; } = null!;

    [Display(Name = "PolicyCoverageLevel:FromAmount")]
    public decimal FromAmount { get; set; }

    [Display(Name = "PolicyCoverageLevel:ToAmount")]
    public decimal ToAmount { get; set; }
}
