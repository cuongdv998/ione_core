using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResFeeItems;

namespace iOne.Master.ResFeeItems;

public class UpdateResFeeItemDto
{
    // ⚠️ QUAN TRỌNG: Không có Code property - Code không được phép sửa

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
    public ResFeeItemStatus Status { get; set; }
}
