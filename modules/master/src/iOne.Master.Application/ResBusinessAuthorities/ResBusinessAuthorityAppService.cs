using System;
using System.Linq;
using System.Threading.Tasks;
using iOne.Master;
using iOne.Master.Localization;
using iOne.Master.Permissions;
using iOne.Master.ResBusinessAuthorities;
using iOne.ResBusinessAuthorities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResBusinessAuthorities;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Authorize(ResBusinessAuthorityPermissions.Default)]
public class ResBusinessAuthorityAppService : CrudAppService<
    ResBusinessAuthority,
    ResBusinessAuthorityDto,
    Guid,
    GetResBusinessAuthoritiesInput,
    CreateResBusinessAuthorityDto,
    UpdateResBusinessAuthorityDto>, IResBusinessAuthorityAppService
{
    protected ResBusinessAuthorityManager Manager { get; }
    protected IResBusinessAuthorityRepository AuthorityRepository { get; }

    public ResBusinessAuthorityAppService(
        IResBusinessAuthorityRepository repository,
        ResBusinessAuthorityManager manager)
        : base(repository)
    {
        Manager = manager;
        AuthorityRepository = repository;
        LocalizationResource = typeof(MasterResource);
        GetPolicyName = ResBusinessAuthorityPermissions.View;
        GetListPolicyName = ResBusinessAuthorityPermissions.View;
        CreatePolicyName = ResBusinessAuthorityPermissions.Create;
        UpdatePolicyName = ResBusinessAuthorityPermissions.Edit;
        DeletePolicyName = ResBusinessAuthorityPermissions.Delete;
    }

    public override async Task<ResBusinessAuthorityDto> CreateAsync(CreateResBusinessAuthorityDto input)
    {
        var normalizedCode = input.Code?.Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(normalizedCode) && await AuthorityRepository.IsCodeExistsAsync(normalizedCode))
        {
            throw new BusinessException("ResBusinessAuthority:CodeExists")
                .WithData("Code", normalizedCode);
        }

        var entity = new ResBusinessAuthority(
            GuidGenerator.Create(),
            input.BusinessCode,
            input.Code,
            input.Name,
            input.Status
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<ResBusinessAuthority, ResBusinessAuthorityDto>(entity);
    }

    public override async Task<ResBusinessAuthorityDto> UpdateAsync(Guid id, UpdateResBusinessAuthorityDto input)
    {
        var entity = await Repository.GetAsync(id);

        await Manager.UpdateAsync(entity, input.Name, input.Status);

        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<ResBusinessAuthority, ResBusinessAuthorityDto>(entity);
    }

    public override async Task DeleteAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);

        await Repository.DeleteAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();

        entity.UpdateStatus(ResBusinessAuthorityStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<ResBusinessAuthority>> CreateFilteredQueryAsync(GetResBusinessAuthoritiesInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.Code))
        {
            query = query.Where(x => EF.Functions.ILike(x.Code, $"%{input.Code}%"));
        }

        if (!string.IsNullOrWhiteSpace(input.BusinessCode))
        {
            query = query.Where(x => EF.Functions.ILike(x.BusinessCode, $"%{input.BusinessCode}%"));
        }

        if (!string.IsNullOrWhiteSpace(input.Name))
        {
            query = query.Where(x => EF.Functions.ILike(x.Name, $"%{input.Name}%"));
        }

        if (input.Status.HasValue)
        {
            query = query.Where(x => x.Status == input.Status.Value);
        }

        return query;
    }
}
