using System;
using System.Linq;
using System.Threading.Tasks;
using iOne.Master;
using iOne.Master.Localization;
using iOne.Master.Permissions;
using iOne.Master.ResCarLines;
using iOne.ResCarCategories; // For IResCarCategoryRepository
using iOne.ResCarGroups; // For IResCarGroupRepository
using iOne.ResCarLines; // For Entity, Manager, Repository
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResCarLines;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Authorize(ResCarLinePermissions.Default)]
public class ResCarLineAppService : CrudAppService<
    ResCarLine,
    ResCarLineDto,
    Guid,
    GetResCarLinesInput,
    CreateResCarLineDto,
    UpdateResCarLineDto>, IResCarLineAppService
{
    protected ResCarLineManager Manager { get; }
    protected IResCarLineRepository CarLineRepository { get; }
    protected IResCarCategoryRepository CarCategoryRepository { get; }
    protected IResCarGroupRepository CarGroupRepository { get; }

    public ResCarLineAppService(
        IResCarLineRepository repository,
        ResCarLineManager manager,
        IResCarCategoryRepository carCategoryRepository,
        IResCarGroupRepository carGroupRepository)
        : base(repository)
    {
        Manager = manager;
        CarLineRepository = repository;
        CarCategoryRepository = carCategoryRepository;
        CarGroupRepository = carGroupRepository;
        LocalizationResource = typeof(MasterResource);
        GetPolicyName = ResCarLinePermissions.View;
        GetListPolicyName = ResCarLinePermissions.View;
        CreatePolicyName = ResCarLinePermissions.Create;
        UpdatePolicyName = ResCarLinePermissions.Edit;
        DeletePolicyName = ResCarLinePermissions.Delete;
    }

    public override async Task<ResCarLineDto> CreateAsync(CreateResCarLineDto input)
    {
        // Validate duplicate code
        var normalizedCode = input.Code?.Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(normalizedCode))
        {
            if (await CarLineRepository.IsCodeExistsAsync(normalizedCode))
            {
                throw new UserFriendlyException(
                    L["ResCarLine:CodeExists"].Value.Replace("{Code}", normalizedCode)
                );
                //throw new BusinessException("Master:ResCarLine:CodeExists")
                //    .WithData("Code", normalizedCode);
            }
        }

        var entity = new ResCarLine(
            GuidGenerator.Create(),
            input.Code,
            input.Name,
            input.Description,
            input.Status
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<ResCarLine, ResCarLineDto>(entity);
    }

    public override async Task<ResCarLineDto> UpdateAsync(Guid id, UpdateResCarLineDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id);

        // ⚠️ QUAN TRỌNG: Chỉ update Name, Description và Status, không update Code
        await Manager.UpdateAsync(entity, input.Name, input.Description, input.Status);

        // Ensure changes are saved (Repository.UpdateAsync should handle this, but explicitly save to ensure audit logging)
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<ResCarLine, ResCarLineDto>(entity!);
    }

    public override async Task DeleteAsync(Guid id)
    {
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id);

        // Check if there are any car categories using this car line
        if (await CarCategoryRepository.HasCategoriesForLineAsync(id))
        {
            throw new UserFriendlyException(L["ResCarLine:HasCarCategories"]);
        }

        // Check if there are any car groups using this car line
        if (await CarGroupRepository.HasGroupsForLineAsync(id))
        {
            throw new UserFriendlyException(L["ResCarLine:HasCarGroups"]);
        }
        
        // Delete to trigger audit logging with ChangeType = Deleted
        // FullAuditedAggregateRoot uses soft delete, so entity is not actually removed from database
        await Repository.DeleteAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
        
        // Update status to Deactive (entity still exists in database due to soft delete)
        entity!.UpdateStatus(ResCarLineStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<ResCarLine>> CreateFilteredQueryAsync(GetResCarLinesInput input)
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


