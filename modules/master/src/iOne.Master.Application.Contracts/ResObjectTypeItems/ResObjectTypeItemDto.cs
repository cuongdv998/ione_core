using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResObjectTypeItems;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResObjectTypeItems;

public class ResObjectTypeItemDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ResObjectTypeItem:ObjectTypeId")]
    public Guid ObjectTypeId { get; set; }

    [Display(Name = "ResObjectTypeItem:ObjectItemType")]
    public Guid? ObjectItemType { get; set; }

    [Display(Name = "ResObjectTypeItem:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "ResObjectTypeItem:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "ResObjectTypeItem:UomId")]
    public Guid UomId { get; set; }

    [Display(Name = "ResObjectTypeItem:Description")]
    public string? Description { get; set; }

    [Display(Name = "ResObjectTypeItem:Status")]
    public ResObjectTypeItemStatus Status { get; set; }
}
