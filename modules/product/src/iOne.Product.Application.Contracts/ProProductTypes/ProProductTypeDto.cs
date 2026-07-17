using System;
using System.ComponentModel.DataAnnotations;
using iOne.ProProductTypes;
using Volo.Abp.Application.Dtos;

namespace iOne.Product.ProProductTypes;

public class ProProductTypeDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ProProductType:LobId")]
    public Guid LobId { get; set; }

    [Display(Name = "ProProductType:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "ProProductType:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "ProProductType:Description")]
    public string? Description { get; set; }

    [Display(Name = "ProProductType:Status")]
    public ProProductTypeStatus Status { get; set; }
}
