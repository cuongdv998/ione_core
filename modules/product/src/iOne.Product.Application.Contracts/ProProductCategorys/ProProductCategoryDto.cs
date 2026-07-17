using System;
using System.ComponentModel.DataAnnotations;
using iOne.ProProductCategorys;
using Volo.Abp.Application.Dtos;

namespace iOne.Product.ProProductCategorys;

public class ProProductCategoryDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ProProductCategory:LobId")]
    public Guid LobId { get; set; }

    [Display(Name = "ProProductCategory:ParentId")]
    public Guid? ParentId { get; set; }

    [Display(Name = "ProProductCategory:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "ProProductCategory:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "ProProductCategory:Description")]
    public string? Description { get; set; }

    [Display(Name = "ProProductCategory:Status")]
    public ProProductCategoryStatus Status { get; set; }
}
