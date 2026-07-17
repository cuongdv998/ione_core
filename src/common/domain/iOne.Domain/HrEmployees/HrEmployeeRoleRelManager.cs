using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.HrEmployees;

public class HrEmployeeRoleRelManager : DomainService
{
    protected IHrEmployeeRoleRelRepository Repository { get; }

    public HrEmployeeRoleRelManager(IHrEmployeeRoleRelRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(HrEmployeeRoleRel roleRel)
    {
        // Validate date overlap
        if (await Repository.HasOverlappingDatesAsync(
            roleRel.EmployeeId,
            roleRel.RoleId,
            roleRel.EffectDate,
            roleRel.ExpireDate))
        {
            throw new BusinessException("Hr:HrEmployeeRoleRel:DateOverlap")
                .WithData("EmployeeId", roleRel.EmployeeId)
                .WithData("RoleId", roleRel.RoleId)
                .WithData("EffectDate", roleRel.EffectDate)
                .WithData("ExpireDate", roleRel.ExpireDate);
        }

        await Repository.InsertAsync(roleRel);
    }

    public virtual async Task UpdateAsync(
        HrEmployeeRoleRel roleRel,
        Guid roleId,
        DateTime effectDate,
        DateTime? expireDate)
    {
        // Validate date overlap (exclude current record)
        if (await Repository.HasOverlappingDatesAsync(
            roleRel.EmployeeId,
            roleId,
            effectDate,
            expireDate,
            roleRel.Id))
        {
            throw new BusinessException("Hr:HrEmployeeRoleRel:DateOverlap")
                .WithData("EmployeeId", roleRel.EmployeeId)
                .WithData("RoleId", roleId)
                .WithData("EffectDate", effectDate)
                .WithData("ExpireDate", expireDate);
        }

        roleRel.UpdateRoleId(roleId);
        roleRel.UpdateEffectDate(effectDate);
        roleRel.UpdateExpireDate(expireDate);

        await Repository.UpdateAsync(roleRel);
    }

    public virtual async Task<bool> ValidateDateOverlapAsync(
        Guid employeeId,
        Guid roleId,
        DateTime effectDate,
        DateTime? expireDate,
        Guid? excludeId = null)
    {
        return await Repository.HasOverlappingDatesAsync(
            employeeId,
            roleId,
            effectDate,
            expireDate,
            excludeId);
    }
}

