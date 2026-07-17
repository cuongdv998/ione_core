using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace iOne.Product.ProTableRateVariables;

public class ProTableRateVariableDto : AuditedEntityDto<Guid>
{
    [Display(Name = "ProTableRateVariable:TableRateId")]
    public Guid TableRateId { get; set; }

    [Display(Name = "ProTableRateVariable:AttributeId")]
    public Guid AttributeId { get; set; }

    [Display(Name = "ProTableRateVariable:Operator")]
    public string Operator { get; set; } = null!;

    [Display(Name = "ProTableRateVariable:AttributeName")]
    public string? AttributeName { get; set; }

    [Display(Name = "ProTableRateVariable:AttributeCreatorName")]
    public string? AttributeCreatorName { get; set; }
}
