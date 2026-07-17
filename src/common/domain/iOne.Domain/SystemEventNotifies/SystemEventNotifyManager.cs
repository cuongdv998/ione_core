using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace iOne.SystemEventNotifies;

public class SystemEventNotifyManager : DomainService
{
    protected ISystemEventNotifyRepository Repository { get; }

    public SystemEventNotifyManager(ISystemEventNotifyRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(SystemEventNotify entity)
    {
        await Repository.InsertAsync(entity);
    }

    /// <summary>
    /// Chỉ cập nhật Status (khi user sửa từ UI – chỉ đổi status).
    /// </summary>
    public virtual async Task UpdateStatusAsync(SystemEventNotify entity, SystemEventNotifyStatus status)
    {
        entity.UpdateStatus(status);
        await Repository.UpdateAsync(entity);
    }

    /// <summary>
    /// Cập nhật Status và ReadAt.
    /// </summary>
    public virtual async Task UpdateAsync(SystemEventNotify entity, SystemEventNotifyStatus status, DateTime? readAt)
    {
        entity.UpdateStatus(status);
        entity.UpdateReadAt(readAt);
        await Repository.UpdateAsync(entity);
    }
}
