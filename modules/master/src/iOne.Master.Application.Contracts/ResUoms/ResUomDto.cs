using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResUoms;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResUoms;

public class ResUomDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ResUom:ClassId")]
    public Guid ClassId { get; set; }

    [Display(Name = "ResUom:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "ResUom:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "ResUom:Status")]
    public ResUomStatus Status { get; set; }

    [Display(Name = "ResUom:Rounding")]
    public decimal Rounding { get; set; }

    [Display(Name = "ResUom:Factor")]
    public decimal? Factor { get; set; }

    [Display(Name = "ResUom:Type")]
    public ResUomType Type { get; set; }
}
