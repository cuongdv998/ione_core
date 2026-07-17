using System.Collections.Generic;
using iOne.Policy.Policies;

namespace iOne.Policy.PolicyRequestApproval;

/// <summary>
/// Input for listing policies that the current user can approve (create or terminate request approval).
/// Reuses the same filter set as the main policy list. Use BusinessCode(s) to filter by approval type.
/// </summary>
public class GetPolicyRequestApprovalListInput : GetPoliciesInput
{
    /// <summary>
    /// Work task business code, e.g. CREATE_POLICY_APPROVAL or TERMINATE_POLICY_APPROVAL.
    /// Used for backward compatibility when <see cref="BusinessCodes"/> is not set.
    /// </summary>
    public string? BusinessCode { get; set; }

    /// <summary>
    /// When non-null and non-empty: filter by these work task business codes only.
    /// When null or empty: do not filter by business code; all approval types are returned (any future business codes will appear).
    /// If only legacy <see cref="BusinessCode"/> is set, it is treated as a single-element list.
    /// </summary>
    public List<string>? BusinessCodes { get; set; }
}
