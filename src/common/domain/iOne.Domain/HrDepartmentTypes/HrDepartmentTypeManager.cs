using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.HrDepartmentTypes;

public class HrDepartmentTypeManager : DomainService
{
    protected IHrDepartmentTypeRepository Repository { get; }

    public HrDepartmentTypeManager(IHrDepartmentTypeRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(HrDepartmentType departmentType)
    {
        // Check code uniqueness
        if (await Repository.IsCodeExistsAsync(departmentType.Code))
        {
            throw new BusinessException("HrDepartmentType:CodeExists")
                .WithData("Code", departmentType.Code);
        }

        await Repository.InsertAsync(departmentType);
    }

    public virtual async Task UpdateAsync(HrDepartmentType departmentType, string name, HrDepartmentTypeStatus status)
    {
        departmentType.UpdateName(name);
        departmentType.UpdateStatus(status);
        await Repository.UpdateAsync(departmentType);
    }
}

