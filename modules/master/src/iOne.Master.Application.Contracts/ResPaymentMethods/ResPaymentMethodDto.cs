using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResPaymentMethods;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResPaymentMethods;

public class ResPaymentMethodDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ResPaymentMethod:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "ResPaymentMethod:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "ResPaymentMethod:Status")]
    public ResPaymentMethodStatus Status { get; set; }
}
