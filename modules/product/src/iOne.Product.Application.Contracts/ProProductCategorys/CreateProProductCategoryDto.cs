using System;
using System.ComponentModel.DataAnnotations;
using iOne.ProProductCategorys;

namespace iOne.Product.ProProductCategorys;

public class CreateProProductCategoryDto
{
    [Required(ErrorMessage = "ProProductCategory:LobIdRequired")]
    [Display(Name = "ProProductCategory:LobId")]
    public Guid LobId { get; set; }

    [Display(Name = "ProProductCategory:ParentId")]
    public Guid? ParentId { get; set; }

    [Required(ErrorMessage = "ProProductCategory:CodeRequired")]
    [StringLength(50, ErrorMessage = "ProProductCategory:CodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "ProProductCategory:CodeInvalidFormat")]
    [Display(Name = "ProProductCategory:Code")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "ProProductCategory:NameRequired")]
    [StringLength(250, ErrorMessage = "ProProductCategory:NameMaxLength")]
    [Display(Name = "ProProductCategory:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "ProProductCategory:StatusRequired")]
    [Display(Name = "ProProductCategory:Status")]
    public ProProductCategoryStatus Status { get; set; }

    [StringLength(500, ErrorMessage = "ProProductCategory:DescriptionMaxLength")]
    [Display(Name = "ProProductCategory:Description")]
    public string? Description { get; set; }
}
