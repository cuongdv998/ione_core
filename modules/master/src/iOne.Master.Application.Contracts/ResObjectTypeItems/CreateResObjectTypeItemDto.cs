using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResObjectTypeItems;

namespace iOne.Master.ResObjectTypeItems;

public class CreateResObjectTypeItemDto
{
    [Required(ErrorMessage = "ResObjectTypeItem:ObjectTypeIdRequired")]
    [Display(Name = "ResObjectTypeItem:ObjectTypeId")]
    public Guid ObjectTypeId { get; set; }

    [Display(Name = "ResObjectTypeItem:ObjectItemType")]
    public Guid? ObjectItemType { get; set; }

    [Required(ErrorMessage = "ResObjectTypeItem:CodeRequired")]
    [StringLength(50, ErrorMessage = "ResObjectTypeItem:CodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "ResObjectTypeItem:CodeInvalid")]
    [Display(Name = "ResObjectTypeItem:Code")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "ResObjectTypeItem:NameRequired")]
    [StringLength(250, ErrorMessage = "ResObjectTypeItem:NameMaxLength")]
    [Display(Name = "ResObjectTypeItem:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "ResObjectTypeItem:UomIdRequired")]
    [Display(Name = "ResObjectTypeItem:UomId")]
    public Guid UomId { get; set; }

    [StringLength(500, ErrorMessage = "ResObjectTypeItem:DescriptionMaxLength")]
    [Display(Name = "ResObjectTypeItem:Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "ResObjectTypeItem:StatusRequired")]
    [Display(Name = "ResObjectTypeItem:Status")]
    public ResObjectTypeItemStatus Status { get; set; } = ResObjectTypeItemStatus.Active;
}
