using System;
using Volo.Abp.Domain.Repositories;

namespace iOne.WorkTasks;

public interface IWorkTaskRepository : IRepository<WorkTask, Guid>
{
}
