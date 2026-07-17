using System;
using System.ComponentModel.DataAnnotations;
using iOne.ProProductTypes;

namespace iOne.Product.ProProductTypes;

public class CreateProProductTypeDto
{
    [Required(ErrorMessage = "ProProductType:LobIdRequired")]
    [Display(Name = "ProProductType:LobId")]
    public Guid LobId { get; set; }

    [Required(ErrorMessage = "ProProductType:CodeRequired")]
    [StringLength(50, ErrorMessage = "ProProductType:CodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "ProProductType:CodeInvalidFormat")]
    [Display(Name = "ProProductType:Code")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "ProProductType:NameRequired")]
    [StringLength(250, ErrorMessage = "ProProductType:NameMaxLength")]
    [Display(Name = "ProProductType:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "ProProductType:StatusRequired")]
    [Display(Name = "ProProductType:Status")]
    public ProProductTypeStatus Status { get; set; }

    [StringLength(500, ErrorMessage = "ProProductType:DescriptionMaxLength")]
    [Display(Name = "ProProductType:Description")]
    public string? Description { get; set; }
}
