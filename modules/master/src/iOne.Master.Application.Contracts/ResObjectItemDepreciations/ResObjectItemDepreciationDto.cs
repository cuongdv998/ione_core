using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResObjectItemDepreciations;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResObjectItemDepreciations;

public class ResObjectItemDepreciationDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ResObjectItemDepreciation:ObjectTypeItemId")]
    public Guid ObjectTypeItemId { get; set; }

    [Display(Name = "ResObjectItemDepreciation:CarGroupId")]
    public Guid CarGroupId { get; set; }

    [Display(Name = "ResObjectItemDepreciation:CarGroupCode")]
    public string? CarGroupCode { get; set; }

    [Display(Name = "ResObjectItemDepreciation:CarGroupName")]
    public string? CarGroupName { get; set; }

    [Display(Name = "ResObjectItemDepreciation:UsedTimeFrom")]
    public double UsedTimeFrom { get; set; }

    [Display(Name = "ResObjectItemDepreciation:UsedTimeTo")]
    public double UsedTimeTo { get; set; }

    [Display(Name = "ResObjectItemDepreciation:DepreciationPercent")]
    public double DepreciationPercent { get; set; }

    [Display(Name = "ResObjectItemDepreciation:EffectDate")]
    public DateTime EffectDate { get; set; }

    [Display(Name = "ResObjectItemDepreciation:ExpireDate")]
    public DateTime? ExpireDate { get; set; }

    [Display(Name = "ResObjectItemDepreciation:Status")]
    public ResObjectItemDepreciationStatus Status { get; set; }
}
