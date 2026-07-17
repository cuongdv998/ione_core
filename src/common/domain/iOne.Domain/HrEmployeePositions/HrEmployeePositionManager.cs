using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.HrEmployeePositions;

public class HrEmployeePositionManager : DomainService
{
    protected IHrEmployeePositionRepository Repository { get; }

    public HrEmployeePositionManager(IHrEmployeePositionRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(HrEmployeePosition employeePosition)
    {
        // Check code uniqueness
        if (await Repository.IsCodeExistsAsync(employeePosition.Code))
        {
            throw new BusinessException("HrEmployeePosition:CodeExists")
                .WithData("Code", employeePosition.Code);
        }

        await Repository.InsertAsync(employeePosition);
    }

    public virtual async Task UpdateAsync(HrEmployeePosition employeePosition, string name, HrEmployeePositionType type, HrEmployeePositionStatus status)
    {
        employeePosition.UpdateName(name);
        employeePosition.UpdateType(type);
        employeePosition.UpdateStatus(status);
        await Repository.UpdateAsync(employeePosition);
    }
}

