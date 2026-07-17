using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iOne.Hr;
using iOne.Hr.HrEmployees;
using iOne.Hr.Localization;
using iOne.Hr.Permissions;
using iOne.HrDepartments;
using iOne.HrEmployees; // For Entity, Manager, Repository
using iOne.HrEmployeeRoles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp;

namespace iOne.Hr.HrEmployees;

[RemoteService(Name = HrRemoteServiceConsts.RemoteServiceName)]
[Authorize]
public class HrEmployeeAppService : CrudAppService<
    HrEmployee,
    HrEmployeeDto,
    Guid,
    GetHrEmployeesInput,
    CreateHrEmployeeDto,
    UpdateHrEmployeeDto>, IHrEmployeeAppService
{
    protected HrEmployeeManager Manager { get; }
    protected IHrEmployeeRepository EmployeeRepository { get; }
    protected IHrEmployeeRoleRelRepository RoleRelRepository { get; }
    protected HrEmployeeRoleRelManager RoleRelManager { get; }
    protected IHrEmployeeRoleRepository RoleRepository { get; }
    protected IRepository<Volo.Abp.Identity.IdentityUser, Guid> UserRepository { get; }
    protected IRepository<HrDepartment, Guid> DepartmentRepository { get; }

    public HrEmployeeAppService(
        IHrEmployeeRepository repository,
        HrEmployeeManager manager,
        IHrEmployeeRoleRelRepository roleRelRepository,
        HrEmployeeRoleRelManager roleRelManager,
        IHrEmployeeRoleRepository roleRepository,
        IRepository<Volo.Abp.Identity.IdentityUser, Guid> userRepository,
        IRepository<HrDepartment, Guid> departmentRepository)
        : base(repository)
    {
        Manager = manager;
        EmployeeRepository = repository;
        RoleRelRepository = roleRelRepository;
        RoleRelManager = roleRelManager;
        RoleRepository = roleRepository;
        UserRepository = userRepository;
        DepartmentRepository = departmentRepository;
        LocalizationResource = typeof(HrResource);
        GetPolicyName = HrEmployeePermissions.View;
        GetListPolicyName = HrEmployeePermissions.View;
        CreatePolicyName = HrEmployeePermissions.Create;
        UpdatePolicyName = HrEmployeePermissions.Edit;
        DeletePolicyName = HrEmployeePermissions.Delete;
    }

    public override async Task<HrEmployeeDto> CreateAsync(CreateHrEmployeeDto input)
    {
        // Create Employee entity
        var entity = new HrEmployee(
            GuidGenerator.Create(),
            input.Code,
            input.FullName,
            input.Status,
            input.DepartmentId,
            input.PositionId,
            input.LevelId,
            input.PartnerId,
            input.OrgId,
            input.IsManager,
            input.ManagerId,
            input.ProvinceId,
            input.WardId,
            input.Address,
            input.FullAddress,
            input.Phone,
            input.Email,
            input.UserId
        );

        // Create Employee
        await Manager.CreateAsync(entity);

        // Map to DTO with navigation properties
        var dto = await MapToGetOutputDtoAsync(entity);
        return dto;
    }

    public override async Task<HrEmployeeDto> UpdateAsync(Guid id, UpdateHrEmployeeDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        var entity = await Repository.GetAsync(id, includeDetails: true);

        // Update entity fields (Code is immutable)
        await Manager.UpdateAsync(
            entity,
            input.FullName,
            input.Status,
            input.PositionId,
            input.LevelId,
            input.PartnerId,
            input.OrgId,
            input.DepartmentId,
            input.IsManager,
            input.ManagerId,
            input.ProvinceId,
            input.WardId,
            input.Address,
            input.FullAddress,
            input.Phone,
            input.Email,
            input.UserId
        );

        // Ensure changes are saved
        await CurrentUnitOfWork.SaveChangesAsync();

        // Map to DTO with navigation properties
        var dto = await MapToGetOutputDtoAsync(entity);
        return dto;
    }

    public override async Task DeleteAsync(Guid id)
    {
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id);
        
        // Delete to trigger audit logging with ChangeType = Deleted
        // FullAuditedAggregateRoot uses soft delete, so entity is not actually removed from database
        await Manager.DeleteAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    public override async Task<PagedResultDto<HrEmployeeDto>> GetListAsync(GetHrEmployeesInput input)
    {
        // Get filtered query with navigation properties included
        var query = await CreateFilteredQueryAsync(input);
        
        // Get total count
        var totalCount = await AsyncExecuter.CountAsync(query);

        // Apply sorting and paging
        query = ApplySorting(query, input);
        query = ApplyPaging(query, input);

        // Execute query and get entities with navigation properties loaded
        var entities = await AsyncExecuter.ToListAsync(query);

        // Map all entities to DTOs at once
        var dtos = ObjectMapper.Map<List<HrEmployee>, List<HrEmployeeDto>>(entities);

        // Set navigation property names from loaded entities (no additional queries needed)
        for (int i = 0; i < entities.Count; i++)
        {
            if (entities[i].Position != null)
            {
                dtos[i].PositionName = entities[i].Position.Name;
            }

            if (entities[i].Level != null)
            {
                dtos[i].LevelName = entities[i].Level.Name;
            }

            if (entities[i].Partner != null)
            {
                dtos[i].PartnerName = entities[i].Partner.Name;
            }

            if (entities[i].Org != null)
            {
                dtos[i].OrgName = entities[i].Org.Name;
            }

            if (entities[i].Department != null)
            {
                dtos[i].DepartmentName = entities[i].Department.Name;
            }

            if (entities[i].Manager != null)
            {
                dtos[i].ManagerName = entities[i].Manager.FullName;
            }

            if (entities[i].Province != null)
            {
                dtos[i].ProvinceName = entities[i].Province.Name;
            }

            if (entities[i].Ward != null)
            {
                dtos[i].WardName = entities[i].Ward.Name;
            }

            if (entities[i].UserId.HasValue)
            {
                var user = await UserRepository.FirstOrDefaultAsync(x => x.Id == entities[i].UserId.Value);
                if (user != null)
                {
                    dtos[i].UserName = user.UserName;
                }
            }
        }

        return new PagedResultDto<HrEmployeeDto>(totalCount, dtos);
    }

    public async Task<List<HrEmployeeDto>> GetByDepartmentIdAsync(Guid departmentId)
    {
        var employees = await EmployeeRepository.GetByDepartmentIdAsync(departmentId);
        var dtos = await Task.WhenAll(employees.Select(MapToGetOutputDtoAsync));
        return dtos.ToList();
    }

    public async Task<List<HrEmployeeDto>> GetListByIdsAsync(List<Guid> ids)
    {
        if (ids == null || ids.Count == 0)
        {
            return new List<HrEmployeeDto>();
        }

        var query = (await Repository.GetQueryableAsync())
            .Include(x => x.Position)
            .Include(x => x.Level)
            .Include(x => x.Partner)
            .Include(x => x.Org)
            .Include(x => x.Department)
            .Include(x => x.Manager)
            .Include(x => x.Province)
            .Include(x => x.Ward)
            .Where(x => ids.Contains(x.Id) && !x.IsDeleted);

        var entities = await AsyncExecuter.ToListAsync(query);

        var dtos = ObjectMapper.Map<List<HrEmployee>, List<HrEmployeeDto>>(entities);

        // Set navigation property names
        for (int i = 0; i < entities.Count; i++)
        {
            if (entities[i].Position != null)
                dtos[i].PositionName = entities[i].Position.Name;
            if (entities[i].Level != null)
                dtos[i].LevelName = entities[i].Level.Name;
            if (entities[i].Partner != null)
                dtos[i].PartnerName = entities[i].Partner.Name;
            if (entities[i].Org != null)
                dtos[i].OrgName = entities[i].Org.Name;
            if (entities[i].Department != null)
                dtos[i].DepartmentName = entities[i].Department.Name;
            if (entities[i].Manager != null)
                dtos[i].ManagerName = entities[i].Manager.FullName;
            if (entities[i].Province != null)
                dtos[i].ProvinceName = entities[i].Province.Name;
            if (entities[i].Ward != null)
                dtos[i].WardName = entities[i].Ward.Name;
        }

        return dtos;
    }

    public async Task<List<HrEmployeeRoleRelDto>> GetRolesAsync(Guid employeeId)
    {
        var roleRels = await RoleRelRepository.GetByEmployeeIdAsync(employeeId);
        var dtos = roleRels.Select(rel =>
        {
            var dto = ObjectMapper.Map<HrEmployeeRoleRel, HrEmployeeRoleRelDto>(rel);
            if (rel.Role != null)
            {
                dto.RoleName = rel.Role.Name;
            }
            return dto;
        }).ToList();
        return dtos;
    }

    public async Task<HrEmployeeRoleRelDto> AddRoleAsync(Guid employeeId, CreateHrEmployeeRoleRelDto input)
    {
        // Verify employee exists
        var employee = await Repository.GetAsync(employeeId);

        // Create role relation
        var roleRel = new HrEmployeeRoleRel(
            GuidGenerator.Create(),
            employeeId,
            input.RoleId,
            input.EffectDate,
            input.ExpireDate
        );

        // Create with date overlap validation
        await RoleRelManager.CreateAsync(roleRel);

        // Ensure changes are saved to database before querying
        await CurrentUnitOfWork.SaveChangesAsync();

        // Load with navigation properties using query
        var query = await RoleRelRepository.GetQueryableAsync();
        var roleRelWithNav = await query
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == roleRel.Id);
        
        if (roleRelWithNav == null)
        {
            throw new Volo.Abp.BusinessException("Hr:HrEmployeeRoleRel:NotFound");
        }
        
        var dto = ObjectMapper.Map<HrEmployeeRoleRel, HrEmployeeRoleRelDto>(roleRelWithNav);
        if (roleRelWithNav.Role != null)
        {
            dto.RoleName = roleRelWithNav.Role.Name;
        }
        return dto;
    }

    public async Task<HrEmployeeRoleRelDto> UpdateRoleAsync(Guid employeeId, Guid roleRelId, UpdateHrEmployeeRoleRelDto input)
    {
        // Verify employee exists
        await Repository.GetAsync(employeeId);

        // Load role relation
        var roleRel = await RoleRelRepository.GetAsync(roleRelId);

        // Verify it belongs to the employee
        if (roleRel.EmployeeId != employeeId)
        {
            throw new Volo.Abp.BusinessException("Hr:HrEmployeeRoleRel:NotBelongToEmployee");
        }

        // Update with date overlap validation
        await RoleRelManager.UpdateAsync(
            roleRel,
            input.RoleId,
            input.EffectDate,
            input.ExpireDate
        );

        // Ensure changes are saved to database before querying
        await CurrentUnitOfWork.SaveChangesAsync();

        // Load with navigation properties using query
        var query = await RoleRelRepository.GetQueryableAsync();
        var roleRelWithNav = await query
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == roleRelId);
        
        if (roleRelWithNav == null)
        {
            throw new Volo.Abp.BusinessException("Hr:HrEmployeeRoleRel:NotFound");
        }
        
        var dto = ObjectMapper.Map<HrEmployeeRoleRel, HrEmployeeRoleRelDto>(roleRelWithNav);
        if (roleRelWithNav.Role != null)
        {
            dto.RoleName = roleRelWithNav.Role.Name;
        }
        return dto;
    }

    public async Task RemoveRoleAsync(Guid employeeId, Guid roleRelId)
    {
        // Verify employee exists
        await Repository.GetAsync(employeeId);

        // Load role relation
        var roleRel = await RoleRelRepository.GetAsync(roleRelId);

        // Verify it belongs to the employee
        if (roleRel.EmployeeId != employeeId)
        {
            throw new Volo.Abp.BusinessException("Hr:HrEmployeeRoleRel:NotBelongToEmployee");
        }

        // Delete
        await RoleRelRepository.DeleteAsync(roleRel);
    }

    [Authorize]
    public virtual async Task<HrEmployeeDto?> GetCurrentAsync()
    {
        var currentUserId = CurrentUser.Id;
        if (currentUserId == null)
        {
            return null;
        }

        var query = (await Repository.GetQueryableAsync())
            .Include(x => x.Department)
            .Include(x => x.Position)
            .Include(x => x.Org)
            .Where(x => x.UserId == currentUserId && !x.IsDeleted);

        var entity = await AsyncExecuter.FirstOrDefaultAsync(query);
        if (entity == null)
        {
            return null;
        }

        return await MapToGetOutputDtoAsync(entity);
    }

    [Authorize]
    public virtual async Task<List<HrEmployeeSelectDto>> GetSelectListAsync()
    {
        // Get current user ID
        var currentUserId = CurrentUser.Id;
        if (currentUserId == null)
        {
            return new List<HrEmployeeSelectDto>();
        }

        // Find employee by UserId to get the user's department
        var currentEmployee = await EmployeeRepository.FirstOrDefaultAsync(x => x.UserId == currentUserId);
        if (currentEmployee == null)
        {
            return new List<HrEmployeeSelectDto>();
        }

        var userDepartmentId = currentEmployee.DepartmentId;

        // Get all departments (to build tree structure)
        var allDepartmentsQuery = await DepartmentRepository.GetQueryableAsync();
        var allDepartments = await AsyncExecuter.ToListAsync(allDepartmentsQuery);

        // Get user's department and all child departments recursively
        var allowedDepartmentIds = new HashSet<Guid> { userDepartmentId };
        GetAllChildDepartmentIds(userDepartmentId, allDepartments, allowedDepartmentIds);

        // Filter employees by allowed departments and sort by FullName A-Z
        var query = await ReadOnlyRepository.GetQueryableAsync();
        query = query.Where(x => allowedDepartmentIds.Contains(x.DepartmentId));
        query = query.OrderBy(x => x.FullName);

        var entities = await AsyncExecuter.ToListAsync(query);

        // Map entities to Select DTOs (only Id, Code, FullName)
        var dtos = entities.Select(x => new HrEmployeeSelectDto
        {
            Id = x.Id,
            Code = x.Code,
            FullName = x.FullName
        }).ToList();

        return dtos;
    }

    /// <summary>
    /// Recursively get all child department IDs
    /// </summary>
    private void GetAllChildDepartmentIds(Guid parentId, List<HrDepartment> allDepartments, HashSet<Guid> result)
    {
        var children = allDepartments.Where(x => x.ParentId == parentId).ToList();
        foreach (var child in children)
        {
            if (result.Add(child.Id))
            {
                GetAllChildDepartmentIds(child.Id, allDepartments, result);
            }
        }
    }

    protected override async Task<IQueryable<HrEmployee>> CreateFilteredQueryAsync(GetHrEmployeesInput input)
    {
        var query = (await Repository.GetQueryableAsync())
            .Include(x => x.Position)
            .Include(x => x.Level)
            .Include(x => x.Partner)
            .Include(x => x.Org)
            .Include(x => x.Department)
            .Include(x => x.Manager)
            .Include(x => x.Province)
            .Include(x => x.Ward)
            .Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(input.RoleCode))
        {
            var normalizedRoleCode = input.RoleCode.Trim().ToUpperInvariant();

            var roleQuery = await RoleRepository.GetQueryableAsync();
            var roleRelQuery = await RoleRelRepository.GetQueryableAsync();

            var today = DateTime.Today;

            var employeeIdsQuery =
                from rel in roleRelQuery
                join role in roleQuery on rel.RoleId equals role.Id
                where role.Code == normalizedRoleCode
                      && (rel.ExpireDate == null || rel.ExpireDate >= today)
                select rel.EmployeeId;

            var employeeIds = await AsyncExecuter.ToListAsync(employeeIdsQuery);

            if (employeeIds.Count == 0)
            {
                return query.Where(x => false);
            }

            query = query.Where(x => employeeIds.Contains(x.Id));
        }

        if (!string.IsNullOrWhiteSpace(input.Code))
        {
            query = query.Where(x => EF.Functions.ILike(x.Code, $"%{input.Code}%"));
        }

        if (!string.IsNullOrWhiteSpace(input.FullName))
        {
            query = query.Where(x => EF.Functions.ILike(x.FullName, $"%{input.FullName}%"));
        }

        if (input.Status.HasValue)
        {
            query = query.Where(x => x.Status == input.Status.Value);
        }

        if (input.DepartmentId.HasValue)
        {
            query = query.Where(x => x.DepartmentId == input.DepartmentId.Value);
        }

        if (input.OrgId.HasValue)
        {
            query = query.Where(x => x.OrgId == input.OrgId.Value);
        }

        if (input.PartnerId.HasValue)
        {
            query = query.Where(x => x.PartnerId == input.PartnerId.Value);
        }

        if (input.PositionId.HasValue)
        {
            query = query.Where(x => x.PositionId == input.PositionId.Value);
        }

        if (input.LevelId.HasValue)
        {
            query = query.Where(x => x.LevelId == input.LevelId.Value);
        }

        return query;
    }

    protected override async Task<HrEmployeeDto> MapToGetOutputDtoAsync(HrEmployee entity)
    {
        var dto = ObjectMapper.Map<HrEmployee, HrEmployeeDto>(entity);
        
        // Map navigation properties
        if (entity.Position != null)
        {
            dto.PositionName = entity.Position.Name;
        }

        if (entity.Level != null)
        {
            dto.LevelName = entity.Level.Name;
        }

        if (entity.Partner != null)
        {
            dto.PartnerName = entity.Partner.Name;
        }

        if (entity.Org != null)
        {
            dto.OrgName = entity.Org.Name;
        }

        if (entity.Department != null)
        {
            dto.DepartmentName = entity.Department.Name;
        }

        if (entity.Manager != null)
        {
            dto.ManagerName = entity.Manager.FullName;
        }

        if (entity.Province != null)
        {
            dto.ProvinceName = entity.Province.Name;
        }

        if (entity.Ward != null)
        {
            dto.WardName = entity.Ward.Name;
        }

        if (entity.UserId.HasValue)
        {
            var user = await UserRepository.FirstOrDefaultAsync(x => x.Id == entity.UserId.Value);
            if (user != null)
            {
                dto.UserName = user.UserName;
            }
        }

        return dto;
    }
}

