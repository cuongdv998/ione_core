using System;
using System.Linq;
using System.Threading.Tasks;
using iOne.Master;
using iOne.Master.Localization;
using iOne.Master.Permissions;
using iOne.Master.ResTaskCategories;
using iOne.ResTaskCategories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResTaskCategories;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Authorize(ResTaskCategoryPermissions.Default)]
public class ResTaskCategoryAppService : CrudAppService<
    ResTaskCategory,
    ResTaskCategoryDto,
    Guid,
    GetResTaskCategoriesInput,
    CreateResTaskCategoryDto,
    UpdateResTaskCategoryDto>, IResTaskCategoryAppService
{
    protected ResTaskCategoryManager Manager { get; }
    protected IResTaskCategoryRepository TaskCategoryRepository { get; }

    public ResTaskCategoryAppService(
        IResTaskCategoryRepository repository,
        ResTaskCategoryManager manager)
        : base(repository)
    {
        Manager = manager;
        TaskCategoryRepository = repository;
        LocalizationResource = typeof(MasterResource);
        GetPolicyName = ResTaskCategoryPermissions.View;
        GetListPolicyName = ResTaskCategoryPermissions.View;
        CreatePolicyName = ResTaskCategoryPermissions.Create;
        UpdatePolicyName = ResTaskCategoryPermissions.Edit;
        DeletePolicyName = ResTaskCategoryPermissions.Delete;
    }

    public override async Task<ResTaskCategoryDto> CreateAsync(CreateResTaskCategoryDto input)
    {
        var normalizedCode = input.Code?.Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(normalizedCode) && await TaskCategoryRepository.IsCodeExistsAsync(normalizedCode))
        {
            throw new BusinessException("ResTaskCategory:CodeExists")
                .WithData("Code", normalizedCode);
        }

        var entity = new ResTaskCategory(
            GuidGenerator.Create(),
            input.BusinessType,
            input.Code,
            input.Name,
            input.Status
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<ResTaskCategory, ResTaskCategoryDto>(entity);
    }

    public override async Task<ResTaskCategoryDto> UpdateAsync(Guid id, UpdateResTaskCategoryDto input)
    {
        var entity = await Repository.GetAsync(id);

        await Manager.UpdateAsync(entity, input.Name, input.Status);

        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<ResTaskCategory, ResTaskCategoryDto>(entity);
    }

    public override async Task DeleteAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);

        await Repository.DeleteAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();

        entity.UpdateStatus(ResTaskCategoryStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<ResTaskCategory>> CreateFilteredQueryAsync(GetResTaskCategoriesInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.Code))
        {
            query = query.Where(x => EF.Functions.ILike(x.Code, $"%{input.Code}%"));
        }

        if (input.BusinessType.HasValue)
        {
            query = query.Where(x => x.BusinessType == input.BusinessType.Value);
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
