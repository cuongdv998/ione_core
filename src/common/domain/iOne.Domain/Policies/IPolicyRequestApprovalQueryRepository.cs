using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iOne.Policies;

/// <summary>
/// Provides a queryable of policies that the current user can approve (via WorkTask assignment).
/// Visibility: only work tasks where AssigneeId equals the current user's employee id.
/// </summary>
public interface IPolicyRequestApprovalQueryRepository
{
    /// <summary>
    /// Returns policies that the current user can approve: WorkTask with the given businessCode, business_name = policy,
    /// and AssigneeId = currentEmployeeId.
    /// </summary>
    /// <param name="currentEmployeeId">The current user's HrEmployee.Id.</param>
    /// <param name="businessCode">e.g. CREATE_POLICY_APPROVAL or TERMINATE_POLICY_APPROVAL.</param>
    Task<IQueryable<Policy>> GetApprovalListQueryableAsync(Guid currentEmployeeId, string businessCode);

    /// <summary>
    /// Returns policy + business code + work task fields for policies that the current user can approve (AssigneeId = currentEmployeeId).
    /// When businessCodes is non-null and non-empty, filters by those codes; when null or empty, does not filter by business code.
    /// </summary>
    Task<IQueryable<PolicyApprovalListRow>> GetApprovalListWithCodeQueryableAsync(
        Guid currentEmployeeId,
        IReadOnlyList<string>? businessCodes);
}
