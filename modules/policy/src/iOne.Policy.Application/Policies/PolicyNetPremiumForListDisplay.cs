using System;

namespace iOne.Policy.Policies;

/// <summary>
/// Tổng phí hiển thị trên danh sách (màn tìm kiếm đơn, phê duyệt): net từ <c>policy_version</c> — không âm.
/// </summary>
public static class PolicyNetPremiumForListDisplay
{
    public static decimal Compute(decimal versionPremiumTotal, decimal? versionDiscount, decimal? versionMarkup) =>
        Math.Max(0m, versionPremiumTotal - (versionDiscount ?? 0m) + (versionMarkup ?? 0m));
}
