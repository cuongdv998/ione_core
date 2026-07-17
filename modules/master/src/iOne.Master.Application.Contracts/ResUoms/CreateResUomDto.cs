using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResUoms;

namespace iOne.Master.ResUoms;

public class CreateResUomDto
{
    [Required(ErrorMessage = "ResUom:ClassIdRequired")]
    [Display(Name = "ResUom:ClassId")]
    public Guid ClassId { get; set; }

    [Required(ErrorMessage = "ResUom:CodeRequired")]
    [StringLength(25, ErrorMessage = "ResUom:CodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "ResUom:CodeInvalid")]
    [Display(Name = "ResUom:Code")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "ResUom:NameRequired")]
    [StringLength(100, ErrorMessage = "ResUom:NameMaxLength")]
    [Display(Name = "ResUom:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "ResUom:StatusRequired")]
    [Display(Name = "ResUom:Status")]
    public ResUomStatus Status { get; set; } = ResUomStatus.Active;

    [Required(ErrorMessage = "ResUom:RoundingRequired")]
    [Display(Name = "ResUom:Rounding")]
    public decimal Rounding { get; set; } = 0.001m;

    [Display(Name = "ResUom:Factor")]
    public decimal? Factor { get; set; } = 0m;

    [Required(ErrorMessage = "ResUom:TypeRequired")]
    [Display(Name = "ResUom:Type")]
    public ResUomType Type { get; set; } = ResUomType.None;
}
