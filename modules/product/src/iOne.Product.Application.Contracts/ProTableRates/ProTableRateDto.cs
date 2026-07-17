using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using iOne.Product.ProTableRateVariables;
using iOne.ProTableRates;
using Volo.Abp.Application.Dtos;

namespace iOne.Product.ProTableRates;

public class ProTableRateDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ProTableRate:LobId")]
    public Guid LobId { get; set; }

    [Display(Name = "ProTableRate:InsurerId")]
    public Guid? InsurerId { get; set; }

    [Display(Name = "ProTableRate:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "ProTableRate:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "ProTableRate:Description")]
    public string? Description { get; set; }

    [Display(Name = "ProTableRate:Status")]
    public ProTableRateStatus Status { get; set; }

    [Display(Name = "ProTableRate:Variables")]
    public List<ProTableRateVariableDto> Variables { get; set; } = new List<ProTableRateVariableDto>();
}
