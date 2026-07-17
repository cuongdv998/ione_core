using System;
using System.ComponentModel.DataAnnotations;

namespace iOne.Product.ProTableRateLines;

public class CreateProTableRateLineDto
{
    [Required(ErrorMessage = "ProTableRateLine:TableRateIdRequired")]
    [Display(Name = "ProTableRateLine:TableRateId")]
    public Guid TableRateId { get; set; }

    [Display(Name = "ProTableRateLine:CoverageId")]
    public Guid? CoverageId { get; set; }

    [Display(Name = "ProTableRateLine:ChannelId")]
    public Guid? ChannelId { get; set; }

    [Display(Name = "ProTableRateLine:PartnerId")]
    public Guid? PartnerId { get; set; }

    [StringLength(250, ErrorMessage = "ProTableRateLine:NameMaxLength")]
    [Display(Name = "ProTableRateLine:Name")]
    public string? Name { get; set; }

    [Required(ErrorMessage = "ProTableRateLine:ConditionRequired")]
    [Display(Name = "ProTableRateLine:Condition")]
    public string Condition { get; set; } = null!;

    [Display(Name = "ProTableRateLine:MinimumRate")]
    public decimal? MinimumRate { get; set; }

    [Display(Name = "ProTableRateLine:BaseRate")]
    public decimal? BaseRate { get; set; }

    [Display(Name = "ProTableRateLine:FlatRate")]
    public decimal? FlatRate { get; set; }

    [Display(Name = "ProTableRateLine:MaxDiscount")]
    public decimal? MaxDiscount { get; set; }

    [Display(Name = "ProTableRateLine:LoadingRate")]
    public decimal? LoadingRate { get; set; }

    [Display(Name = "ProTableRateLine:Loading")]
    public decimal? Loading { get; set; }

    [Required(ErrorMessage = "ProTableRateLine:EffectDateRequired")]
    [Display(Name = "ProTableRateLine:EffectDate")]
    public DateTime EffectDate { get; set; }

    [Display(Name = "ProTableRateLine:ExpireDate")]
    public DateTime? ExpireDate { get; set; }
}
