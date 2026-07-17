using System;
using System.Linq;
using System.Threading.Tasks;
using iOne.Partner.Localization;
using iOne.Partner.ResPartnerTypes;
using iOne.Partner.Permissions;
using iOne.ResPartnerTypes; // For Entity, Manager, Repository
using iOne.ResPartners; // For Partner Repository
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace iOne.Partner.ResPartnerTypes;

[RemoteService(Name = PartnerRemoteServiceConsts.RemoteServiceName)]
[Authorize(ResPartnerTypePermissions.Default)]
public class ResPartnerTypeAppService : CrudAppService<
    ResPartnerType,
    ResPartnerTypeDto,
    Guid,
    GetResPartnerTypesInput,
    CreateResPartnerTypeDto,
    UpdateResPartnerTypeDto>, IResPartnerTypeAppService
{
    protected ResPartnerTypeManager Manager { get; }
    protected IRepository<ResPartner, Guid> PartnerRepository { get; }

    public ResPartnerTypeAppService(
        IResPartnerTypeRepository repository,
        ResPartnerTypeManager manager,
        IRepository<ResPartner, Guid> partnerRepository)
        : base(repository)
    {
        Manager = manager;
        PartnerRepository = partnerRepository;
        LocalizationResource = typeof(PartnerResource);
        GetPolicyName = ResPartnerTypePermissions.View;
        GetListPolicyName = ResPartnerTypePermissions.View;
        CreatePolicyName = ResPartnerTypePermissions.Create;
        UpdatePolicyName = ResPartnerTypePermissions.Edit;
        DeletePolicyName = ResPartnerTypePermissions.Delete;
    }

    public override async Task<ResPartnerTypeDto> CreateAsync(CreateResPartnerTypeDto input)
    {
        var entity = new ResPartnerType(
            GuidGenerator.Create(),
            input.Code,
            input.Name,
            input.Status
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<ResPartnerType, ResPartnerTypeDto>(entity);
    }

    public override async Task<ResPartnerTypeDto> UpdateAsync(Guid id, UpdateResPartnerTypeDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id);

        // ⚠️ QUAN TRỌNG: Chỉ update Name và Status, không update Code
        await Manager.UpdateAsync(entity, input.Name, input.Status);

        // Ensure changes are saved (Repository.UpdateAsync should handle this, but explicitly save to ensure audit logging)
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<ResPartnerType, ResPartnerTypeDto>(entity);
    }

    public override async Task DeleteAsync(Guid id)
    {
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id);
        
        // Check if partner type is being used by any partner
        var isInUse = await PartnerRepository.AnyAsync(x => x.PartnerTypeId == id);
        if (isInUse)
        {
            throw new UserFriendlyException(L["ResPartnerType:InUseByPartner", entity.Name]);
        }
        
        // ⚠️ QUAN TRỌNG: Delete trước để trigger audit logging với ChangeType = Deleted
        // FullAuditedAggregateRoot uses soft delete, so entity is not actually removed from database
        await Repository.DeleteAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
        
        // Sau đó update status to Deactive (entity still exists in database due to soft delete)
        entity.UpdateStatus(ResPartnerTypeStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<ResPartnerType>> CreateFilteredQueryAsync(GetResPartnerTypesInput input)
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

