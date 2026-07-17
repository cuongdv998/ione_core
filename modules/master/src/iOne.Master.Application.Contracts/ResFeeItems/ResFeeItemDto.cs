using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResFeeItems;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResFeeItems;

public class ResFeeItemDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ResFeeItem:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "ResFeeItem:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "ResFeeItem:TaxId")]
    public Guid? TaxId { get; set; }

    [Display(Name = "ResFeeItem:Description")]
    public string? Description { get; set; }

    [Display(Name = "ResFeeItem:Status")]
    public ResFeeItemStatus Status { get; set; }
}
