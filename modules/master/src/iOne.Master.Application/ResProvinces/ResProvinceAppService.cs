using System.Collections.Generic;
using System.Linq;
using iOne.Master;
using iOne.Master.Localization;
using iOne.Master.Permissions;
using iOne.Master.ResProvinces;
using iOne.ResCountries;
using iOne.ResCustomers;
using iOne.ResProvinces;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

using iOne.HrDepartments;
using iOne.HrEmployees;
using iOne.ResPartners;
using iOne.ResWards;

namespace iOne.Master.ResProvinces;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Authorize(ResProvincePermissions.Default)]
public class ResProvinceAppService : CrudAppService<
    ResProvince,
    ResProvinceDto,
    Guid,
    GetResProvincesInput,
    CreateResProvinceDto,
    UpdateResProvinceDto>,
    IResProvinceAppService
{
    protected ResProvinceManager Manager { get; }
    protected IResCountryRepository CountryRepository { get; }
    protected IResCustomerRepository CustomerRepository { get; }
    protected IResWardRepository WardRepository { get; }
    protected IResPartnerRepository PartnerRepository { get; }
    protected IHrDepartmentRepository DepartmentRepository { get; }
    protected IHrEmployeeRepository EmployeeRepository { get; }

    public ResProvinceAppService(
        IResProvinceRepository repository,
        ResProvinceManager manager,
        IResCountryRepository countryRepository,
        IResCustomerRepository customerRepository,
        IResWardRepository wardRepository,
        IResPartnerRepository partnerRepository,
        IHrDepartmentRepository departmentRepository,
        IHrEmployeeRepository employeeRepository)
        : base(repository)
    {
        Manager = manager;
        CountryRepository = countryRepository;
        CustomerRepository = customerRepository;
        WardRepository = wardRepository;
        PartnerRepository = partnerRepository;
        DepartmentRepository = departmentRepository;
        EmployeeRepository = employeeRepository;
        LocalizationResource = typeof(MasterResource);
        GetPolicyName = ResProvincePermissions.View;
        GetListPolicyName = ResProvincePermissions.View;
        CreatePolicyName = ResProvincePermissions.Create;
        UpdatePolicyName = ResProvincePermissions.Edit;
        DeletePolicyName = ResProvincePermissions.Delete;
    }

    public override async Task<ResProvinceDto> CreateAsync(CreateResProvinceDto input)
    {
        var entity = new ResProvince(
            GuidGenerator.Create(),
            input.CountryId,
            input.Code,
            input.Name,
            input.Status,
            input.Description
        );

        await Manager.CreateAsync(entity);

        return await MapToDtoWithCountryNameAsync(entity);
    }

    public override async Task<ResProvinceDto> UpdateAsync(Guid id, UpdateResProvinceDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        var entity = await Repository.GetAsync(id);

        // Kiểm tra nếu chuyển status về Deactive thì phải kiểm tra ràng buộc
        if (input.Status == ResProvinceStatus.Deactive && entity.Status == ResProvinceStatus.Active)
        {
            await CheckConstraintsAsync(id, entity.Name);
        }

        // ⚠️ QUAN TRỌNG: Chỉ update CountryId, Name, Status, Description - không update Code
        await Manager.UpdateAsync(
            entity,
            input.CountryId,
            input.Name,
            input.Status,
            input.Description
        );

        // Ensure changes are saved
        await CurrentUnitOfWork.SaveChangesAsync();

        return await MapToDtoWithCountryNameAsync(entity);
    }

    public override async Task<ResProvinceDto> GetAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);
        return await MapToDtoWithCountryNameAsync(entity);
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
        entity.UpdateStatus(ResProvinceStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected virtual async Task CheckConstraintsAsync(Guid id, string name)
    {
        // 1. Kiểm tra có Phường/Xã thuộc Tỉnh/Thành này không
        if (await WardRepository.AnyByProvinceIdAsync(id))
        {
            throw new BusinessException("Master:ResProvince:InUseByWard")
                .WithData("Name", name);
        }

        // 2. Kiểm tra có Customer đang sử dụng không
        if (await CustomerRepository.AnyByProvinceIdAsync(id))
        {
            throw new BusinessException("Master:ResProvince:InUseByCustomer")
                .WithData("Name", name);
        }

        // 3. Kiểm tra có Partner đang sử dụng không
        if (await PartnerRepository.AnyByProvinceIdAsync(id))
        {
            throw new BusinessException("Master:ResProvince:InUseByPartner")
                .WithData("Name", name);
        }

        // 4. Kiểm tra có Đơn vị/Phòng ban đang sử dụng không
        if (await DepartmentRepository.AnyByProvinceIdAsync(id))
        {
            throw new BusinessException("Master:ResProvince:InUseByDepartment")
                .WithData("Name", name);
        }

        // 5. Kiểm tra có Nhân viên đang sử dụng không
        if (await EmployeeRepository.AnyByProvinceIdAsync(id))
        {
            throw new BusinessException("Master:ResProvince:InUseByEmployee")
                .WithData("Name", name);
        }
    }

    protected async Task<ResProvinceDto> MapToDtoWithCountryNameAsync(ResProvince entity)
    {
        var dto = ObjectMapper.Map<ResProvince, ResProvinceDto>(entity);

        // Load country name if needed
        if (entity.CountryId != Guid.Empty)
        {
            // If Country navigation property is already loaded, use it
            if (entity.Country != null)
            {
                dto.CountryName = entity.Country.Name;
            }
            else
            {
                // Otherwise, load from repository
                var country = await CountryRepository.GetAsync(entity.CountryId);
                dto.CountryName = country.Name;
            }
        }

        return dto;
    }

    public override async Task<PagedResultDto<ResProvinceDto>> GetListAsync(GetResProvincesInput input)
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
        var dtos = ObjectMapper.Map<List<ResProvince>, List<ResProvinceDto>>(entities);

        // N+1 issue fix: Lấy danh sách CountryId
        var countryIds = entities.Select(x => x.CountryId).Distinct().ToList();
        
        if (countryIds.Count > 0)
        {
            var countries = await CountryRepository.GetListAsync(x => countryIds.Contains(x.Id));
            var countryDict = countries.ToDictionary(x => x.Id, x => x.Name);

            for (int i = 0; i < entities.Count; i++)
            {
                if (countryDict.TryGetValue(entities[i].CountryId, out var countryName))
                {
                    dtos[i].CountryName = countryName;
                }
            }
        }

        return new PagedResultDto<ResProvinceDto>(totalCount, dtos);
    }

    protected override async Task<IQueryable<ResProvince>> CreateFilteredQueryAsync(GetResProvincesInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();

        // Filter by CountryId
        if (input.CountryId.HasValue)
        {
            query = query.Where(x => x.CountryId == input.CountryId.Value);
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
    /// Lấy danh sách tỉnh/thành cho dropdown. Chỉ cần đăng nhập.
    /// </summary>
    [Authorize]
    public virtual async Task<List<ResProvinceSelectDto>> GetSelectListAsync()
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();
        query = query.Where(x => x.Status == ResProvinceStatus.Active);
        query = query.OrderBy(x => x.Name);
        var entities = await AsyncExecuter.ToListAsync(query);
        return entities.Select(x => new ResProvinceSelectDto
        {
            Id = x.Id,
            Code = x.Code ?? string.Empty,
            Name = x.Name ?? string.Empty
        }).ToList();
    }
}

