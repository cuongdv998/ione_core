using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace iOne.Product.ProProducts;

public class ProProductTableRateDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ProProductTableRate:ProductId")]
    public Guid ProductId { get; set; }

    [Display(Name = "ProProductTableRate:TableRateId")]
    public Guid TableRateId { get; set; }

    [Display(Name = "ProProductTableRate:EffectDate")]
    public DateTime EffectDate { get; set; }

    [Display(Name = "ProProductTableRate:ExpireDate")]
    public DateTime? ExpireDate { get; set; }

    [Display(Name = "ProProductTableRate:CreatorName")]
    public string? CreatorName { get; set; }
}
