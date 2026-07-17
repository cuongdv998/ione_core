using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iOne.Hr;
using iOne.Hr.HrDepartments;
using iOne.Hr.Localization;
using iOne.Hr.Permissions;
using iOne.HrDepartments; // For Entity, Manager, Repository
using iOne.HrEmployees;
using iOne.ResPartners;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp;

namespace iOne.Hr.HrDepartments;

[RemoteService(Name = HrRemoteServiceConsts.RemoteServiceName)]
[Authorize]
public class HrDepartmentAppService : CrudAppService<
    HrDepartment,
    HrDepartmentDto,
    Guid,
    GetHrDepartmentsInput,
    CreateHrDepartmentDto,
    UpdateHrDepartmentDto>, IHrDepartmentAppService
{
    protected HrDepartmentManager Manager { get; }
    protected IHrDepartmentRepository DepartmentRepository { get; }
    protected IRepository<HrEmployee, Guid> EmployeeRepository { get; }

    public HrDepartmentAppService(
        IHrDepartmentRepository repository,
        HrDepartmentManager manager,
        IRepository<HrEmployee, Guid> employeeRepository)
        : base(repository)
    {
        Manager = manager;
        DepartmentRepository = repository;
        EmployeeRepository = employeeRepository;
        LocalizationResource = typeof(HrResource);
        GetPolicyName = HrDepartmentPermissions.View;
        GetListPolicyName = HrDepartmentPermissions.View;
        CreatePolicyName = HrDepartmentPermissions.Create;
        UpdatePolicyName = HrDepartmentPermissions.Edit;
        DeletePolicyName = HrDepartmentPermissions.Delete;
    }

    public override async Task<HrDepartmentDto> CreateAsync(CreateHrDepartmentDto input)
    {
        // Auto create Partner with PartnerType code = "DIRECT"
        var partnerId = await Manager.CreatePartnerForDepartmentAsync(
            input.Code,
            input.Name,
            input.ProvinceId,
            input.WardId,
            input.Address
        );

        // Create Department entity
        var entity = new HrDepartment(
            GuidGenerator.Create(),
            input.Code,
            input.Name,
            input.Status,
            input.DeptLevel,
            partnerId,
            input.OrgId,
            input.ParentId,
            input.TypeId,
            input.Description,
            input.ProvinceId,
            input.WardId,
            input.Address,
            input.FullAddress,
            input.BankId,
            input.BankNo
        );

        // Create Department (Partner already created)
        await Manager.CreateAsync(entity);

        // Map to DTO with navigation properties
        var dto = await MapToGetOutputDtoAsync(entity);
        return dto;
    }

    public override async Task<HrDepartmentDto> UpdateAsync(Guid id, UpdateHrDepartmentDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        var entity = await Repository.GetAsync(id, includeDetails: true);

        // Update entity fields (Code is immutable)
        await Manager.UpdateAsync(
            entity,
            input.Name,
            input.Description,
            input.Status,
            input.DeptLevel,
            input.ParentId,
            input.OrgId,
            input.TypeId,
            input.ProvinceId,
            input.WardId,
            input.Address,
            input.FullAddress,
            input.BankId,
            input.BankNo
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
        await Repository.DeleteAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
        
        // Update status to Deactive (entity still exists in database due to soft delete)
        entity.UpdateStatus(HrDepartmentStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    public override async Task<PagedResultDto<HrDepartmentDto>> GetListAsync(GetHrDepartmentsInput input)
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
        var dtos = ObjectMapper.Map<List<HrDepartment>, List<HrDepartmentDto>>(entities);

        // Set navigation property names from loaded entities (no additional queries needed)
        for (int i = 0; i < entities.Count; i++)
        {
            if (entities[i].Parent != null)
            {
                dtos[i].ParentName = entities[i].Parent.Name;
            }

            if (entities[i].Org != null)
            {
                dtos[i].OrgName = entities[i].Org.Name;
            }

            if (entities[i].Type != null)
            {
                dtos[i].TypeName = entities[i].Type.Name;
            }

            if (entities[i].Province != null)
            {
                dtos[i].ProvinceName = entities[i].Province.Name;
            }

            if (entities[i].Ward != null)
            {
                dtos[i].WardName = entities[i].Ward.Name;
            }

            if (entities[i].Bank != null)
            {
                dtos[i].BankName = entities[i].Bank.Name;
            }
        }

        return new PagedResultDto<HrDepartmentDto>(totalCount, dtos);
    }

    public async Task<ListResultDto<HrDepartmentTreeDto>> GetTreeAsync()
    {
        var departments = await DepartmentRepository.GetTreeAsync();
        var treeDtos = departments.Select(MapToTreeDto).ToList();
        return new ListResultDto<HrDepartmentTreeDto>(treeDtos);
    }

    public async Task<ListResultDto<HrDepartmentDto>> GetByParentIdAsync(Guid? parentId)
    {
        var departments = await DepartmentRepository.GetByParentIdAsync(parentId);
        var dtos = await Task.WhenAll(departments.Select(MapToGetOutputDtoAsync));
        return new ListResultDto<HrDepartmentDto>(dtos.ToList());
    }

    public async Task<ListResultDto<HrDepartmentDto>> GetRootDepartmentsAsync()
    {
        var departments = await DepartmentRepository.GetRootDepartmentsAsync();
        var dtos = await Task.WhenAll(departments.Select(MapToGetOutputDtoAsync));
        return new ListResultDto<HrDepartmentDto>(dtos.ToList());
    }

    protected override async Task<HrDepartmentDto> MapToGetOutputDtoAsync(HrDepartment entity)
    {
        var dto = ObjectMapper.Map<HrDepartment, HrDepartmentDto>(entity);
        
        // Map navigation properties
        if (entity.Parent != null)
        {
            dto.ParentName = entity.Parent.Name;
        }

        if (entity.Org != null)
        {
            dto.OrgName = entity.Org.Name;
        }

        if (entity.Type != null)
        {
            dto.TypeName = entity.Type.Name;
        }

        if (entity.Province != null)
        {
            dto.ProvinceName = entity.Province.Name;
        }

        if (entity.Ward != null)
        {
            dto.WardName = entity.Ward.Name;
        }

        if (entity.Bank != null)
        {
            dto.BankName = entity.Bank.Name;
        }

        return dto;
    }

    private HrDepartmentTreeDto MapToTreeDto(HrDepartment entity)
    {
        var dto = new HrDepartmentTreeDto
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            Description = entity.Description,
            Status = entity.Status,
            DeptLevel = entity.DeptLevel,
            ParentId = entity.ParentId,
            OrgId = entity.OrgId,
            TypeId = entity.TypeId,
            ProvinceId = entity.ProvinceId,
            WardId = entity.WardId,
            BankId = entity.BankId,
            BankNo = entity.BankNo
        };

        // Map navigation properties
        if (entity.Org != null)
        {
            dto.OrgName = entity.Org.Name;
        }

        if (entity.Type != null)
        {
            dto.TypeName = entity.Type.Name;
        }

        if (entity.Province != null)
        {
            dto.ProvinceName = entity.Province.Name;
        }

        if (entity.Ward != null)
        {
            dto.WardName = entity.Ward.Name;
        }

        if (entity.Bank != null)
        {
            dto.BankName = entity.Bank.Name;
        }

        // Map children recursively
        if (entity.Children != null && entity.Children.Any())
        {
            dto.Children = entity.Children.Select(MapToTreeDto).ToList();
        }

        return dto;
    }

    protected override async Task<IQueryable<HrDepartment>> CreateFilteredQueryAsync(GetHrDepartmentsInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();

        // Include navigation properties to avoid N+1 queries
        query = query
            .Include(x => x.Parent)
            .Include(x => x.Org)
            .Include(x => x.Type)
            .Include(x => x.Province)
            .Include(x => x.Ward)
            .Include(x => x.Bank);

        // Filter by Code (case-insensitive, contains)
        if (!string.IsNullOrWhiteSpace(input.Code))
        {
            query = query.Where(x => EF.Functions.ILike(x.Code, $"%{input.Code}%"));
        }

        // Filter by Name (case-insensitive, contains)
        if (!string.IsNullOrWhiteSpace(input.Name))
        {
            query = query.Where(x => EF.Functions.ILike(x.Name, $"%{input.Name}%"));
        }

        // Filter by Status
        if (input.Status.HasValue)
        {
            query = query.Where(x => x.Status == input.Status.Value);
        }

        // Filter by DeptLevel
        if (input.DeptLevel.HasValue)
        {
            query = query.Where(x => x.DeptLevel == input.DeptLevel.Value);
        }

        // Filter by ParentId
        if (input.ParentId.HasValue)
        {
            query = query.Where(x => x.ParentId == input.ParentId.Value);
        }

        // Filter by OrgId
        if (input.OrgId.HasValue)
        {
            query = query.Where(x => x.OrgId == input.OrgId.Value);
        }

        // Filter by TypeId
        if (input.TypeId.HasValue)
        {
            query = query.Where(x => x.TypeId == input.TypeId.Value);
        }

        // Filter by ProvinceId
        if (input.ProvinceId.HasValue)
        {
            query = query.Where(x => x.ProvinceId == input.ProvinceId.Value);
        }

        // Filter by WardId
        if (input.WardId.HasValue)
        {
            query = query.Where(x => x.WardId == input.WardId.Value);
        }

        // Filter by BankId
        if (input.BankId.HasValue)
        {
            query = query.Where(x => x.BankId == input.BankId.Value);
        }

        return query;
    }

    [Authorize]
    public virtual async Task<List<HrDepartmentSelectDto>> GetSelectListAsync(string? scope = null)
    {
        // If FE passes `scope=ALL` => return all active departments (not deleted).
        var isAll = string.Equals(scope, "ALL", StringComparison.OrdinalIgnoreCase);
        // If FE passes `scope=UNIT` => return all active unit-level departments (not deleted).
        var isUnit = string.Equals(scope, "UNIT", StringComparison.OrdinalIgnoreCase);

        IQueryable<HrDepartment> query;
        if (isAll)
        {
            query = await ReadOnlyRepository.GetQueryableAsync();
            query = query
                .Where(x => x.Status == HrDepartmentStatus.Active && !x.IsDeleted)
                .OrderBy(x => x.Name);
        }
        else if (isUnit)
        {
            query = await ReadOnlyRepository.GetQueryableAsync();
            query = query
                .Where(x =>
                    x.DeptLevel == HrDepartmentLevel.Unit &&
                    x.Status == HrDepartmentStatus.Active &&
                    !x.IsDeleted)
                .OrderBy(x => x.Name);
        }
        else
        {
            // Get current user ID
            var currentUserId = CurrentUser.Id;
            if (currentUserId == null)
            {
                return new List<HrDepartmentSelectDto>();
            }

            // Find employee by UserId
            var employee = await EmployeeRepository.FirstOrDefaultAsync(x => x.UserId == currentUserId);
            if (employee == null)
            {
                return new List<HrDepartmentSelectDto>();
            }

            var userDepartmentId = employee.DepartmentId;

            // Get all departments (to build tree structure)
            var allDepartmentsQuery = await ReadOnlyRepository.GetQueryableAsync();
            var allDepartments = await AsyncExecuter.ToListAsync(allDepartmentsQuery);

            // Get user's department and all child departments recursively
            var allowedDepartmentIds = new HashSet<Guid> { userDepartmentId };
            GetAllChildDepartmentIds(userDepartmentId, allDepartments, allowedDepartmentIds);

            // Filter departments by allowed IDs + active + not deleted
            query = await ReadOnlyRepository.GetQueryableAsync();
            query = query
                .Where(x =>
                    allowedDepartmentIds.Contains(x.Id) &&
                    x.Status == HrDepartmentStatus.Active &&
                    !x.IsDeleted)
                .OrderBy(x => x.Name);
        }

        // Execute query
        var entities = await AsyncExecuter.ToListAsync(query);

        // Map entities to Select DTOs (only Id, Code, Name)
        var dtos = entities.Select(x => new HrDepartmentSelectDto
        {
            Id = x.Id,
            Code = x.Code,
            Name = x.Name
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
}

