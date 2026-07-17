using System;
using Volo.Abp.Domain.Repositories;

namespace iOne.WorkInstances;

public interface IWorkInstanceRepository : IRepository<WorkInstance, Guid>
{
}
