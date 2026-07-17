using iOne.Customer.Localization;
using iOne.Customer.Permissions;
using iOne.ResCustomers;
using iOne.ResIndustries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
// For Entity, Manager, Repository

namespace iOne.Customer.ResIndustries;

[RemoteService(Name = CustomerRemoteServiceConsts.RemoteServiceName)]
[Authorize(ResIndustryPermissions.Default)]
public class ResIndustryAppService : CrudAppService<
    ResIndustry,
    ResIndustryDto,
    Guid,
    GetResIndustriesInput,
    CreateResIndustryDto,
    UpdateResIndustryDto>, IResIndustryAppService
{
    public ResIndustryAppService(
        IResIndustryRepository repository,
        ResIndustryManager manager,
        IResCustomerRepository customerRepository)
        : base(repository)
    {
        Manager = manager;
        IndustryRepository = repository;
        CustomerRepository = customerRepository;
        LocalizationResource = typeof(CustomerResource);
        GetPolicyName = ResIndustryPermissions.View;
        GetListPolicyName = ResIndustryPermissions.View;
        CreatePolicyName = ResIndustryPermissions.Create;
        UpdatePolicyName = ResIndustryPermissions.Edit;
        DeletePolicyName = ResIndustryPermissions.Delete;
    }

    protected ResIndustryManager Manager { get; }
    protected IResIndustryRepository IndustryRepository { get; }
    protected IResCustomerRepository CustomerRepository { get; }

    public override async Task<ResIndustryDto> CreateAsync(CreateResIndustryDto input)
    {
        // Create Industry entity
        var entity = new ResIndustry(
            GuidGenerator.Create(),
            input.Code,
            input.Name,
            input.Status,
            input.Description
        );

        // Create Industry with validation
        await Manager.CreateAsync(entity);

        // Map to DTO
        return ObjectMapper.Map<ResIndustry, ResIndustryDto>(entity);
    }

    public override async Task<ResIndustryDto> UpdateAsync(Guid id, UpdateResIndustryDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        var entity = await Repository.GetAsync(id, true);

        // Kiểm tra nếu chuyển status về Deactive thì phải kiểm tra có Customer đang sử dụng không
        if (input.Status == ResIndustryStatus.Deactive && entity.Status == ResIndustryStatus.Active)
        {
            if (await CustomerRepository.AnyByIndustryIdAsync(id))
            {
                throw new BusinessException("Customer:ResIndustry:InUseByCustomer")
                    .WithData("Name", entity.Name);
            }
        }

        // Update entity fields (Code is immutable)
        await Manager.UpdateAsync(
            entity,
            input.Name,
            input.Description,
            input.Status
        );

        // Ensure changes are saved
        await CurrentUnitOfWork.SaveChangesAsync();

        // Map to DTO
        return ObjectMapper.Map<ResIndustry, ResIndustryDto>(entity);
    }

    public override async Task DeleteAsync(Guid id)
    {
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id);

        // Kiểm tra có Customer đang sử dụng không
        if (await CustomerRepository.AnyByIndustryIdAsync(id))
        {
            throw new BusinessException("Customer:ResIndustry:InUseByCustomer")
                .WithData("Name", entity.Name);
        }

        // ⚠️ QUAN TRỌNG: Soft delete - xóa trước (ABP audit log) → cập nhật status về Deactive sau
        // 1. Delete để trigger ABP audit log (set IsDeleted = true, DeletionTime, DeleterId)
        await Manager.DeleteAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<ResIndustry>> CreateFilteredQueryAsync(GetResIndustriesInput input)
    {
        var query = (await Repository.GetQueryableAsync())
            .Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(input.Code))
        {
            var code = input.Code.ToLower();
            query = query.Where(x => x.Code.ToLower().Contains(code));
        }

        // Keyword search (server-side): match by Code using LIKE (case-insensitive)
        if (!string.IsNullOrWhiteSpace(input.Keyword))
        {
            var keyword = input.Keyword.Trim().ToLower();
            query = query.Where(x => x.Code != null && x.Code.ToLower().Contains(keyword));
        }

        if (!string.IsNullOrWhiteSpace(input.Name))
        {
            var name = input.Name.ToLower();
            query = query.Where(x => x.Name.ToLower().Contains(name));
        }

        if (input.Status.HasValue)
        {
            query = query.Where(x => x.Status == input.Status.Value);
        }

        return query;
    }
}