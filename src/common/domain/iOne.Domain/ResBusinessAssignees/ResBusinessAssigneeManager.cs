using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace iOne.ResBusinessAssignees;

public class ResBusinessAssigneeManager : DomainService
{
    protected IResBusinessAssigneeRepository Repository { get; }

    public ResBusinessAssigneeManager(IResBusinessAssigneeRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ResBusinessAssignee entity)
    {
        await Repository.InsertAsync(entity);
    }

    /// <summary>
    /// Cập nhật AssigneeId và ExpireDate.
    /// </summary>
    public virtual async Task UpdateAsync(ResBusinessAssignee entity, Guid? assigneeId, DateTime? expireDate)
    {
        entity.UpdateAssigneeId(assigneeId);
        entity.UpdateExpireDate(expireDate);
        await Repository.UpdateAsync(entity);
    }
}
