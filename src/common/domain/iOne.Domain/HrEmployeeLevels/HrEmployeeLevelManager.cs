using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.HrEmployeeLevels;

public class HrEmployeeLevelManager : DomainService
{
    protected IHrEmployeeLevelRepository Repository { get; }

    public HrEmployeeLevelManager(IHrEmployeeLevelRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(HrEmployeeLevel employeeLevel)
    {
        // Check code uniqueness
        if (await Repository.IsCodeExistsAsync(employeeLevel.Code))
        {
            throw new BusinessException("HrEmployeeLevel:CodeExists")
                .WithData("Code", employeeLevel.Code);
        }

        await Repository.InsertAsync(employeeLevel);
    }

    public virtual async Task UpdateAsync(
        HrEmployeeLevel employeeLevel, 
        string name, 
        HrEmployeeLevelStatus status,
        string? description = null)
    {
        // ⚠️ QUAN TRỌNG: Không có parameter code - Code không được phép sửa
        employeeLevel.UpdateName(name);
        employeeLevel.UpdateStatus(status);
        employeeLevel.UpdateDescription(description);
        await Repository.UpdateAsync(employeeLevel);
    }
}

