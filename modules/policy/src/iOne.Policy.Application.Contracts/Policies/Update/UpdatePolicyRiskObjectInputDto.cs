using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace iOne.Policy.Policies;

public class UpdatePolicyRiskObjectInputDto
{
    // policy_risk_object.id (nullable: null means create new row)
    public Guid? Id { get; set; }

    // Same shape as create; objectTypeId is required for create-new risk object cases.
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

    /// <summary>
    ///     Documents linked to this risk object (stored in policy_risk_object_document).
    ///     If provided (including empty list), the server will sync the link table accordingly.
    /// </summary>
    public List<UpdatePolicyRiskObjectDocumentInputDto>? Documents { get; set; }

    // Nested motor risk info
    public UpdatePolicyRiskMotorInputDto? RiskObjectMotor { get; set; }
}

public class UpdatePolicyRiskObjectDocumentInputDto
{
    public Guid DocumentId { get; set; }
}