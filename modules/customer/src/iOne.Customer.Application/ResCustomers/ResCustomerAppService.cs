using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iOne.Customer;
using iOne.Customer.Localization;
using iOne.Customer.Permissions;
using iOne.Customer.ResCustomers;
using iOne.ResCustomers;
using iOne.ResIndustries;
using iOne.ResProvinces;
using iOne.ResWards;
using iOne.ResOrganizationTypes;
using iOne.HrEmployees;
using iOne.Master.ResSequences;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace iOne.Customer.ResCustomers;

[RemoteService(Name = CustomerRemoteServiceConsts.RemoteServiceName)]
[Authorize(ResCustomerPermissions.Default)]
public class ResCustomerAppService : CrudAppService<
    ResCustomer,
    ResCustomerDto,
    Guid,
    GetResCustomersInput,
    CreateResCustomerDto,
    UpdateResCustomerDto>, IResCustomerAppService
{
    protected IResCustomerRepository CustomerRepository { get; }
    protected IRepository<ResIndustry, Guid> IndustryRepository { get; }
    protected IRepository<ResProvince, Guid> ProvinceRepository { get; }
    protected IRepository<ResWard, Guid> WardRepository { get; }
    protected IRepository<ResOrganizationType, Guid> OrganizationTypeRepository { get; }
    protected IRepository<HrEmployee, Guid> HrEmployeeRepository { get; }
    protected IResSequenceAppService SequenceAppService { get; }

    public ResCustomerAppService(
        IResCustomerRepository repository,
        IRepository<ResIndustry, Guid> industryRepository,
        IRepository<ResProvince, Guid> provinceRepository,
        IRepository<ResWard, Guid> wardRepository,
        IRepository<ResOrganizationType, Guid> organizationTypeRepository,
        IRepository<HrEmployee, Guid> hrEmployeeRepository,
        IResSequenceAppService sequenceAppService)
        : base(repository)
    {
        CustomerRepository = repository;
        IndustryRepository = industryRepository;
        ProvinceRepository = provinceRepository;
        WardRepository = wardRepository;
        OrganizationTypeRepository = organizationTypeRepository;
        HrEmployeeRepository = hrEmployeeRepository;
        SequenceAppService = sequenceAppService;
        LocalizationResource = typeof(CustomerResource);
        GetPolicyName = ResCustomerPermissions.View;
        GetListPolicyName = ResCustomerPermissions.View;
        CreatePolicyName = ResCustomerPermissions.Create;
        UpdatePolicyName = ResCustomerPermissions.Edit;
        DeletePolicyName = ResCustomerPermissions.Delete;
    }

    public override async Task<ResCustomerDto> CreateAsync(CreateResCustomerDto input)
    {
        // Validation & Auto Generation: Code unique (nếu có)
        if (!string.IsNullOrWhiteSpace(input.Code))
        {
            if (await CustomerRepository.AnyAsync(x => x.Code == input.Code))
            {
                throw new BusinessException("Customer:ResCustomer:CodeExists")
                    .WithData("Code", input.Code);
            }
        }
        else
        {
            // Tự động sinh mã khách hàng theo cấu hình CUSTOMER_SEQ
            var sequenceOutput = await SequenceAppService.GetNextSequenceAsync(new GetNextSequenceInput
            {
                Code = "CUSTOMER_SEQ"
            });
            var generatedCode = sequenceOutput.GeneratedCode;

            if (!string.IsNullOrWhiteSpace(generatedCode) && generatedCode.Length > 4)
            {
                var prefix = generatedCode.Substring(0, 4);
                var numberPart = generatedCode.Substring(4);

                if (int.TryParse(numberPart, out int number))
                {
                    input.Code = $"{prefix}{number:D7}";
                }
                else
                {
                    input.Code = generatedCode;
                }
            }
            else
            {
                input.Code = generatedCode;
            }
        }

        // Load navigation properties để compute FullAddress (tỉnh/phường tùy chọn)
        ResProvince? province = null;
        ResWard? ward = null;
        if (input.ProvinceId.HasValue)
        {
            province = await ProvinceRepository.GetAsync(input.ProvinceId.Value);
        }

        if (input.WardId.HasValue)
        {
            ward = await WardRepository.GetAsync(input.WardId.Value);
        }

        // Create entity
        var entity = new ResCustomer(
            GuidGenerator.Create(),
            input.Code,
            input.Name,
            input.ProvinceId,
            input.WardId,
            input.Address,
            input.Phone,
            input.Status,
            input.IndustryId,
            input.Email,
            input.Note,
            input.OrganizationTypeId,
            input.InvoiceProvinceId,
            input.InvoiceWardId,
            input.InvoiceAddress,
            input.SaleId,
            input.Tin,
            input.IdNo,
            input.PassportNo,
            input.Dob,
            input.Sex,
            input.RepName,
            input.RepEmail,
            input.RepPhone,
            input.RepIdNo,
            input.RepTitle,
            input.Authorizer,
            input.AuthorizerPhone,
            input.AuthorizerEmail,
            input.AuthorizerNo,
            input.AuthorizerDate,
            input.AuthorizerTitle,
            input.BusinessNo,
            input.RefCode
        );

        // Tự động tính FullAddress
        entity.UpdateFullAddress(ward?.Name, province?.Name);

        // Tự động tính InvoiceFullAddress (nếu có)
        if (input.InvoiceProvinceId.HasValue && input.InvoiceWardId.HasValue && !string.IsNullOrWhiteSpace(input.InvoiceAddress))
        {
            var invoiceProvince = await ProvinceRepository.GetAsync(input.InvoiceProvinceId.Value);
            var invoiceWard = await WardRepository.GetAsync(input.InvoiceWardId.Value);
            entity.UpdateInvoiceFullAddress(invoiceWard.Name, invoiceProvince.Name);
        }

        await CustomerRepository.InsertAsync(entity);

        return await MapToDtoWithNavigationPropertiesAsync(entity);
    }

    public override async Task<ResCustomerDto> UpdateAsync(Guid id, UpdateResCustomerDto input)
    {
        // Load entity with tracking
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await CustomerRepository.GetAsync(id);

        // Load navigation properties để compute FullAddress (tỉnh/phường tùy chọn)
        ResProvince? province = null;
        ResWard? ward = null;
        if (input.ProvinceId.HasValue)
        {
            province = await ProvinceRepository.GetAsync(input.ProvinceId.Value);
        }

        if (input.WardId.HasValue)
        {
            ward = await WardRepository.GetAsync(input.WardId.Value);
        }

        // Update entity fields (Code is immutable)
        entity.UpdateRefCode(input.RefCode);
        entity.UpdateName(input.Name);
        entity.UpdateStatus(input.Status);
        entity.UpdateIndustryId(input.IndustryId);
        entity.UpdateProvinceId(input.ProvinceId);
        entity.UpdateWardId(input.WardId);
        entity.UpdateAddress(input.Address);
        entity.UpdatePhone(input.Phone);
        entity.UpdateEmail(input.Email);
        entity.UpdateNote(input.Note);
        entity.UpdateOrganizationTypeId(input.OrganizationTypeId);
        entity.UpdateSaleId(input.SaleId);
        entity.UpdateTin(input.Tin);
        entity.UpdateIdNo(input.IdNo);
        entity.UpdatePassportNo(input.PassportNo);
        entity.UpdateDob(input.Dob);
        entity.UpdateSex(input.Sex);
        entity.UpdateRepName(input.RepName);
        entity.UpdateRepEmail(input.RepEmail);
        entity.UpdateRepPhone(input.RepPhone);
        entity.UpdateRepIdNo(input.RepIdNo);
        entity.UpdateRepTitle(input.RepTitle);
        entity.UpdateAuthorizer(input.Authorizer);
        entity.UpdateAuthorizerPhone(input.AuthorizerPhone);
        entity.UpdateAuthorizerEmail(input.AuthorizerEmail);
        entity.UpdateAuthorizerNo(input.AuthorizerNo);
        entity.UpdateAuthorizerDate(input.AuthorizerDate);
        entity.UpdateAuthorizerTitle(input.AuthorizerTitle);
        entity.UpdateBusinessNo(input.BusinessNo);

        // Tự động tính FullAddress
        entity.UpdateFullAddress(ward?.Name, province?.Name);

        // Tự động tính InvoiceFullAddress (nếu có)
        if (input.InvoiceProvinceId.HasValue && input.InvoiceWardId.HasValue && !string.IsNullOrWhiteSpace(input.InvoiceAddress))
        {
            // Repository.GetAsync will throw exception if not found, never returns null
            var invoiceProvince = await ProvinceRepository.GetAsync(input.InvoiceProvinceId.Value);
            var invoiceWard = await WardRepository.GetAsync(input.InvoiceWardId.Value);
            entity.UpdateInvoiceFullAddress(invoiceWard.Name, invoiceProvince.Name);
            entity.UpdateInvoiceAddress(input.InvoiceAddress);
            entity.UpdateInvoiceProvinceId(input.InvoiceProvinceId.Value);
            entity.UpdateInvoiceWardId(input.InvoiceWardId.Value);
        }
        else
        {
            entity.UpdateInvoiceProvinceId(input.InvoiceProvinceId);
            entity.UpdateInvoiceWardId(input.InvoiceWardId);
            entity.UpdateInvoiceAddress(input.InvoiceAddress);
            entity.UpdateInvoiceFullAddress(null, null);
        }

        await CustomerRepository.UpdateAsync(entity);

        return await MapToDtoWithNavigationPropertiesAsync(entity);
    }

    public override async Task<ResCustomerDto> GetAsync(Guid id)
    {
        var entity = await CustomerRepository.GetAsync(id);
        return await MapToDtoWithNavigationPropertiesAsync(entity);
    }

    protected override async Task<IQueryable<ResCustomer>> CreateFilteredQueryAsync(GetResCustomersInput input)
    {
        var query = (await CustomerRepository.GetQueryableAsync())
            .Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            query = query.Where(x =>
                (x.Code != null && EF.Functions.ILike(x.Code, $"%{input.Filter}%")) ||
                (x.RefCode != null && EF.Functions.ILike(x.RefCode, $"%{input.Filter}%")) ||
                EF.Functions.ILike(x.Name, $"%{input.Filter}%") ||
                (x.Phone != null && EF.Functions.ILike(x.Phone, $"%{input.Filter}%")) ||
                (x.Email != null && EF.Functions.ILike(x.Email, $"%{input.Filter}%")) ||
                (x.IdNo != null && EF.Functions.ILike(x.IdNo, $"%{input.Filter}%")) ||
                (x.Address != null && EF.Functions.ILike(x.Address, $"%{input.Filter}%")) ||
                (x.FullAddress != null && EF.Functions.ILike(x.FullAddress, $"%{input.Filter}%")));
        }

        // Keyword: tìm theo tên khách hàng (contains, không phân biệt hoa thường)
        if (!string.IsNullOrWhiteSpace(input.Keyword))
        {
            var keyword = input.Keyword.Trim();
            query = query.Where(x => EF.Functions.ILike(x.Name, $"%{keyword}%"));
        }

        // Filter by Code (case-insensitive, contains)
        if (!string.IsNullOrWhiteSpace(input.Code))
        {
            query = query.Where(x => x.Code != null && EF.Functions.ILike(x.Code, $"%{input.Code}%"));
        }

        // Filter by Name (case-insensitive, contains)
        if (!string.IsNullOrWhiteSpace(input.Name))
        {
            query = query.Where(x => EF.Functions.ILike(x.Name, $"%{input.Name}%"));
        }

        if (input.IndustryId.HasValue)
        {
            query = query.Where(x => x.IndustryId == input.IndustryId.Value);
        }

        if (input.ProvinceId.HasValue)
        {
            query = query.Where(x => x.ProvinceId == input.ProvinceId.Value);
        }

        if (input.WardId.HasValue)
        {
            query = query.Where(x => x.WardId == input.WardId.Value);
        }

        if (input.OrganizationTypeId.HasValue)
        {
            query = query.Where(x => x.OrganizationTypeId == input.OrganizationTypeId.Value);
        }

        if (input.SaleId.HasValue)
        {
            query = query.Where(x => x.SaleId == input.SaleId.Value);
        }

        if (input.Status.HasValue)
        {
            query = query.Where(x => x.Status == input.Status.Value);
        }

        return query;
    }

    protected override async Task<ResCustomerDto> MapToGetOutputDtoAsync(ResCustomer entity)
    {
        return await MapToDtoWithNavigationPropertiesAsync(entity);
    }

    public override async Task<PagedResultDto<ResCustomerDto>> GetListAsync(GetResCustomersInput input)
    {
        await CheckGetListPolicyAsync();
        var query = await CreateFilteredQueryAsync(input);
        var totalCount = await AsyncExecuter.CountAsync(query);
        query = ApplySorting(query, input);
        var entities = await AsyncExecuter.ToListAsync(
            query.Skip(input.SkipCount).Take(input.MaxResultCount));

        if (entities.Count == 0)
            return new PagedResultDto<ResCustomerDto>(totalCount, new List<ResCustomerDto>());

        // Batch-load navigation data để tránh N+1 (mỗi dòng không còn gọi 7 lần GetAsync)
        var industryIds = entities.Where(e => e.IndustryId.HasValue).Select(e => e.IndustryId!.Value).Distinct().ToList();
        var provinceIds = entities.Where(e => e.ProvinceId.HasValue).Select(e => e.ProvinceId!.Value).Distinct().ToList();
        var wardIds = entities.Where(e => e.WardId.HasValue).Select(e => e.WardId!.Value).Distinct().ToList();
        var orgTypeIds = entities.Where(e => e.OrganizationTypeId.HasValue).Select(e => e.OrganizationTypeId!.Value).Distinct().ToList();
        var invoiceProvinceIds = entities.Where(e => e.InvoiceProvinceId.HasValue).Select(e => e.InvoiceProvinceId!.Value).Distinct().ToList();
        var invoiceWardIds = entities.Where(e => e.InvoiceWardId.HasValue).Select(e => e.InvoiceWardId!.Value).Distinct().ToList();
        var saleIds = entities.Where(e => e.SaleId.HasValue).Select(e => e.SaleId!.Value).Distinct().ToList();

        var industryById = new Dictionary<Guid, ResIndustry>();
        if (industryIds.Count > 0)
        {
            var indQuery = await IndustryRepository.GetQueryableAsync();
            var industries = await AsyncExecuter.ToListAsync(indQuery.Where(x => industryIds.Contains(x.Id)));
            industryById = industries.ToDictionary(x => x.Id);
        }
        var provinceById = new Dictionary<Guid, ResProvince>();
        if (provinceIds.Count > 0 || invoiceProvinceIds.Count > 0)
        {
            var allProvinceIds = provinceIds.Union(invoiceProvinceIds).Distinct().ToList();
            var provQuery = await ProvinceRepository.GetQueryableAsync();
            var provinces = await AsyncExecuter.ToListAsync(provQuery.Where(x => allProvinceIds.Contains(x.Id)));
            provinceById = provinces.ToDictionary(x => x.Id);
        }
        var wardById = new Dictionary<Guid, ResWard>();
        if (wardIds.Count > 0 || invoiceWardIds.Count > 0)
        {
            var allWardIds = wardIds.Union(invoiceWardIds).Distinct().ToList();
            var wardQuery = await WardRepository.GetQueryableAsync();
            var wards = await AsyncExecuter.ToListAsync(wardQuery.Where(x => allWardIds.Contains(x.Id)));
            wardById = wards.ToDictionary(x => x.Id);
        }
        var orgTypeById = new Dictionary<Guid, ResOrganizationType>();
        if (orgTypeIds.Count > 0)
        {
            var otQuery = await OrganizationTypeRepository.GetQueryableAsync();
            var orgTypes = await AsyncExecuter.ToListAsync(otQuery.Where(x => orgTypeIds.Contains(x.Id)));
            orgTypeById = orgTypes.ToDictionary(x => x.Id);
        }
        var saleById = new Dictionary<Guid, HrEmployee>();
        if (saleIds.Count > 0)
        {
            var empQuery = await HrEmployeeRepository.GetQueryableAsync();
            var employees = await AsyncExecuter.ToListAsync(empQuery.Where(x => saleIds.Contains(x.Id)));
            saleById = employees.ToDictionary(x => x.Id);
        }

        var dtos = entities.Select(e => MapToDtoWithLookups(e, industryById, provinceById, wardById, orgTypeById, saleById)).ToList();
        return new PagedResultDto<ResCustomerDto>(totalCount, dtos);
    }

    protected override async Task<ResCustomerDto> MapToGetListOutputDtoAsync(ResCustomer entity)
    {
        return await MapToDtoWithNavigationPropertiesAsync(entity);
    }

    private static IQueryable<ResCustomer> ApplySorting(IQueryable<ResCustomer> query, GetResCustomersInput input)
    {
        var sorting = input?.Sorting?.Trim();
        if (!string.IsNullOrWhiteSpace(sorting))
        {
            var sortingLower = sorting.ToLowerInvariant();
            if (sortingLower.StartsWith("name"))
            {
                var isDesc = sortingLower.Contains("desc");
                return isDesc
                    ? query.OrderByDescending(x => x.Name ?? string.Empty).ThenBy(x => x.Code ?? string.Empty)
                    : query.OrderBy(x => x.Name ?? string.Empty).ThenBy(x => x.Code ?? string.Empty);
            }
        }

        return query.OrderByDescending(x => x.CreationTime);
    }

    private static ResCustomerDto MapToDtoWithLookups(
        ResCustomer entity,
        Dictionary<Guid, ResIndustry> industryById,
        Dictionary<Guid, ResProvince> provinceById,
        Dictionary<Guid, ResWard> wardById,
        Dictionary<Guid, ResOrganizationType> orgTypeById,
        Dictionary<Guid, HrEmployee> saleById)
    {
        var dto = new ResCustomerDto
        {
            Id = entity.Id,
            Code = entity.Code,
            RefCode = entity.RefCode,
            Name = entity.Name,
            ProvinceId = entity.ProvinceId,
            WardId = entity.WardId,
            IndustryId = entity.IndustryId,
            Address = entity.Address,
            FullAddress = entity.FullAddress,
            Email = entity.Email,
            Phone = entity.Phone,
            Note = entity.Note,
            Status = entity.Status,
            Tin = entity.Tin,
            IdNo = entity.IdNo,
            PassportNo = entity.PassportNo,
            Dob = entity.Dob,
            Sex = entity.Sex,
            OrganizationTypeId = entity.OrganizationTypeId,
            InvoiceProvinceId = entity.InvoiceProvinceId,
            InvoiceWardId = entity.InvoiceWardId,
            InvoiceAddress = entity.InvoiceAddress,
            InvoiceFullAddress = entity.InvoiceFullAddress,
            SaleId = entity.SaleId,
            RepName = entity.RepName,
            RepEmail = entity.RepEmail,
            RepPhone = entity.RepPhone,
            RepIdNo = entity.RepIdNo,
            RepTitle = entity.RepTitle,
            Authorizer = entity.Authorizer,
            AuthorizerPhone = entity.AuthorizerPhone,
            AuthorizerEmail = entity.AuthorizerEmail,
            AuthorizerNo = entity.AuthorizerNo,
            AuthorizerDate = entity.AuthorizerDate,
            AuthorizerTitle = entity.AuthorizerTitle,
            BusinessNo = entity.BusinessNo,
            CreationTime = entity.CreationTime,
            CreatorId = entity.CreatorId,
            LastModificationTime = entity.LastModificationTime,
            LastModifierId = entity.LastModifierId
        };
        if (entity.IndustryId.HasValue && industryById.TryGetValue(entity.IndustryId.Value, out var industry))
            dto.IndustryName = industry.Name;
        if (entity.ProvinceId.HasValue && provinceById.TryGetValue(entity.ProvinceId.Value, out var province))
            dto.ProvinceName = province.Name;
        if (entity.WardId.HasValue && wardById.TryGetValue(entity.WardId.Value, out var ward))
            dto.WardName = ward.Name;
        if (entity.OrganizationTypeId.HasValue && orgTypeById.TryGetValue(entity.OrganizationTypeId.Value, out var orgType))
        {
            dto.OrganizationTypeName = orgType.Name;
            dto.OrganizationTypeType = orgType.Type.ToString();
        }
        if (entity.InvoiceProvinceId.HasValue && provinceById.TryGetValue(entity.InvoiceProvinceId.Value, out var invProvince))
            dto.InvoiceProvinceName = invProvince.Name;
        if (entity.InvoiceWardId.HasValue && wardById.TryGetValue(entity.InvoiceWardId.Value, out var invWard))
            dto.InvoiceWardName = invWard.Name;
        if (entity.SaleId.HasValue && saleById.TryGetValue(entity.SaleId.Value, out var sale))
            dto.SaleName = sale.FullName;
        return dto;
    }

    public async Task<List<ResCustomerDto>> GetListByIdsAsync(List<Guid> ids)
    {
        if (ids == null || ids.Count == 0)
        {
            return new List<ResCustomerDto>();
        }

        var query = (await CustomerRepository.GetQueryableAsync())
            .Where(x => ids.Contains(x.Id) && !x.IsDeleted);

        var entities = await AsyncExecuter.ToListAsync(query);

        var dtos = new List<ResCustomerDto>();
        foreach (var entity in entities)
        {
            dtos.Add(await MapToDtoWithNavigationPropertiesAsync(entity));
        }

        return dtos;
    }

    private async Task<ResCustomerDto> MapToDtoWithNavigationPropertiesAsync(ResCustomer entity)
    {
        var dto = ObjectMapper.Map<ResCustomer, ResCustomerDto>(entity);

        // Load navigation properties để lấy tên
        // Repository.GetAsync will throw exception if not found, never returns null
        if (entity.IndustryId.HasValue)
        {
            var industry = await IndustryRepository.GetAsync(entity.IndustryId.Value);
            dto.IndustryName = industry.Name;
        }

        if (entity.ProvinceId.HasValue)
        {
            var province = await ProvinceRepository.GetAsync(entity.ProvinceId.Value);
            dto.ProvinceName = province.Name;
        }

        if (entity.WardId.HasValue)
        {
            var ward = await WardRepository.GetAsync(entity.WardId.Value);
            dto.WardName = ward.Name;
        }

        if (entity.OrganizationTypeId.HasValue)
        {
            var organizationType = await OrganizationTypeRepository.GetAsync(entity.OrganizationTypeId.Value);
            dto.OrganizationTypeName = organizationType.Name;
            dto.OrganizationTypeType = organizationType.Type.ToString(); // "CN" hoặc "TC"
        }

        if (entity.InvoiceProvinceId.HasValue)
        {
            var invoiceProvince = await ProvinceRepository.GetAsync(entity.InvoiceProvinceId.Value);
            dto.InvoiceProvinceName = invoiceProvince.Name;
        }

        if (entity.InvoiceWardId.HasValue)
        {
            var invoiceWard = await WardRepository.GetAsync(entity.InvoiceWardId.Value);
            dto.InvoiceWardName = invoiceWard.Name;
        }

        if (entity.SaleId.HasValue)
        {
            var sale = await HrEmployeeRepository.GetAsync(entity.SaleId.Value);
            dto.SaleName = sale.FullName;
        }

        return dto;
    }
}

