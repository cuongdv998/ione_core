using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iOne.Master.Localization;
using iOne.Master.Permissions;
using iOne.Master.AdminConfigs;
using iOne.AdminConfigs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace iOne.Master.AdminConfigs;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Authorize(AdminConfigPermissions.Default)]
public class AdminConfigAppService : CrudAppService<
    AdminConfig,
    AdminConfigDto,
    Guid,
    GetAdminConfigsInput,
    CreateAdminConfigDto,
    UpdateAdminConfigDto>,
    IAdminConfigAppService
{
    protected AdminConfigManager Manager { get; }

    public AdminConfigAppService(
        IAdminConfigRepository repository,
        AdminConfigManager manager)
        : base(repository)
    {
        Manager = manager;
        LocalizationResource = typeof(MasterResource);
        GetPolicyName = AdminConfigPermissions.View;
        GetListPolicyName = AdminConfigPermissions.View;
        CreatePolicyName = AdminConfigPermissions.Create;
        UpdatePolicyName = AdminConfigPermissions.Edit;
        DeletePolicyName = AdminConfigPermissions.Delete;
    }

    public override async Task<AdminConfigDto> CreateAsync(CreateAdminConfigDto input)
    {
        var entity = new AdminConfig(
            GuidGenerator.Create(),
            input.Code,
            input.Name,
            input.SubCode,
            input.Value,
            input.Status,
            input.Description
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<AdminConfig, AdminConfigDto>(entity);
    }

    public override async Task<AdminConfigDto> UpdateAsync(Guid id, UpdateAdminConfigDto input)
    {
        var entity = await Repository.GetAsync(id);

        // ⚠️ QUAN TRỌNG: Chỉ update Name, Value, Description, Status - không update Code và SubCode
        await Manager.UpdateAsync(
            entity,
            input.Name,
            input.Value,
            input.Status,
            input.Description
        );

        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<AdminConfig, AdminConfigDto>(entity);
    }

    public override async Task DeleteAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);

        // ⚠️ QUAN TRỌNG: Delete trước để trigger audit logging với ChangeType = Deleted
        // FullAuditedAggregateRoot uses soft delete, so entity is not actually removed from database
        await Repository.DeleteAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();

        // Sau đó update status to Deactive (entity still exists in database due to soft delete)
        entity.UpdateStatus(AdminConfigStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<AdminConfig>> CreateFilteredQueryAsync(GetAdminConfigsInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();

        // Filter by Code (case-insensitive, contains)
        if (!string.IsNullOrWhiteSpace(input.Code))
        {
            query = query.Where(x => EF.Functions.ILike(x.Code, $"%{input.Code}%"));
        }

        // Filter by SubCode (case-insensitive, contains)
        if (!string.IsNullOrWhiteSpace(input.SubCode))
        {
            query = query.Where(x => EF.Functions.ILike(x.SubCode, $"%{input.SubCode}%"));
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

    /// <summary>
    /// Lấy danh sách AdminConfig cho dropdown theo code. Chỉ cần đăng nhập.
    /// </summary>
    [Authorize]
    public virtual async Task<List<AdminConfigSelectDto>> GetSelectListAsync(string? code = null)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();
        query = query.Where(x => x.Status == AdminConfigStatus.Active);
        if (!string.IsNullOrWhiteSpace(code))
        {
            query = query.Where(x => x.Code == code);
        }
        query = query.OrderBy(x => x.Name);
        var entities = await AsyncExecuter.ToListAsync(query);
        return entities.Select(x => new AdminConfigSelectDto
        {
            Id = x.Id,
            SubCode = x.SubCode ?? string.Empty,
            Name = x.Name ?? string.Empty,
            Value = x.Value ?? string.Empty
        }).ToList();
    }
}

