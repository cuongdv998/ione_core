using System.ComponentModel.DataAnnotations;
using iOne.ResUoms;

namespace iOne.Master.ResUoms;

public class UpdateResUomDto
{
    // ⚠️ QUAN TRỌNG: Không có Code và ClassId property - Code và ClassId không được phép sửa

    [Required(ErrorMessage = "ResUom:NameRequired")]
    [StringLength(100, ErrorMessage = "ResUom:NameMaxLength")]
    [Display(Name = "ResUom:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "ResUom:StatusRequired")]
    [Display(Name = "ResUom:Status")]
    public ResUomStatus Status { get; set; }

    [Required(ErrorMessage = "ResUom:RoundingRequired")]
    [Display(Name = "ResUom:Rounding")]
    public decimal Rounding { get; set; }

    [Range(0, 0.999999, ErrorMessage = "ResUom:FactorRange")]
    [Display(Name = "ResUom:Factor")]
    public decimal? Factor { get; set; }

    [Required(ErrorMessage = "ResUom:TypeRequired")]
    [Display(Name = "ResUom:Type")]
    public ResUomType Type { get; set; }
}
