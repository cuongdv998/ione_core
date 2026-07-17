using System;
using System.ComponentModel.DataAnnotations;

namespace iOne.Policy.Policies;

public class UpdatePolicyCoverageLevelInputDto
{
    // policy_coverage_level.id (nullable: null means create new row)
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "PolicyCoverageLevel:CoverageLevelTypeIdRequired")]
    [Display(Name = "PolicyCoverageLevel:CoverageLevelTypeId")]
    public Guid CoverageLevelTypeId { get; set; }

    [Required(ErrorMessage = "PolicyCoverageLevel:CoverageLevelBasisIdRequired")]
    [Display(Name = "PolicyCoverageLevel:CoverageLevelBasisId")]
    public Guid CoverageLevelBasisId { get; set; }

    [Display(Name = "PolicyCoverageLevel:ConditionScript")]
    public string? ConditionScript { get; set; }

    [Display(Name = "PolicyCoverageLevel:ComputeScript")]
    public string? ComputeScript { get; set; }

    [Required(ErrorMessage = "PolicyCoverageLevel:AmountTypeRequired")]
    [StringLength(15, ErrorMessage = "PolicyCoverageLevel:AmountTypeMaxLength")]
    [Display(Name = "PolicyCoverageLevel:AmountType")]
    public string AmountType { get; set; } = null!;

    [Required(ErrorMessage = "PolicyCoverageLevel:FromAmountRequired")]
    [Display(Name = "PolicyCoverageLevel:FromAmount")]
    public decimal FromAmount { get; set; }

    [Required(ErrorMessage = "PolicyCoverageLevel:ToAmountRequired")]
    [Display(Name = "PolicyCoverageLevel:ToAmount")]
    public decimal ToAmount { get; set; }
}

