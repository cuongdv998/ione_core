using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResPaymentTypes;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResPaymentTypes;

public class ResPaymentTypeDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ResPaymentType:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "ResPaymentType:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "ResPaymentType:Description")]
    public string? Description { get; set; }

    [Display(Name = "ResPaymentType:Status")]
    public ResPaymentTypeStatus Status { get; set; }
}
