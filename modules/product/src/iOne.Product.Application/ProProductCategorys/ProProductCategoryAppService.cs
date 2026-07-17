using System;
using System.Linq;
using System.Threading.Tasks;
using iOne.Product;
using iOne.Product.ProProductCategorys;
using iOne.Product.Localization;
using iOne.Product.Permissions;
using iOne.ProProductCategorys; // For Entity, Manager, Repository
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp;

namespace iOne.Product.ProProductCategorys;

[RemoteService(Name = ProductRemoteServiceConsts.RemoteServiceName)]
[Authorize(ProProductCategoryPermissions.Default)]
public class ProProductCategoryAppService : CrudAppService<
    ProProductCategory,
    ProProductCategoryDto,
    Guid,
    GetProProductCategorysInput,
    CreateProProductCategoryDto,
    UpdateProProductCategoryDto>, IProProductCategoryAppService
{
    protected ProProductCategoryManager Manager { get; }

    public ProProductCategoryAppService(
        IProProductCategoryRepository repository,
        ProProductCategoryManager manager)
        : base(repository)
    {
        Manager = manager;
        LocalizationResource = typeof(ProductResource);
        GetPolicyName = ProProductCategoryPermissions.View;
        GetListPolicyName = ProProductCategoryPermissions.View;
        CreatePolicyName = ProProductCategoryPermissions.Create;
        UpdatePolicyName = ProProductCategoryPermissions.Edit;
        DeletePolicyName = ProProductCategoryPermissions.Delete;
    }

    public override async Task<ProProductCategoryDto> CreateAsync(CreateProProductCategoryDto input)
    {
        var entity = new ProProductCategory(
            GuidGenerator.Create(),
            input.LobId,
            input.Code,
            input.Name,
            input.Status,
            input.ParentId,
            input.Description
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<ProProductCategory, ProProductCategoryDto>(entity);
    }

    public override async Task<ProProductCategoryDto> UpdateAsync(Guid id, UpdateProProductCategoryDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        var entity = await Repository.GetAsync(id);

        await Manager.UpdateAsync(entity, input.LobId, input.Name, input.Status, input.ParentId, input.Description);

        // Ensure changes are saved (Repository.UpdateAsync should handle this, but explicitly save to ensure audit logging)
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<ProProductCategory, ProProductCategoryDto>(entity);
    }

    public override async Task DeleteAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);

        // Check if this category is a parent of other categories
        var repository = (IProProductCategoryRepository)Repository;
        if (await repository.HasChildrenAsync(id))
        {
            throw new BusinessException("ProProductCategory:CannotDeleteParentCategory")
                .WithData("Id", id);
        }

        // Delete to trigger audit logging with ChangeType = Deleted
        // FullAuditedAggregateRoot uses soft delete, so entity is not actually removed from database
        await Repository.DeleteAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();

        // Update status to Deactive (entity still exists in database due to soft delete)
        entity.UpdateStatus(ProProductCategoryStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<ProProductCategory>> CreateFilteredQueryAsync(GetProProductCategorysInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();

        // Filter by LobId
        if (input.LobId.HasValue)
        {
            query = query.Where(x => x.LobId == input.LobId.Value);
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

        // Filter by ParentId
        if (input.ParentId.HasValue)
        {
            query = query.Where(x => x.ParentId == input.ParentId.Value);
        }
        else if (input.ParentId == Guid.Empty)
        {
            // Filter for root items (no parent)
            query = query.Where(x => x.ParentId == null);
        }

        return query;
    }
}
