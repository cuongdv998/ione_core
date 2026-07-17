using System;
using Volo.Abp.Domain.Repositories;

namespace iOne.ResBusinessAssignees;

public interface IResBusinessAssigneeRepository : IRepository<ResBusinessAssignee, Guid>
{
}
