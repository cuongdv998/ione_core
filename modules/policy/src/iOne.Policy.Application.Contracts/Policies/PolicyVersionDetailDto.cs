using System;
using System.ComponentModel.DataAnnotations;

namespace iOne.Policy.Policies;

public class PolicyVersionDetailDto
{
    public decimal Version { get; set; }

    public DateTime EffectDate { get; set; }
    public DateTime ExpireDate { get; set; }
    public DateTime OrgEffectDate { get; set; }
    public DateTime OrgExpireDate { get; set; }

    [StringLength(500)]
    public string? InternalNote { get; set; }

    [StringLength(500)]
    public string? CustomerNote { get; set; }

    public decimal PremiumTotal { get; set; }
    public decimal Premium { get; set; }
    public decimal Vat { get; set; }

    public decimal? Discount { get; set; }
    public decimal? DiscountRate { get; set; }

    public decimal? Markup { get; set; }

    public Guid? EndorsementType { get; set; }
    public Guid? EndorsementReasonId { get; set; }

    [StringLength(300)]
    public string? EndorsementDescription { get; set; }

    /// <summary>
    /// Total refund amount (negative value) for termination. Stored when user confirms termination.
    /// </summary>
    public decimal? RefundAmount { get; set; }
}

