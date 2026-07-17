using System;
using System.ComponentModel.DataAnnotations;
using iOne.InsurerDictionaries;

namespace iOne.Master.InsurerDictionaries;

/// <summary>
/// Only InsurerCode, ExtraData, Status, EffectDate, ExpireDate are updatable.
/// BusinessName, InsurerId, OwnCode are not allowed to be changed.
/// </summary>
public class UpdateInsurerDictionaryDto
{
    [Required(ErrorMessage = "InsurerDictionary:InsurerCodeRequired")]
    [StringLength(50, ErrorMessage = "InsurerDictionary:InsurerCodeMaxLength")]
    [Display(Name = "InsurerDictionary:InsurerCode")]
    public string InsurerCode { get; set; } = null!;

    [Display(Name = "InsurerDictionary:ExtraData")]
    public string? ExtraData { get; set; }

    [Required(ErrorMessage = "InsurerDictionary:StatusRequired")]
    [Display(Name = "InsurerDictionary:Status")]
    public InsurerDictionaryStatus Status { get; set; }

    [Required(ErrorMessage = "InsurerDictionary:EffectDateRequired")]
    [Display(Name = "InsurerDictionary:EffectDate")]
    public DateTime EffectDate { get; set; }

    [Display(Name = "InsurerDictionary:ExpireDate")]
    public DateTime? ExpireDate { get; set; }
}
