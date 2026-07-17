using System;
using System.ComponentModel.DataAnnotations;
using iOne.InsurerDictionaries;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.InsurerDictionaries;

public class InsurerDictionaryDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "InsurerDictionary:BusinessName")]
    public string BusinessName { get; set; } = null!;

    [Display(Name = "InsurerDictionary:InsurerId")]
    public Guid InsurerId { get; set; }

    [Display(Name = "InsurerDictionary:OwnCode")]
    public string OwnCode { get; set; } = null!;

    [Display(Name = "InsurerDictionary:InsurerCode")]
    public string InsurerCode { get; set; } = null!;

    [Display(Name = "InsurerDictionary:ExtraData")]
    public string? ExtraData { get; set; }

    [Display(Name = "InsurerDictionary:Status")]
    public InsurerDictionaryStatus Status { get; set; }

    [Display(Name = "InsurerDictionary:EffectDate")]
    public DateTime EffectDate { get; set; }

    [Display(Name = "InsurerDictionary:ExpireDate")]
    public DateTime? ExpireDate { get; set; }
}
