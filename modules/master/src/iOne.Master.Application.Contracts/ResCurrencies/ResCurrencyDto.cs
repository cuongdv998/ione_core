using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResCurrencies;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResCurrencies;

public class ResCurrencyDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ResCurrency:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "ResCurrency:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "ResCurrency:Description")]
    public string? Description { get; set; }

    [Display(Name = "ResCurrency:Status")]
    public ResCurrencyStatus Status { get; set; }
}
