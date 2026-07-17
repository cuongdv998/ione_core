using System;
using System.Linq;
using System.Threading.Tasks;
using iOne.Master;
using iOne.Master.Localization;
using iOne.Master.Permissions;
using iOne.Master.ResFeeItems;
using iOne.ResFeeItems; // For Entity, Manager, Repository
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResFeeItems;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Authorize(ResFeeItemPermissions.Default)]
public class ResFeeItemAppService : CrudAppService<
    ResFeeItem,
    ResFeeItemDto,
    Guid,
    GetResFeeItemsInput,
    CreateResFeeItemDto,
    UpdateResFeeItemDto>, IResFeeItemAppService
{
    protected ResFeeItemManager Manager { get; }
    protected IResFeeItemRepository FeeItemRepository { get; }

    public ResFeeItemAppService(
        IResFeeItemRepository repository,
        ResFeeItemManager manager)
        : base(repository)
    {
        Manager = manager;
        FeeItemRepository = repository;
        LocalizationResource = typeof(MasterResource);
        GetPolicyName = ResFeeItemPermissions.View;
        GetListPolicyName = ResFeeItemPermissions.View;
        CreatePolicyName = ResFeeItemPermissions.Create;
        UpdatePolicyName = ResFeeItemPermissions.Edit;
        DeletePolicyName = ResFeeItemPermissions.Delete;
    }

    public override async Task<ResFeeItemDto> CreateAsync(CreateResFeeItemDto input)
    {
        // Validate duplicate code
        var normalizedCode = input.Code?.Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(normalizedCode))
        {
            if (await FeeItemRepository.IsCodeExistsAsync(normalizedCode))
            {
                throw new UserFriendlyException(
                    L["ResFeeItem:CodeExists"].Value.Replace("{Code}", normalizedCode)
                );
                //throw new BusinessException("Master:ResFeeItem:CodeExists")
                //    .WithData("Code", normalizedCode);
            }
        }

        var entity = new ResFeeItem(
            GuidGenerator.Create(),
            input.Code,
            input.Name,
            input.Description,
            input.Status,
            input.TaxId
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<ResFeeItem, ResFeeItemDto>(entity);
    }

    public override async Task<ResFeeItemDto> UpdateAsync(Guid id, UpdateResFeeItemDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id);

        // ⚠️ QUAN TRỌNG: Chỉ update Name, Description, Status và TaxId, không update Code
        // Code is immutable, so no duplicate validation needed
        await Manager.UpdateAsync(entity, input.Name, input.Description, input.Status, input.TaxId);

        // Ensure changes are saved (Repository.UpdateAsync should handle this, but explicitly save to ensure audit logging)
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<ResFeeItem, ResFeeItemDto>(entity!);
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
        entity!.UpdateStatus(ResFeeItemStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<ResFeeItem>> CreateFilteredQueryAsync(GetResFeeItemsInput input)
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
