using System;
using System.Linq;
using System.Threading.Tasks;
using iOne.Master;
using iOne.Master.Localization;
using iOne.Master.Permissions;
using iOne.Master.ResCountries;
using iOne.ResCountries; // For Entity, Manager, Repository
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResCountries;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Authorize(ResCountryPermissions.Default)]
public class ResCountryAppService : CrudAppService<
    ResCountry,
    ResCountryDto,
    Guid,
    GetResCountriesInput,
    CreateResCountryDto,
    UpdateResCountryDto>, IResCountryAppService
{
    protected ResCountryManager Manager { get; }

    public ResCountryAppService(
        IResCountryRepository repository,
        ResCountryManager manager)
        : base(repository)
    {
        Manager = manager;
        LocalizationResource = typeof(MasterResource);
        GetPolicyName = ResCountryPermissions.View;
        GetListPolicyName = ResCountryPermissions.View;
        CreatePolicyName = ResCountryPermissions.Create;
        UpdatePolicyName = ResCountryPermissions.Edit;
        DeletePolicyName = ResCountryPermissions.Delete;
    }

    public override async Task<ResCountryDto> CreateAsync(CreateResCountryDto input)
    {
        var entity = new ResCountry(
            GuidGenerator.Create(),
            input.Code,
            input.Name,
            input.Status
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<ResCountry, ResCountryDto>(entity);
    }

    public override async Task<ResCountryDto> UpdateAsync(Guid id, UpdateResCountryDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id);

        // ⚠️ QUAN TRỌNG: Chỉ update Name và Status, không update Code
        await Manager.UpdateAsync(entity, input.Name, input.Status);

        // Ensure changes are saved (Repository.UpdateAsync should handle this, but explicitly save to ensure audit logging)
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<ResCountry, ResCountryDto>(entity!);
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
        entity!.UpdateStatus(ResCountryStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<ResCountry>> CreateFilteredQueryAsync(GetResCountriesInput input)
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

