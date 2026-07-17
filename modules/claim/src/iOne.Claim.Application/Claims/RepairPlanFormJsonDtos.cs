using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace iOne.Claim.Claims;

/// <summary>
/// Cấu trúc JSON lưu trong ClaimFolderQuotationApproval.Data (đồng bộ với RepairPlanFormData phía Angular).
/// </summary>
internal sealed class RepairPlanFormJsonRoot
{
    [JsonPropertyName("items")]
    public List<RepairPlanFormJsonItem>? Items { get; set; }
}

internal sealed class RepairPlanFormJsonItem
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("claimPlanId")]
    public string? ClaimPlanId { get; set; }

    [JsonPropertyName("costRows")]
    public List<RepairPlanFormJsonCostRow>? CostRows { get; set; }
}

internal sealed class RepairPlanFormJsonCostRow
{
    [JsonPropertyName("costType")]
    public string? CostType { get; set; }

    [JsonPropertyName("garageAmount")]
    public decimal? GarageAmount { get; set; }

    [JsonPropertyName("proposedAmount")]
    public decimal? ProposedAmount { get; set; }

    [JsonPropertyName("discountPercent")]
    public decimal? DiscountPercent { get; set; }

    [JsonPropertyName("discountAmount")]
    public decimal? DiscountAmount { get; set; }

    [JsonPropertyName("depreciationPercent")]
    public decimal? DepreciationPercent { get; set; }
}
