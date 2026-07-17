using System;
using System.ComponentModel.DataAnnotations;

namespace iOne.Policy.Policies;

public class UpdatePolicyVersionInputDto
{
    // ⚠️ QUAN TRỌNG: Không cho phép update "Version" (số version), "Type", "Status" qua API này.

    [Display(Name = "PolicyVersion:EffectDate")]
    public DateTime? EffectDate { get; set; }

    [Display(Name = "PolicyVersion:ExpireDate")]
    public DateTime? ExpireDate { get; set; }

    [Display(Name = "PolicyVersion:OrgEffectDate")]
    public DateTime? OrgEffectDate { get; set; }

    [Display(Name = "PolicyVersion:OrgExpireDate")]
    public DateTime? OrgExpireDate { get; set; }

    [Display(Name = "PolicyVersion:InternalNote")]
    public string? InternalNote { get; set; }

    [Display(Name = "PolicyVersion:CustomerNote")]
    public string? CustomerNote { get; set; }

    [Display(Name = "PolicyVersion:PremiumTotal")]
    public decimal? PremiumTotal { get; set; }

    [Display(Name = "PolicyVersion:Premium")]
    public decimal? Premium { get; set; }

    [Display(Name = "PolicyVersion:Vat")]
    public decimal? Vat { get; set; }

    [Display(Name = "PolicyVersion:Discount")]
    public decimal? Discount { get; set; }

    [Display(Name = "PolicyVersion:DiscountRate")]
    public decimal? DiscountRate { get; set; }

    [Display(Name = "PolicyVersion:Markup")]
    public decimal? Markup { get; set; }

    [Display(Name = "PolicyVersion:EndorsementType")]
    public Guid? EndorsementType { get; set; }

    [Display(Name = "PolicyVersion:EndorsementReasonId")]
    public Guid? EndorsementReasonId { get; set; }

    [StringLength(300, ErrorMessage = "PolicyVersion:EndorsementDescriptionMaxLength")]
    [Display(Name = "PolicyVersion:EndorsementDescription")]
    public string? EndorsementDescription { get; set; }

    /// <summary>
    /// Endorsement type code (e.g. POLICY_COMMON_INFOR_ENDORSEMENT) for backend branching. Not stored in DB.
    /// </summary>
    public string? EndorsementTypeCode { get; set; }
}

