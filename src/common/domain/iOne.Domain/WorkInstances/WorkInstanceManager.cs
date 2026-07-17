using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace iOne.WorkInstances;

public class WorkInstanceManager : DomainService
{
    protected IWorkInstanceRepository Repository { get; }

    public WorkInstanceManager(IWorkInstanceRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(WorkInstance entity)
    {
        await Repository.InsertAsync(entity);
    }

    public virtual async Task UpdateStatusAsync(WorkInstance entity, WorkInstanceStatus status)
    {
        entity.UpdateStatus(status);
        await Repository.UpdateAsync(entity);
    }
}
