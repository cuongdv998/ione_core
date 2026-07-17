using System.ComponentModel.DataAnnotations;
using iOne.ResTaxes;

namespace iOne.Product.ResTaxes;

public class UpdateResTaxDto
{
    [Required(ErrorMessage = "ResTax:NameRequired")]
    [StringLength(250, ErrorMessage = "ResTax:NameMaxLength")]
    [Display(Name = "ResTax:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "ResTax:ValueRequired")]
    [Display(Name = "ResTax:Value")]
    public decimal Value { get; set; }

    [Required(ErrorMessage = "ResTax:StatusRequired")]
    [Display(Name = "ResTax:Status")]
    public ResTaxStatus Status { get; set; }

    [StringLength(500, ErrorMessage = "ResTax:DescriptionMaxLength")]
    [Display(Name = "ResTax:Description")]
    public string? Description { get; set; }
}

