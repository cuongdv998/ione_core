using System;
using System.Collections.Generic;

namespace iOne.ClaimFolderQuotationApprovals;

/// <summary>
/// Map enum ↔ giá trị cột status (VARCHAR) trong database.
/// </summary>
public static class ClaimFolderQuotationApprovalStatusDatabase
{
    private static readonly Dictionary<ClaimFolderQuotationApprovalStatus, string> ToDatabase = new()
    {
        [ClaimFolderQuotationApprovalStatus.New] = "new",
        [ClaimFolderQuotationApprovalStatus.InProgress] = "inprogress",
        [ClaimFolderQuotationApprovalStatus.Pending_Approval] = "pending_approval",
        [ClaimFolderQuotationApprovalStatus.Approved] = "approved",
        [ClaimFolderQuotationApprovalStatus.Rejected] = "rejected",
        [ClaimFolderQuotationApprovalStatus.Done] = "done",
        [ClaimFolderQuotationApprovalStatus.Cancelled] = "cancelled"
    };

    private static readonly Dictionary<string, ClaimFolderQuotationApprovalStatus> FromDatabase =
        new(StringComparer.OrdinalIgnoreCase);

    static ClaimFolderQuotationApprovalStatusDatabase()
    {
        foreach (var kv in ToDatabase)
        {
            FromDatabase[kv.Value] = kv.Key;
        }
    }

    public static string ToColumnValue(ClaimFolderQuotationApprovalStatus status) =>
        ToDatabase[status];

    public static ClaimFolderQuotationApprovalStatus FromColumnValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !FromDatabase.TryGetValue(value.Trim(), out var status))
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, "Invalid quotation approval status.");
        }

        return status;
    }
}
