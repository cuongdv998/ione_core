using System;
using System.Linq;
using System.Threading.Tasks;
using iOne.Product;
using iOne.Product.ProProductTypes;
using iOne.Product.Localization;
using iOne.Product.Permissions;
using iOne.ProProductTypes; // For Entity, Manager, Repository
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp;

namespace iOne.Product.ProProductTypes;

[RemoteService(Name = ProductRemoteServiceConsts.RemoteServiceName)]
[Authorize(ProProductTypePermissions.Default)]
public class ProProductTypeAppService : CrudAppService<
    ProProductType,
    ProProductTypeDto,
    Guid,
    GetProProductTypesInput,
    CreateProProductTypeDto,
    UpdateProProductTypeDto>, IProProductTypeAppService
{
    protected ProProductTypeManager Manager { get; }

    public ProProductTypeAppService(
        IProProductTypeRepository repository,
        ProProductTypeManager manager)
        : base(repository)
    {
        Manager = manager;
        LocalizationResource = typeof(ProductResource);
        GetPolicyName = ProProductTypePermissions.View;
        GetListPolicyName = ProProductTypePermissions.View;
        CreatePolicyName = ProProductTypePermissions.Create;
        UpdatePolicyName = ProProductTypePermissions.Edit;
        DeletePolicyName = ProProductTypePermissions.Delete;
    }

    public override async Task<ProProductTypeDto> CreateAsync(CreateProProductTypeDto input)
    {
        var entity = new ProProductType(
            GuidGenerator.Create(),
            input.LobId,
            input.Code,
            input.Name,
            input.Status,
            input.Description
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<ProProductType, ProProductTypeDto>(entity);
    }

    public override async Task<ProProductTypeDto> UpdateAsync(Guid id, UpdateProProductTypeDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id);

        await Manager.UpdateAsync(entity, input.LobId, input.Name, input.Status, input.Description);

        // Ensure changes are saved (Repository.UpdateAsync should handle this, but explicitly save to ensure audit logging)
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<ProProductType, ProProductTypeDto>(entity);
    }

    public override async Task DeleteAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);

        // Delete to trigger audit logging with ChangeType = Deleted
        // FullAuditedAggregateRoot uses soft delete, so entity is not actually removed from database
        await Repository.DeleteAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();

        // Update status to Deactive (entity still exists in database due to soft delete)
        entity.UpdateStatus(ProProductTypeStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<ProProductType>> CreateFilteredQueryAsync(GetProProductTypesInput input)
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

        return query;
    }
}
