using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResFeeItems;

namespace iOne.Master.ResFeeItems;

public class CreateResFeeItemDto
{
    [Required(ErrorMessage = "ResFeeItem:CodeRequired")]
    [StringLength(50, ErrorMessage = "ResFeeItem:CodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "ResFeeItem:CodeInvalid")]
    [Display(Name = "ResFeeItem:Code")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "ResFeeItem:NameRequired")]
    [StringLength(250, ErrorMessage = "ResFeeItem:NameMaxLength")]
    [Display(Name = "ResFeeItem:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "ResFeeItem:TaxId")]
    public Guid? TaxId { get; set; }

    [StringLength(500, ErrorMessage = "ResFeeItem:DescriptionMaxLength")]
    [Display(Name = "ResFeeItem:Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "ResFeeItem:StatusRequired")]
    [Display(Name = "ResFeeItem:Status")]
    public ResFeeItemStatus Status { get; set; } = ResFeeItemStatus.Active;
}
