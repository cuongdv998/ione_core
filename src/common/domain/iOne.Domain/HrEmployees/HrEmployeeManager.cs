using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using iOne.HrDepartments;

namespace iOne.HrEmployees;

public class HrEmployeeManager : DomainService
{
    protected IHrEmployeeRepository Repository { get; }
    protected IRepository<HrDepartment, Guid> DepartmentRepository { get; }

    public HrEmployeeManager(
        IHrEmployeeRepository repository,
        IRepository<HrDepartment, Guid> departmentRepository)
    {
        Repository = repository;
        DepartmentRepository = departmentRepository;
    }

    public virtual async Task CreateAsync(HrEmployee employee)
    {
        // Check code uniqueness
        if (await Repository.IsCodeExistsAsync(employee.Code))
        {
            throw new BusinessException("Hr:HrEmployee:CodeExists")
                .WithData("Code", employee.Code);
        }

        // Validate department exists
        var department = await DepartmentRepository.FirstOrDefaultAsync(x => x.Id == employee.DepartmentId);
        if (department == null)
        {
            throw new BusinessException("Hr:HrEmployee:DepartmentNotFound");
        }

        // Validate org exists and is Unit level (if provided)
        if (employee.OrgId.HasValue)
        {
            var org = await DepartmentRepository.FirstOrDefaultAsync(x => x.Id == employee.OrgId.Value);
            if (org == null)
            {
                throw new BusinessException("Hr:HrEmployee:OrgNotFound");
            }

            if (org.DeptLevel != HrDepartmentLevel.Unit)
            {
                throw new BusinessException("Hr:HrEmployee:OrgMustBeUnit")
                    .WithData("OrgId", employee.OrgId.Value);
            }
        }

        // Validate manager exists (if provided)
        if (employee.ManagerId.HasValue)
        {
            var manager = await Repository.FirstOrDefaultAsync(x => x.Id == employee.ManagerId.Value);
            if (manager == null)
            {
                throw new BusinessException("Hr:HrEmployee:ManagerNotFound");
            }
        }

        await Repository.InsertAsync(employee);
    }

    public virtual async Task UpdateAsync(
        HrEmployee employee,
        string fullName,
        HrEmployeeStatus status,
        Guid? positionId,
        Guid? levelId,
        Guid? partnerId,
        Guid? orgId,
        Guid departmentId,
        bool? isManager,
        Guid? managerId,
        Guid? provinceId,
        Guid? wardId,
        string? address,
        string? fullAddress,
        string? phone,
        string? email,
        Guid? userId)
    {
        // Validate department exists
        var department = await DepartmentRepository.FirstOrDefaultAsync(x => x.Id == departmentId);
        if (department == null)
        {
            throw new BusinessException("Hr:HrEmployee:DepartmentNotFound");
        }

        // Validate org exists and is Unit level (if provided)
        if (orgId.HasValue)
        {
            var org = await DepartmentRepository.FirstOrDefaultAsync(x => x.Id == orgId.Value);
            if (org == null)
            {
                throw new BusinessException("Hr:HrEmployee:OrgNotFound");
            }

            if (org.DeptLevel != HrDepartmentLevel.Unit)
            {
                throw new BusinessException("Hr:HrEmployee:OrgMustBeUnit")
                    .WithData("OrgId", orgId.Value);
            }
        }

        // Validate manager exists (if provided)
        if (managerId.HasValue)
        {
            var manager = await Repository.FirstOrDefaultAsync(x => x.Id == managerId.Value);
            if (manager == null)
            {
                throw new BusinessException("Hr:HrEmployee:ManagerNotFound");
            }

            // Prevent self-reference
            if (managerId.Value == employee.Id)
            {
                throw new BusinessException("Hr:HrEmployee:ManagerCannotBeSelf");
            }
        }

        employee.UpdateFullName(fullName);
        employee.UpdateStatus(status);
        employee.UpdatePositionId(positionId);
        employee.UpdateLevelId(levelId);
        employee.UpdatePartnerId(partnerId);
        employee.UpdateOrgId(orgId);
        employee.UpdateDepartmentId(departmentId);
        employee.UpdateIsManager(isManager);
        employee.UpdateManagerId(managerId);
        employee.UpdateProvinceId(provinceId);
        employee.UpdateWardId(wardId);
        employee.UpdateAddress(address);
        employee.UpdateFullAddress(fullAddress);
        employee.UpdatePhone(phone);
        employee.UpdateEmail(email);
        employee.UpdateUserId(userId);

        await Repository.UpdateAsync(employee);
    }

    public virtual async Task DeleteAsync(HrEmployee employee)
    {
        // Soft delete (ABP audit log)
        await Repository.DeleteAsync(employee);

        // Update status to Deactive
        employee.UpdateStatus(HrEmployeeStatus.Deactive);
        await Repository.UpdateAsync(employee);
    }
}

