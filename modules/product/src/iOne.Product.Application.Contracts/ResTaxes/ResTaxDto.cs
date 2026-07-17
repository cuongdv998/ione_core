using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResTaxes;
using Volo.Abp.Application.Dtos;

namespace iOne.Product.ResTaxes;

public class ResTaxDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ResTax:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "ResTax:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "ResTax:Description")]
    public string? Description { get; set; }

    [Display(Name = "ResTax:Value")]
    public decimal Value { get; set; }

    [Display(Name = "ResTax:Status")]
    public ResTaxStatus Status { get; set; }
}

