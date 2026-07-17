using System;
using System.ComponentModel.DataAnnotations;

namespace iOne.Policy.PolicyVersions;

public class UpdatePolicyVersionDto
{
    [Required(ErrorMessage = "PolicyVersion:VersionRequired")]
    [Display(Name = "PolicyVersion:Version")]
    public decimal Version { get; set; }

    [Required(ErrorMessage = "PolicyVersion:StatusRequired")]
    [StringLength(15, ErrorMessage = "PolicyVersion:StatusMaxLength")]
    [Display(Name = "PolicyVersion:Status")]
    public string Status { get; set; } = null!;

    [Required(ErrorMessage = "PolicyVersion:EffectDateRequired")]
    [Display(Name = "PolicyVersion:EffectDate")]
    public DateTime EffectDate { get; set; }

    [Required(ErrorMessage = "PolicyVersion:ExpireDateRequired")]
    [Display(Name = "PolicyVersion:ExpireDate")]
    public DateTime ExpireDate { get; set; }

    [Required(ErrorMessage = "PolicyVersion:OrgEffectDateRequired")]
    [Display(Name = "PolicyVersion:OrgEffectDate")]
    public DateTime OrgEffectDate { get; set; }

    [Required(ErrorMessage = "PolicyVersion:OrgExpireDateRequired")]
    [Display(Name = "PolicyVersion:OrgExpireDate")]
    public DateTime OrgExpireDate { get; set; }

    [StringLength(500, ErrorMessage = "PolicyVersion:InternalNoteMaxLength")]
    [Display(Name = "PolicyVersion:InternalNote")]
    public string? InternalNote { get; set; }

    [StringLength(500, ErrorMessage = "PolicyVersion:CustomerNoteMaxLength")]
    [Display(Name = "PolicyVersion:CustomerNote")]
    public string? CustomerNote { get; set; }

    [Required(ErrorMessage = "PolicyVersion:PremiumTotalRequired")]
    [Display(Name = "PolicyVersion:PremiumTotal")]
    public decimal PremiumTotal { get; set; }

    [Required(ErrorMessage = "PolicyVersion:PremiumRequired")]
    [Display(Name = "PolicyVersion:Premium")]
    public decimal Premium { get; set; }

    [Required(ErrorMessage = "PolicyVersion:VatRequired")]
    [Display(Name = "PolicyVersion:Vat")]
    public decimal Vat { get; set; }

    [Display(Name = "PolicyVersion:Discount")]
    public decimal? Discount { get; set; }

    [Display(Name = "PolicyVersion:DiscountRate")]
    public decimal? DiscountRate { get; set; }

    [Display(Name = "PolicyVersion:Markup")]
    public decimal? Markup { get; set; }

    [Display(Name = "PolicyVersion:ApprovalDate")]
    public DateTime? ApprovalDate { get; set; }

    [Display(Name = "PolicyVersion:ApproverId")]
    public Guid? ApproverId { get; set; }

    [StringLength(15, ErrorMessage = "PolicyVersion:ApprovalStatusMaxLength")]
    [Display(Name = "PolicyVersion:ApprovalStatus")]
    public string? ApprovalStatus { get; set; }

    [StringLength(15, ErrorMessage = "PolicyVersion:InsurerIntegrationStatusMaxLength")]
    [Display(Name = "PolicyVersion:InsurerIntegrationStatus")]
    public string? InsurerIntegrationStatus { get; set; }

    [Display(Name = "PolicyVersion:InsurerIntegrationDescription")]
    public string? InsurerIntegrationDescription { get; set; }

    [Display(Name = "PolicyVersion:EndorsementType")]
    public Guid? EndorsementType { get; set; }

    [Display(Name = "PolicyVersion:EndorsementReasonId")]
    public Guid? EndorsementReasonId { get; set; }

    [StringLength(300, ErrorMessage = "PolicyVersion:EndorsementDescriptionMaxLength")]
    [Display(Name = "PolicyVersion:EndorsementDescription")]
    public string? EndorsementDescription { get; set; }
}
