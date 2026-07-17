using System;
using System.ComponentModel.DataAnnotations;

namespace iOne.Policy.PolicyRiskObjects;

public class CreatePolicyRiskObjectDto
{
    [Required(ErrorMessage = "PolicyRiskObject:PolicyIdRequired")]
    [Display(Name = "PolicyRiskObject:PolicyId")]
    public Guid PolicyId { get; set; }

    [Required(ErrorMessage = "PolicyRiskObject:PolicyVersionIdRequired")]
    [Display(Name = "PolicyRiskObject:PolicyVersionId")]
    public Guid PolicyVersionId { get; set; }

    [Required(ErrorMessage = "PolicyRiskObject:ObjectTypeIdRequired")]
    [Display(Name = "PolicyRiskObject:ObjectTypeId")]
    public Guid ObjectTypeId { get; set; }

    [StringLength(250, ErrorMessage = "PolicyRiskObject:RepNameMaxLength")]
    [Display(Name = "PolicyRiskObject:RepName")]
    public string? RepName { get; set; }

    [StringLength(15, ErrorMessage = "PolicyRiskObject:RepIdNoMaxLength")]
    [Display(Name = "PolicyRiskObject:RepIdNo")]
    public string? RepIdNo { get; set; }

    [StringLength(15, ErrorMessage = "PolicyRiskObject:RepPassportMaxLength")]
    [Display(Name = "PolicyRiskObject:RepPassport")]
    public string? RepPassport { get; set; }

    [StringLength(15, ErrorMessage = "PolicyRiskObject:RepPhoneMaxLength")]
    [Display(Name = "PolicyRiskObject:RepPhone")]
    public string? RepPhone { get; set; }

    [StringLength(50, ErrorMessage = "PolicyRiskObject:RepEmailMaxLength")]
    [Display(Name = "PolicyRiskObject:RepEmail")]
    public string? RepEmail { get; set; }

    [Display(Name = "PolicyRiskObject:RepProvinceId")]
    public Guid? RepProvinceId { get; set; }

    [Display(Name = "PolicyRiskObject:RepWardId")]
    public Guid? RepWardId { get; set; }

    [StringLength(250, ErrorMessage = "PolicyRiskObject:RepAddressMaxLength")]
    [Display(Name = "PolicyRiskObject:RepAddress")]
    public string? RepAddress { get; set; }

    [StringLength(500, ErrorMessage = "PolicyRiskObject:RepFullAddressMaxLength")]
    [Display(Name = "PolicyRiskObject:RepFullAddress")]
    public string? RepFullAddress { get; set; }

    [Display(Name = "PolicyRiskObject:RiskObjectProvinceId")]
    public Guid? RiskObjectProvinceId { get; set; }

    [Display(Name = "PolicyRiskObject:RiskObjectWardId")]
    public Guid? RiskObjectWardId { get; set; }

    [StringLength(250, ErrorMessage = "PolicyRiskObject:RiskObjectAddressMaxLength")]
    [Display(Name = "PolicyRiskObject:RiskObjectAddress")]
    public string? RiskObjectAddress { get; set; }

    [StringLength(500, ErrorMessage = "PolicyRiskObject:RiskObjectFullAddressMaxLength")]
    [Display(Name = "PolicyRiskObject:RiskObjectFullAddress")]
    public string? RiskObjectFullAddress { get; set; }

    [Display(Name = "PolicyRiskObject:RiskObjectLat")]
    public double? RiskObjectLat { get; set; }

    [Display(Name = "PolicyRiskObject:RiskObjectLong")]
    public double? RiskObjectLong { get; set; }
}
