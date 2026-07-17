using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.HrEmployeeRoles;

public interface IHrEmployeeRoleRepository : IRepository<HrEmployeeRole, Guid>
{
    Task<bool> IsCodeExistsAsync(string code);
}

