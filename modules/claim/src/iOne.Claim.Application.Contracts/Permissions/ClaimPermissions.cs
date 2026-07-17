namespace iOne.Claim.Permissions;

public static class ClaimPermissions
{
    public const string GroupName = "ClaimClaim";

    public const string Default = GroupName;
    public const string Create = Default + ".Create";
    public const string Edit = Default + ".Edit";
    public const string Delete = Default + ".Delete";
    public const string View = Default + ".View";
    /// <summary>Duyệt danh sách phương án sửa chữa (PASC).</summary>
    public const string QuotationApprovalList = Default + ".QuotationApprovalList";
}
