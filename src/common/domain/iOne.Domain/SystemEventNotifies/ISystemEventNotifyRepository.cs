using System;
using Volo.Abp.Domain.Repositories;

namespace iOne.SystemEventNotifies;

public interface ISystemEventNotifyRepository : IRepository<SystemEventNotify, Guid>
{
}
