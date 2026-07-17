using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace iOne.WorkTasks;

public class WorkTaskManager : DomainService
{
    protected IWorkTaskRepository Repository { get; }

    public WorkTaskManager(IWorkTaskRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(WorkTask entity)
    {
        await Repository.InsertAsync(entity);
    }

    public virtual async Task UpdateStatusAsync(WorkTask entity, WorkTaskStatus status)
    {
        entity.UpdateStatus(status);
        await Repository.UpdateAsync(entity);
    }
}
