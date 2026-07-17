using System;
using System.Linq;
using System.Threading.Tasks;
using iOne.Master;
using iOne.Master.Localization;
using iOne.Master.Permissions;
using iOne.Master.ResBanks;
using iOne.ResBanks; // For Entity, Manager, Repository
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResBanks;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Authorize(ResBankPermissions.Default)]
public class ResBankAppService : CrudAppService<
    ResBank,
    ResBankDto,
    Guid,
    GetResBanksInput,
    CreateResBankDto,
    UpdateResBankDto>, IResBankAppService
{
    protected ResBankManager Manager { get; }

    public ResBankAppService(
        IResBankRepository repository,
        ResBankManager manager)
        : base(repository)
    {
        Manager = manager;
        LocalizationResource = typeof(MasterResource);
        GetPolicyName = ResBankPermissions.View;
        GetListPolicyName = ResBankPermissions.View;
        CreatePolicyName = ResBankPermissions.Create;
        UpdatePolicyName = ResBankPermissions.Edit;
        DeletePolicyName = ResBankPermissions.Delete;
    }

    public override async Task<ResBankDto> CreateAsync(CreateResBankDto input)
    {
        var entity = new ResBank(
            GuidGenerator.Create(),
            input.Code,
            input.Name,
            input.Status
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<ResBank, ResBankDto>(entity);
    }

    public override async Task<ResBankDto> UpdateAsync(Guid id, UpdateResBankDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id);

        // ⚠️ QUAN TRỌNG: Chỉ update Name và Status, không update Code
        await Manager.UpdateAsync(entity, input.Name, input.Status);

        // Ensure changes are saved (Repository.UpdateAsync should handle this, but explicitly save to ensure audit logging)
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<ResBank, ResBankDto>(entity!);
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
        entity!.UpdateStatus(ResBankStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<ResBank>> CreateFilteredQueryAsync(GetResBanksInput input)
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

