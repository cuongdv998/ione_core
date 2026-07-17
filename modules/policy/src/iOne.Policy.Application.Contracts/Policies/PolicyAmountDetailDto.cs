namespace iOne.Policy.Policies;

public class PolicyAmountDetailDto
{
    public decimal PremiumTotal { get; set; }
    public decimal Premium { get; set; }
    public decimal Vat { get; set; }
    public decimal? Discount { get; set; }
    public decimal? DiscountRate { get; set; }

    /// <summary>Effective fee increase (markup), same aggregation as PolicyAmount save.</summary>
    public decimal? Markup { get; set; }

    /// <summary>
    /// Tổng tăng/giảm phí (endorsement) từ policy_amount (fee item ENDORSEMENT_ADJUSTMENT_AMOUNT); có thể âm khi giảm phí. Null khi không phải đơn SĐBS hoặc chưa lưu.
    /// </summary>
    public decimal? EndorsementAdjustmentAmount { get; set; }
}

