using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace iOne.Policy.PolicyVersions;

public class PolicyVersionDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "PolicyVersion:Version")]
    public decimal Version { get; set; }

    [Display(Name = "PolicyVersion:PolicyId")]
    public Guid PolicyId { get; set; }

    [Display(Name = "PolicyVersion:Type")]
    public string Type { get; set; } = null!;

    [Display(Name = "PolicyVersion:Status")]
    public string Status { get; set; } = null!;

    [Display(Name = "PolicyVersion:EffectDate")]
    public DateTime EffectDate { get; set; }

    [Display(Name = "PolicyVersion:ExpireDate")]
    public DateTime ExpireDate { get; set; }

    [Display(Name = "PolicyVersion:OrgEffectDate")]
    public DateTime OrgEffectDate { get; set; }

    [Display(Name = "PolicyVersion:OrgExpireDate")]
    public DateTime OrgExpireDate { get; set; }

    [Display(Name = "PolicyVersion:InternalNote")]
    public string? InternalNote { get; set; }

    [Display(Name = "PolicyVersion:CustomerNote")]
    public string? CustomerNote { get; set; }

    [Display(Name = "PolicyVersion:PremiumTotal")]
    public decimal PremiumTotal { get; set; }

    [Display(Name = "PolicyVersion:Premium")]
    public decimal Premium { get; set; }

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

    [Display(Name = "PolicyVersion:ApprovalStatus")]
    public string? ApprovalStatus { get; set; }

    [Display(Name = "PolicyVersion:InsurerIntegrationStatus")]
    public string? InsurerIntegrationStatus { get; set; }

    [Display(Name = "PolicyVersion:InsurerIntegrationDescription")]
    public string? InsurerIntegrationDescription { get; set; }

    [Display(Name = "PolicyVersion:EndorsementType")]
    public Guid? EndorsementType { get; set; }

    [Display(Name = "PolicyVersion:EndorsementReasonId")]
    public Guid? EndorsementReasonId { get; set; }

    [Display(Name = "PolicyVersion:EndorsementDescription")]
    public string? EndorsementDescription { get; set; }
}
