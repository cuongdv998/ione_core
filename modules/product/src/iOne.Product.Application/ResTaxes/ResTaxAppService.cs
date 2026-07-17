using System;
using System.Linq;
using System.Threading.Tasks;
using iOne.Product;
using iOne.Product.ResTaxes;
using iOne.Product.Localization;
using iOne.Product.Permissions;
using iOne.ResTaxes; // For Entity, Manager, Repository
using iOne.ResFeeItems; // For IResFeeItemRepository
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp;

namespace iOne.Product.ResTaxes;

[RemoteService(Name = ProductRemoteServiceConsts.RemoteServiceName)]
[Authorize(ResTaxPermissions.Default)]
public class ResTaxAppService : CrudAppService<
    ResTax,
    ResTaxDto,
    Guid,
    GetResTaxesInput,
    CreateResTaxDto,
    UpdateResTaxDto>, IResTaxAppService
{
    protected ResTaxManager Manager { get; }
    protected IResFeeItemRepository ResFeeItemRepository { get; }

    public ResTaxAppService(
        IResTaxRepository repository,
        ResTaxManager manager,
        IResFeeItemRepository resFeeItemRepository)
        : base(repository)
    {
        Manager = manager;
        ResFeeItemRepository = resFeeItemRepository;
        LocalizationResource = typeof(ProductResource);
        GetPolicyName = ResTaxPermissions.View;
        GetListPolicyName = ResTaxPermissions.View;
        CreatePolicyName = ResTaxPermissions.Create;
        UpdatePolicyName = ResTaxPermissions.Edit;
        DeletePolicyName = ResTaxPermissions.Delete;
    }

    public override async Task<ResTaxDto> CreateAsync(CreateResTaxDto input)
    {
        var entity = new ResTax(
            GuidGenerator.Create(),
            input.Code,
            input.Name,
            input.Value,
            input.Status,
            input.Description
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<ResTax, ResTaxDto>(entity);
    }

    public override async Task<ResTaxDto> UpdateAsync(Guid id, UpdateResTaxDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        var entity = await Repository.GetAsync(id);

        await Manager.UpdateAsync(entity, input.Name, input.Value, input.Status, input.Description);

        // Ensure changes are saved (Repository.UpdateAsync should handle this, but explicitly save to ensure audit logging)
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<ResTax, ResTaxDto>(entity);
    }

    public override async Task DeleteAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);

        // Check if tax is being used by any ResFeeItem
        var isTaxInUse = await ResFeeItemRepository.IsTaxInUseAsync(id);
        if (isTaxInUse)
        {
            throw new UserFriendlyException(L["ResTax:CannotDeleteInUse"]);
        }

        // Delete to trigger audit logging with ChangeType = Deleted
        // FullAuditedAggregateRoot uses soft delete, so entity is not actually removed from database
        await Repository.DeleteAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();

        // Update status to Deactive (entity still exists in database due to soft delete)
        entity.UpdateStatus(ResTaxStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<ResTax>> CreateFilteredQueryAsync(GetResTaxesInput input)
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

