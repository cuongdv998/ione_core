using System;
using System.Linq;
using System.Threading.Tasks;
using iOne.Partner;
using iOne.Partner.Localization;
using iOne.Partner.Permissions;
using iOne.Partner.ResOrganizationTypes;
using iOne.ResCustomers;
using iOne.ResOrganizationTypes; // For Entity, Manager, Repository
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace iOne.Partner.ResOrganizationTypes;

[RemoteService(Name = PartnerRemoteServiceConsts.RemoteServiceName)]
[Authorize(ResOrganizationTypePermissions.Default)]
public class ResOrganizationTypeAppService : CrudAppService<
    ResOrganizationType,
    ResOrganizationTypeDto,
    Guid,
    GetResOrganizationTypesInput,
    CreateResOrganizationTypeDto,
    UpdateResOrganizationTypeDto>, IResOrganizationTypeAppService
{
    protected ResOrganizationTypeManager Manager { get; }
    protected IResCustomerRepository CustomerRepository { get; }

    public ResOrganizationTypeAppService(
        IResOrganizationTypeRepository repository,
        ResOrganizationTypeManager manager,
        IResCustomerRepository customerRepository)
        : base(repository)
    {
        Manager = manager;
        CustomerRepository = customerRepository;
        LocalizationResource = typeof(PartnerResource);
        GetPolicyName = ResOrganizationTypePermissions.View;
        GetListPolicyName = ResOrganizationTypePermissions.View;
        CreatePolicyName = ResOrganizationTypePermissions.Create;
        UpdatePolicyName = ResOrganizationTypePermissions.Edit;
        DeletePolicyName = ResOrganizationTypePermissions.Delete;
    }

    public override async Task<ResOrganizationTypeDto> CreateAsync(CreateResOrganizationTypeDto input)
    {
        // Convert Code to uppercase (validation in entity will ensure format)
        var entity = new ResOrganizationType(
            GuidGenerator.Create(),
            input.Code.ToUpperInvariant(),
            input.Name,
            input.Status,
            input.Type // Mặc định là TC nếu không truyền
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<ResOrganizationType, ResOrganizationTypeDto>(entity);
    }

    public override async Task<ResOrganizationTypeDto> UpdateAsync(Guid id, UpdateResOrganizationTypeDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id);

        // Kiểm tra nếu chuyển status về Deactive thì phải kiểm tra có Customer đang sử dụng không
        if (input.Status == ResOrganizationTypeStatus.Deactive && entity.Status == ResOrganizationTypeStatus.Active)
        {
            if (await CustomerRepository.AnyByOrganizationTypeIdAsync(id))
            {
                throw new BusinessException("Partner:ResOrganizationType:InUseByCustomer")
                    .WithData("Name", entity.Name);
            }
        }

        // ⚠️ QUAN TRỌNG: Chỉ update Name, Status và Type, không update Code
        await Manager.UpdateAsync(entity, input.Name, input.Status, input.Type);

        // Ensure changes are saved (Repository.UpdateAsync should handle this, but explicitly save to ensure audit logging)
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<ResOrganizationType, ResOrganizationTypeDto>(entity);
    }

    public override async Task DeleteAsync(Guid id)
    {
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id);

        // Kiểm tra có Customer đang sử dụng không
        if (await CustomerRepository.AnyByOrganizationTypeIdAsync(id))
        {
            throw new BusinessException("Partner:ResOrganizationType:InUseByCustomer")
                .WithData("Name", entity.Name);
        }
        
        // Delete to trigger audit logging with ChangeType = Deleted
        // FullAuditedAggregateRoot uses soft delete, so entity is not actually removed from database
        await Repository.DeleteAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
        
        // Update status to Deactive (entity still exists in database due to soft delete)
        entity.UpdateStatus(ResOrganizationTypeStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<ResOrganizationType>> CreateFilteredQueryAsync(GetResOrganizationTypesInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();

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
}

