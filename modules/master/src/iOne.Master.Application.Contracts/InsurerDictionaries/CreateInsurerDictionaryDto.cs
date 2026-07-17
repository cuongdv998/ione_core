using System;
using System.ComponentModel.DataAnnotations;
using iOne.InsurerDictionaries;

namespace iOne.Master.InsurerDictionaries;

public class CreateInsurerDictionaryDto
{
    [Required(ErrorMessage = "InsurerDictionary:BusinessNameRequired")]
    [StringLength(50, ErrorMessage = "InsurerDictionary:BusinessNameMaxLength")]
    [Display(Name = "InsurerDictionary:BusinessName")]
    public string BusinessName { get; set; } = null!;

    [Required(ErrorMessage = "InsurerDictionary:InsurerIdRequired")]
    [Display(Name = "InsurerDictionary:InsurerId")]
    public Guid InsurerId { get; set; }

    [Required(ErrorMessage = "InsurerDictionary:OwnCodeRequired")]
    [StringLength(50, ErrorMessage = "InsurerDictionary:OwnCodeMaxLength")]
    [Display(Name = "InsurerDictionary:OwnCode")]
    public string OwnCode { get; set; } = null!;

    [Required(ErrorMessage = "InsurerDictionary:InsurerCodeRequired")]
    [StringLength(50, ErrorMessage = "InsurerDictionary:InsurerCodeMaxLength")]
    [Display(Name = "InsurerDictionary:InsurerCode")]
    public string InsurerCode { get; set; } = null!;

    [Display(Name = "InsurerDictionary:ExtraData")]
    public string? ExtraData { get; set; }

    [Required(ErrorMessage = "InsurerDictionary:StatusRequired")]
    [Display(Name = "InsurerDictionary:Status")]
    public InsurerDictionaryStatus Status { get; set; } = InsurerDictionaryStatus.Active;

    [Required(ErrorMessage = "InsurerDictionary:EffectDateRequired")]
    [Display(Name = "InsurerDictionary:EffectDate")]
    public DateTime EffectDate { get; set; }

    [Display(Name = "InsurerDictionary:ExpireDate")]
    public DateTime? ExpireDate { get; set; }
}
