using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace iOne.Policy.PolicyRiskObjects;

public class PolicyRiskObjectDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "PolicyRiskObject:PolicyId")]
    public Guid PolicyId { get; set; }

    [Display(Name = "PolicyRiskObject:PolicyVersionId")]
    public Guid PolicyVersionId { get; set; }

    [Display(Name = "PolicyRiskObject:ObjectTypeId")]
    public Guid ObjectTypeId { get; set; }

    [Display(Name = "PolicyRiskObject:RepName")]
    public string? RepName { get; set; }

    [Display(Name = "PolicyRiskObject:RepIdNo")]
    public string? RepIdNo { get; set; }

    [Display(Name = "PolicyRiskObject:RepPassport")]
    public string? RepPassport { get; set; }

    [Display(Name = "PolicyRiskObject:RepPhone")]
    public string? RepPhone { get; set; }

    [Display(Name = "PolicyRiskObject:RepEmail")]
    public string? RepEmail { get; set; }

    [Display(Name = "PolicyRiskObject:RepProvinceId")]
    public Guid? RepProvinceId { get; set; }

    [Display(Name = "PolicyRiskObject:RepWardId")]
    public Guid? RepWardId { get; set; }

    [Display(Name = "PolicyRiskObject:RepAddress")]
    public string? RepAddress { get; set; }

    [Display(Name = "PolicyRiskObject:RepFullAddress")]
    public string? RepFullAddress { get; set; }

    [Display(Name = "PolicyRiskObject:RiskObjectProvinceId")]
    public Guid? RiskObjectProvinceId { get; set; }

    [Display(Name = "PolicyRiskObject:RiskObjectWardId")]
    public Guid? RiskObjectWardId { get; set; }

    [Display(Name = "PolicyRiskObject:RiskObjectAddress")]
    public string? RiskObjectAddress { get; set; }

    [Display(Name = "PolicyRiskObject:RiskObjectFullAddress")]
    public string? RiskObjectFullAddress { get; set; }

    [Display(Name = "PolicyRiskObject:RiskObjectLat")]
    public double? RiskObjectLat { get; set; }

    [Display(Name = "PolicyRiskObject:RiskObjectLong")]
    public double? RiskObjectLong { get; set; }
}
