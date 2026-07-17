using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iOne.Master;
using iOne.Master.Localization;
using iOne.Master.Permissions;
using iOne.Master.ResPaymentMethods;
using iOne.ResPaymentMethods; // For Entity, Manager, Repository
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResPaymentMethods;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Authorize(ResPaymentMethodPermissions.Default)]
public class ResPaymentMethodAppService : CrudAppService<
    ResPaymentMethod,
    ResPaymentMethodDto,
    Guid,
    GetResPaymentMethodsInput,
    CreateResPaymentMethodDto,
    UpdateResPaymentMethodDto>, IResPaymentMethodAppService
{
    protected ResPaymentMethodManager Manager { get; }
    protected IResPaymentMethodRepository PaymentMethodRepository { get; }

    public ResPaymentMethodAppService(
        IResPaymentMethodRepository repository,
        ResPaymentMethodManager manager)
        : base(repository)
    {
        Manager = manager;
        PaymentMethodRepository = repository;
        LocalizationResource = typeof(MasterResource);
        GetPolicyName = ResPaymentMethodPermissions.View;
        GetListPolicyName = ResPaymentMethodPermissions.View;
        CreatePolicyName = ResPaymentMethodPermissions.Create;
        UpdatePolicyName = ResPaymentMethodPermissions.Edit;
        DeletePolicyName = ResPaymentMethodPermissions.Delete;
    }

    public override async Task<ResPaymentMethodDto> CreateAsync(CreateResPaymentMethodDto input)
    {
        // Validate duplicate code
        var normalizedCode = input.Code?.Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(normalizedCode))
        {
            if (await PaymentMethodRepository.IsCodeExistsAsync(normalizedCode))
            {
                throw new UserFriendlyException(
                    L["ResPaymentMethod:CodeExists"].Value.Replace("{Code}", normalizedCode)
                );
            }
        }

        var entity = new ResPaymentMethod(
            GuidGenerator.Create(),
            input.Code,
            input.Name,
            input.Status
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<ResPaymentMethod, ResPaymentMethodDto>(entity);
    }

    public override async Task<ResPaymentMethodDto> UpdateAsync(Guid id, UpdateResPaymentMethodDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id);

        // ⚠️ QUAN TRỌNG: Chỉ update Name và Status, không update Code
        // Code is immutable, so no duplicate validation needed
        await Manager.UpdateAsync(entity, input.Name, input.Status);

        // Ensure changes are saved (Repository.UpdateAsync should handle this, but explicitly save to ensure audit logging)
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<ResPaymentMethod, ResPaymentMethodDto>(entity!);
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
        entity!.UpdateStatus(ResPaymentMethodStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<ResPaymentMethod>> CreateFilteredQueryAsync(GetResPaymentMethodsInput input)
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

    [Authorize]
    public virtual async Task<List<ResPaymentMethodSelectDto>> GetSelectListAsync()
    {
        var query = (await ReadOnlyRepository.GetQueryableAsync())
            .Where(x => !x.IsDeleted && x.Status == ResPaymentMethodStatus.Active)
            .OrderBy(x => x.Name);
        var entities = await AsyncExecuter.ToListAsync(query);
        return entities.Select(x => new ResPaymentMethodSelectDto
        {
            Id = x.Id,
            Code = x.Code ?? string.Empty,
            Name = x.Name ?? string.Empty
        }).ToList();
    }
}
