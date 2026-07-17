using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.EntityFrameworkCore;
using iOne.EntityFrameworkCore;
using iOne.Policies;
using iOne.WorkTasks;

namespace iOne.EntityFrameworkCore.Policies;

public class EfCorePolicyRequestApprovalQueryRepository : IPolicyRequestApprovalQueryRepository
{
    private readonly IDbContextProvider<iOneDbContext> _dbContextProvider;

    public const string PolicyBusinessName = "policyVersion";

    public async Task<IQueryable<Policy>> GetApprovalListQueryableAsync(Guid currentEmployeeId, string businessCode)
    {
        var dbContext = await _dbContextProvider.GetDbContextAsync();
        var workTaskQuery = BuildApprovalWorkTaskQuery(dbContext, currentEmployeeId)
            .Where(wt => wt.BusinessCode == businessCode);

        return from pv in dbContext.PolicyVersions
               where workTaskQuery.Any(wt => wt.BusinessKey == pv.Id)
               join p in dbContext.Policies on pv.PolicyId equals p.Id
               select p;
    }

    public async Task<IQueryable<PolicyApprovalListRow>> GetApprovalListWithCodeQueryableAsync(
        Guid currentEmployeeId,
        IReadOnlyList<string>? businessCodes)
    {
        var dbContext = await _dbContextProvider.GetDbContextAsync();
        var workTaskQuery = BuildApprovalWorkTaskQuery(dbContext, currentEmployeeId);

        if (businessCodes != null && businessCodes.Count > 0)
            workTaskQuery = workTaskQuery.Where(wt => businessCodes.Contains(wt.BusinessCode));

        return from wt in workTaskQuery
               join pv in dbContext.PolicyVersions on wt.BusinessKey equals pv.Id
               join p in dbContext.Policies on pv.PolicyId equals p.Id
               select new PolicyApprovalListRow
               {
                   Policy = p,
                   PolicyVersionId = pv.Id,
                   BusinessCode = wt.BusinessCode,
                   WorkTaskId = wt.Id,
                   WorkTaskStatus = wt.Status,
                   WorkTaskCreationTime = wt.CreationTime
               };
    }

    public EfCorePolicyRequestApprovalQueryRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
    {
        _dbContextProvider = dbContextProvider;
    }

    /// <summary>
    /// Work tasks where BusinessName = 'policyVersion' and AssigneeId = currentEmployeeId (all statuses).
    /// </summary>
    private static IQueryable<WorkTask> BuildApprovalWorkTaskQuery(iOneDbContext dbContext, Guid currentEmployeeId)
    {
        return dbContext.WorkTasks.Where(wt =>
            wt.BusinessName == PolicyBusinessName && wt.AssigneeId == currentEmployeeId);
    }
}
