using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResObjectItemDepreciations;

namespace iOne.Master.ResObjectItemDepreciations;

public class CreateResObjectItemDepreciationDto
{
    [Required(ErrorMessage = "ResObjectItemDepreciation:ObjectTypeItemIdRequired")]
    [Display(Name = "ResObjectItemDepreciation:ObjectTypeItemId")]
    public Guid ObjectTypeItemId { get; set; }

    [Required(ErrorMessage = "ResObjectItemDepreciation:CarGroupIdRequired")]
    [Display(Name = "ResObjectItemDepreciation:CarGroupId")]
    public Guid CarGroupId { get; set; }

    [Required(ErrorMessage = "ResObjectItemDepreciation:UsedTimeFromRequired")]
    [Range(0, double.MaxValue, ErrorMessage = "ResObjectItemDepreciation:UsedTimeFromRange")]
    [Display(Name = "ResObjectItemDepreciation:UsedTimeFrom")]
    public double UsedTimeFrom { get; set; }

    [Required(ErrorMessage = "ResObjectItemDepreciation:UsedTimeToRequired")]
    [Range(0, double.MaxValue, ErrorMessage = "ResObjectItemDepreciation:UsedTimeToRange")]
    [Display(Name = "ResObjectItemDepreciation:UsedTimeTo")]
    public double UsedTimeTo { get; set; }

    [Required(ErrorMessage = "ResObjectItemDepreciation:DepreciationPercentRequired")]
    [Range(0, 100, ErrorMessage = "ResObjectItemDepreciation:DepreciationPercentRange")]
    [Display(Name = "ResObjectItemDepreciation:DepreciationPercent")]
    public double DepreciationPercent { get; set; }

    [Required(ErrorMessage = "ResObjectItemDepreciation:EffectDateRequired")]
    [Display(Name = "ResObjectItemDepreciation:EffectDate")]
    public DateTime EffectDate { get; set; }

    [Display(Name = "ResObjectItemDepreciation:ExpireDate")]
    public DateTime? ExpireDate { get; set; }

    [Required(ErrorMessage = "ResObjectItemDepreciation:StatusRequired")]
    [Display(Name = "ResObjectItemDepreciation:Status")]
    public ResObjectItemDepreciationStatus Status { get; set; } = ResObjectItemDepreciationStatus.Active;
}
