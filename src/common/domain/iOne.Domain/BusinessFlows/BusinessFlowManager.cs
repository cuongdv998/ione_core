using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.BusinessFlows;

public class BusinessFlowManager : DomainService
{
    protected IBusinessFlowRepository Repository { get; }

    public BusinessFlowManager(IBusinessFlowRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(BusinessFlow entity)
    {
        await Repository.InsertAsync(entity);
    }

    public virtual async Task UpdateWorkflowInfoAsync(
        BusinessFlow entity,
        string workflowName,
        string workflowVersion,
        DateTime? expireDate)
    {
        entity.UpdateWorkflowInfo(workflowName, workflowVersion, expireDate);
        await Repository.UpdateAsync(entity);
    }
}
