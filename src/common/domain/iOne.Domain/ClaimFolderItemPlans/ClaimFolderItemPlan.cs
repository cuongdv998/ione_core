using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using iOne.ClaimFolderItems;
using iOne.ResPartners;
using iOne.HrEmployees;
using Volo.Abp.Domain.Entities.Auditing;
using iOne.ResClaimPlans;

namespace iOne.ClaimFolderItemPlans;

[Table("claim_folder_item_plan")]
public class ClaimFolderItemPlan : FullAuditedAggregateRoot<Guid>
{
    [Required]
    public virtual Guid ClaimFolderItemId { get; private set; }

    [Required]
    public virtual Guid ClaimPlanId { get; private set; }

    /// <summary>
    /// Loại chi phí trong phương án sửa chữa: man_cost | paint_cost | material_cost.
    /// Null: bản ghi tạo từ giám định chi tiết (chưa tách dòng).
    /// </summary>
    [MaxLength(32)]
    public virtual string? PlanType { get; private set; }

    public virtual Guid? PartnerId { get; private set; }

    public virtual Guid? AdjustorId { get; private set; }

    /// <summary>Giá trị tổng hợp / legacy.</summary>
    public virtual decimal? Amount { get; private set; }

    /// <summary>Báo giá garage (số nguyên dương).</summary>
    public virtual decimal? PartnerAmount { get; private set; }

    /// <summary>GĐV đề xuất (số nguyên dương).</summary>
    public virtual decimal? AdjusterAmount { get; private set; }

    /// <summary>Giá duyệt = Thành tiền − Tiền khấu hao (làm tròn nguyên).</summary>
    public virtual decimal? ApprovedAmount { get; private set; }

    public virtual decimal? DiscountPercent { get; private set; }

    public virtual decimal? DiscountAmount { get; private set; }

    /// <summary>Thành tiền = GĐV đề xuất − Tiền giảm giá.</summary>
    public virtual decimal? TotalAmount { get; private set; }

    /// <summary>% khấu hao (chỉ material_cost; đồng bộ cấu hình).</summary>
    public virtual decimal? DepreciationPercent { get; private set; }

    public virtual decimal? DepreciationAmount { get; private set; }

    public virtual decimal? PredictAmountMin { get; private set; }

    public virtual decimal? PredictAmountMax { get; private set; }

    public virtual decimal? PredictAmountMedium { get; private set; }

    /// <summary>Y/N - có cảnh báo hay không.</summary>
    [MaxLength(1)]
    public virtual string? IsWarning { get; private set; }

    // Navigation
    public virtual ClaimFolderItem? ClaimFolderItem { get; set; }
    public virtual ResClaimPlan? ClaimPlan { get; set; }
    public virtual ResPartner? Partner { get; set; }
    public virtual HrEmployee? Adjustor { get; set; }

    protected ClaimFolderItemPlan()
    {
    }

    /// <summary>Khởi tạo tối thiểu (giám định chi tiết).</summary>
    public ClaimFolderItemPlan(
        Guid id,
        Guid claimFolderItemId,
        Guid claimPlanId,
        Guid? partnerId = null,
        Guid? adjustorId = null,
        decimal? amount = null,
        decimal? predictAmountMin = null,
        decimal? predictAmountMax = null,
        decimal? predictAmountMedium = null,
        string? isWarning = null)
        : base(id)
    {
        SetClaimFolderItemId(claimFolderItemId);
        SetClaimPlanId(claimPlanId);
        PlanType = null;
        PartnerId = partnerId;
        AdjustorId = adjustorId;
        Amount = amount;
        PartnerAmount = null;
        AdjusterAmount = null;
        ApprovedAmount = null;
        DiscountPercent = null;
        DiscountAmount = null;
        TotalAmount = null;
        DepreciationPercent = null;
        DepreciationAmount = null;
        PredictAmountMin = predictAmountMin;
        PredictAmountMax = predictAmountMax;
        PredictAmountMedium = predictAmountMedium;
        SetIsWarning(isWarning);
    }

    /// <summary>Dòng chi phí PASC (repair plan).</summary>
    public ClaimFolderItemPlan(
        Guid id,
        Guid claimFolderItemId,
        Guid claimPlanId,
        string planType,
        Guid? partnerId,
        decimal? partnerAmount,
        decimal? adjusterAmount,
        decimal? discountPercent,
        decimal? discountAmount,
        decimal totalAmount,
        decimal? depreciationPercent,
        decimal depreciationAmount,
        decimal approvedAmount,
        Guid? adjustorId = null,
        decimal? predictAmountMin = null,
        decimal? predictAmountMax = null,
        decimal? predictAmountMedium = null,
        string? isWarning = null)
        : base(id)
    {
        SetClaimFolderItemId(claimFolderItemId);
        SetClaimPlanId(claimPlanId);
        SetPlanType(planType);
        PartnerId = partnerId;
        AdjustorId = adjustorId;
        Amount = null;
        PartnerAmount = partnerAmount;
        AdjusterAmount = adjusterAmount;
        ApprovedAmount = RoundMoney(approvedAmount);
        DiscountPercent = discountPercent;
        DiscountAmount = discountAmount;
        TotalAmount = RoundMoney(totalAmount);
        DepreciationPercent = depreciationPercent;
        DepreciationAmount = RoundMoney(depreciationAmount);
        PredictAmountMin = predictAmountMin;
        PredictAmountMax = predictAmountMax;
        PredictAmountMedium = predictAmountMedium;
        SetIsWarning(isWarning);
    }

    private static decimal RoundMoney(decimal v) =>
        Math.Round(v, 0, MidpointRounding.AwayFromZero);

    private void SetClaimFolderItemId(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("ClaimFolderItemId cannot be empty.", nameof(id));
        }
        ClaimFolderItemId = id;
    }

    private void SetClaimPlanId(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("ClaimPlanId cannot be empty.", nameof(id));
        }
        ClaimPlanId = id;
    }

    private void SetPlanType(string planType)
    {
        if (string.IsNullOrWhiteSpace(planType))
        {
            throw new ArgumentException("PlanType is required for repair plan lines.", nameof(planType));
        }
        if (planType.Length > 32)
        {
            throw new ArgumentException("PlanType exceeds max length.", nameof(planType));
        }
        PlanType = planType;
    }

    private void SetIsWarning(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            IsWarning = null;
            return;
        }
        if (value.Length != 1 || (value != "Y" && value != "N"))
        {
            throw new ArgumentException("IsWarning must be 'Y' or 'N'.", nameof(value));
        }
        IsWarning = value;
    }
}
