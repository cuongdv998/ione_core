using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iOne.Master;
using iOne.Master.Localization;
using iOne.Master.Permissions;
using iOne.Master.ResWards;
using iOne.ResCustomers;
using iOne.ResProvinces;
using iOne.ResWards;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

using iOne.HrDepartments;
using iOne.HrEmployees;
using iOne.ResPartners;

namespace iOne.Master.ResWards;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Authorize(ResWardPermissions.Default)]
public class ResWardAppService : CrudAppService<
    ResWard,
    ResWardDto,
    Guid,
    GetResWardsInput,
    CreateResWardDto,
    UpdateResWardDto>,
    IResWardAppService
{
    protected ResWardManager Manager { get; }
    protected IResProvinceRepository ProvinceRepository { get; }
    protected IResCustomerRepository CustomerRepository { get; }
    protected IResPartnerRepository PartnerRepository { get; }
    protected IHrDepartmentRepository DepartmentRepository { get; }
    protected IHrEmployeeRepository EmployeeRepository { get; }

    public ResWardAppService(
        IResWardRepository repository,
        ResWardManager manager,
        IResProvinceRepository provinceRepository,
        IResCustomerRepository customerRepository,
        IResPartnerRepository partnerRepository,
        IHrDepartmentRepository departmentRepository,
        IHrEmployeeRepository employeeRepository)
        : base(repository)
    {
        Manager = manager;
        ProvinceRepository = provinceRepository;
        CustomerRepository = customerRepository;
        PartnerRepository = partnerRepository;
        DepartmentRepository = departmentRepository;
        EmployeeRepository = employeeRepository;
        LocalizationResource = typeof(MasterResource);
        GetPolicyName = ResWardPermissions.View;
        GetListPolicyName = ResWardPermissions.View;
        CreatePolicyName = ResWardPermissions.Create;
        UpdatePolicyName = ResWardPermissions.Edit;
        DeletePolicyName = ResWardPermissions.Delete;
    }

    public override async Task<ResWardDto> CreateAsync(CreateResWardDto input)
    {
        var entity = new ResWard(
            GuidGenerator.Create(),
            input.ProvinceId,
            input.Code,
            input.Name,
            input.Status,
            input.Description
        );

        await Manager.CreateAsync(entity);

        return await MapToDtoWithProvinceNameAsync(entity);
    }

    public override async Task<ResWardDto> UpdateAsync(Guid id, UpdateResWardDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        var entity = await Repository.GetAsync(id);

        // Kiểm tra nếu chuyển status về Deactive thì phải kiểm tra ràng buộc
        if (input.Status == ResWardStatus.Deactive && entity.Status == ResWardStatus.Active)
        {
            await CheckConstraintsAsync(id, entity.Name);
        }

        // ⚠️ QUAN TRỌNG: Chỉ update ProvinceId, Name, Status, Description - không update Code
        await Manager.UpdateAsync(
            entity,
            input.ProvinceId,
            input.Name,
            input.Status,
            input.Description
        );

        // Ensure changes are saved
        await CurrentUnitOfWork.SaveChangesAsync();

        return await MapToDtoWithProvinceNameAsync(entity);
    }

    public override async Task<ResWardDto> GetAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);
        return await MapToDtoWithProvinceNameAsync(entity);
    }

    public override async Task DeleteAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);

        // Kiểm tra ràng buộc trước khi xóa
        await CheckConstraintsAsync(id, entity.Name);

        // ⚠️ QUAN TRỌNG: Delete trước để trigger audit logging với ChangeType = Deleted
        // FullAuditedAggregateRoot uses soft delete, so entity is not actually removed from database
        await Repository.DeleteAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();

        // Sau đó update status to Deactive (entity still exists in database due to soft delete)
        entity.UpdateStatus(ResWardStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected virtual async Task CheckConstraintsAsync(Guid id, string name)
    {
        // 1. Kiểm tra có Customer đang sử dụng không
        if (await CustomerRepository.AnyByWardIdAsync(id))
        {
            throw new BusinessException("Master:ResWard:InUseByCustomer")
                .WithData("Name", name);
        }

        // 2. Kiểm tra có Partner đang sử dụng không
        if (await PartnerRepository.AnyByWardIdAsync(id))
        {
            throw new BusinessException("Master:ResWard:InUseByPartner")
                .WithData("Name", name);
        }

        // 3. Kiểm tra có Đơn vị/Phòng ban đang sử dụng không
        if (await DepartmentRepository.AnyByWardIdAsync(id))
        {
            throw new BusinessException("Master:ResWard:InUseByDepartment")
                .WithData("Name", name);
        }

        // 4. Kiểm tra có Nhân viên đang sử dụng không
        if (await EmployeeRepository.AnyByWardIdAsync(id))
        {
            throw new BusinessException("Master:ResWard:InUseByEmployee")
                .WithData("Name", name);
        }
    }

    public override async Task<PagedResultDto<ResWardDto>> GetListAsync(GetResWardsInput input)
    {
        // Get filtered query
        var query = await CreateFilteredQueryAsync(input);
        
        // Get total count
        var totalCount = await AsyncExecuter.CountAsync(query);

        // Apply sorting and paging
        query = ApplySorting(query, input);
        query = ApplyPaging(query, input);

        // Execute query
        var entities = await AsyncExecuter.ToListAsync(query);

        // Map all entities to DTOs at once
        var dtos = ObjectMapper.Map<List<ResWard>, List<ResWardDto>>(entities);

        // Load Province names
        var provinceIds = entities.Select(x => x.ProvinceId).Distinct().ToList();
        
        if (provinceIds.Count > 0)
        {
            var provinces = await ProvinceRepository.GetListAsync(x => provinceIds.Contains(x.Id));
            var provinceDict = provinces.ToDictionary(x => x.Id, x => x.Name);

            for (int i = 0; i < entities.Count; i++)
            {
                if (provinceDict.TryGetValue(entities[i].ProvinceId, out var provinceName))
                {
                    dtos[i].ProvinceName = provinceName;
                }
            }
        }

        return new PagedResultDto<ResWardDto>(totalCount, dtos);
    }

    protected async Task<ResWardDto> MapToDtoWithProvinceNameAsync(ResWard entity)
    {
        var dto = ObjectMapper.Map<ResWard, ResWardDto>(entity);

        // Load province name if needed
        if (entity.ProvinceId != Guid.Empty)
        {
            // If Province navigation property is already loaded, use it
            if (entity.Province != null)
            {
                dto.ProvinceName = entity.Province.Name;
            }
            else
            {
                // Otherwise, load from repository
                var province = await ProvinceRepository.GetAsync(entity.ProvinceId);
                dto.ProvinceName = province.Name;
            }
        }

        return dto;
    }

    protected override async Task<IQueryable<ResWard>> CreateFilteredQueryAsync(GetResWardsInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();

        // Filter by ProvinceId
        if (input.ProvinceId.HasValue)
        {
            query = query.Where(x => x.ProvinceId == input.ProvinceId.Value);
        }

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

        return query;
    }

    /// <summary>
    /// Lấy danh sách phường/xã cho dropdown, có thể lọc theo provinceId. Chỉ cần đăng nhập.
    /// </summary>
    [Authorize]
    public virtual async Task<List<ResWardSelectDto>> GetSelectListAsync(Guid? provinceId = null)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();
        query = query.Where(x => x.Status == ResWardStatus.Active);
        if (provinceId.HasValue && provinceId.Value != Guid.Empty)
        {
            query = query.Where(x => x.ProvinceId == provinceId.Value);
        }
        query = query.OrderBy(x => x.Name);
        var entities = await AsyncExecuter.ToListAsync(query);
        return entities.Select(x => new ResWardSelectDto
        {
            Id = x.Id,
            Code = x.Code ?? string.Empty,
            Name = x.Name ?? string.Empty
        }).ToList();
    }
}

