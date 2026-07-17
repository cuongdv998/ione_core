using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.HrEmployeeRoles;

public class HrEmployeeRoleManager : DomainService
{
    protected IHrEmployeeRoleRepository Repository { get; }

    public HrEmployeeRoleManager(IHrEmployeeRoleRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(HrEmployeeRole employeeRole)
    {
        // Check code uniqueness
        if (await Repository.IsCodeExistsAsync(employeeRole.Code))
        {
            throw new BusinessException("HrEmployeeRole:CodeExists")
                .WithData("Code", employeeRole.Code);
        }

        await Repository.InsertAsync(employeeRole);
    }

    public virtual async Task UpdateAsync(HrEmployeeRole employeeRole, string name, HrEmployeeRoleStatus status)
    {
        // ⚠️ QUAN TRỌNG: Không có parameter code - Code không được phép sửa
        employeeRole.UpdateName(name);
        employeeRole.UpdateStatus(status);
        await Repository.UpdateAsync(employeeRole);
    }
}

