using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using iOne.Product;
using iOne.Product.ProCoverages;
using iOne.Product.Localization;
using iOne.Product.Permissions;
using iOne.ProCoverages; // For Entity, Manager, Repository
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp;

namespace iOne.Product.ProCoverages;

[RemoteService(Name = ProductRemoteServiceConsts.RemoteServiceName)]
[Authorize(ProCoveragePermissions.Default)]
public class ProCoverageAppService : CrudAppService<
    ProCoverage,
    ProCoverageDto,
    Guid,
    GetProCoveragesInput,
    CreateProCoverageDto,
    UpdateProCoverageDto>, IProCoverageAppService
{
    protected ProCoverageManager Manager { get; }

    public ProCoverageAppService(
        IProCoverageRepository repository,
        ProCoverageManager manager)
        : base(repository)
    {
        Manager = manager;
        LocalizationResource = typeof(ProductResource);
        GetPolicyName = ProCoveragePermissions.View;
        GetListPolicyName = ProCoveragePermissions.View;
        CreatePolicyName = ProCoveragePermissions.Create;
        UpdatePolicyName = ProCoveragePermissions.Edit;
        DeletePolicyName = ProCoveragePermissions.Delete;
    }

    public override async Task<ProCoverageDto> CreateAsync(CreateProCoverageDto input)
    {
        // Validate Code format: only A-Z, 0-9, and underscore allowed
        if (!Regex.IsMatch(input.Code, @"^[A-Z0-9_]+$"))
        {
            throw new BusinessException("ProCoverage:CodeInvalidFormat");
        }

        var entity = new ProCoverage(
            GuidGenerator.Create(),
            input.LobId,
            input.CoverageGroupId,
            input.Type,
            input.Code,
            input.ShortName,
            input.Name,
            input.Status,
            input.ObjectTypeId,
            input.CoverageTypeId,
            input.Description
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<ProCoverage, ProCoverageDto>(entity);
    }

    public override async Task<ProCoverageDto> UpdateAsync(Guid id, UpdateProCoverageDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        var entity = await Repository.GetAsync(id);

        await Manager.UpdateAsync(
            entity,
            input.LobId,
            input.CoverageGroupId,
            input.Type,
            input.ShortName,
            input.Name,
            input.Status,
            input.ObjectTypeId,
            input.CoverageTypeId,
            input.Description);

        // Ensure changes are saved (Repository.UpdateAsync should handle this, but explicitly save to ensure audit logging)
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<ProCoverage, ProCoverageDto>(entity);
    }

    public override async Task DeleteAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);
        
        // Delete to trigger audit logging with ChangeType = Deleted
        // FullAuditedAggregateRoot uses soft delete, so entity is not actually removed from database
        await Repository.DeleteAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
        
        // Update status to Deactive (entity still exists in database due to soft delete)
        entity.UpdateStatus(ProCoverageStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<ProCoverage>> CreateFilteredQueryAsync(GetProCoveragesInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();

        // Filter by LobId
        if (input.LobId.HasValue)
        {
            query = query.Where(x => x.LobId == input.LobId.Value);
        }

        // Filter by ObjectTypeId
        if (input.ObjectTypeId.HasValue)
        {
            query = query.Where(x => x.ObjectTypeId == input.ObjectTypeId.Value);
        }

        // Filter by CoverageGroupId
        if (input.CoverageGroupId.HasValue)
        {
            query = query.Where(x => x.CoverageGroupId == input.CoverageGroupId.Value);
        }

        // Filter by CoverageTypeId
        if (input.CoverageTypeId.HasValue)
        {
            query = query.Where(x => x.CoverageTypeId == input.CoverageTypeId.Value);
        }

        // Filter by Type
        if (input.Type.HasValue)
        {
            query = query.Where(x => x.Type == input.Type.Value);
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
