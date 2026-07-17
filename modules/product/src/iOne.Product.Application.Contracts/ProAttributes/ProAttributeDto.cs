using System;
using System.ComponentModel.DataAnnotations;
using iOne.ProAttributes;
using Volo.Abp.Application.Dtos;

namespace iOne.Product.ProAttributes;

public class ProAttributeDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ProAttribute:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "ProAttribute:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "ProAttribute:Status")]
    public ProAttributeStatus Status { get; set; }

    [Display(Name = "ProAttribute:Spec")]
    public ProAttributeSpec Spec { get; set; }

    [Display(Name = "ProAttribute:Description")]
    public string? Description { get; set; }

    [Display(Name = "ProAttribute:DataPath")]
    public string? DataPath { get; set; }

    [Display(Name = "ProAttribute:DataType")]
    public ProAttributeDataType DataType { get; set; }

    [Display(Name = "ProAttribute:ComputeScript")]
    public string? ComputeScript { get; set; }

    [Display(Name = "ProAttribute:ClearDataScript")]
    public string? ClearDataScript { get; set; }

    [Display(Name = "ProAttribute:CreatorName")]
    public string? CreatorName { get; set; }
}
