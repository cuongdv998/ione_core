using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iOne.Master;
using iOne.Master.Localization;
using iOne.Master.Permissions;
using iOne.Master.ResPaymentTypes;
using iOne.ResPaymentTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResPaymentTypes;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Authorize(ResPaymentTypePermissions.Default)]
public class ResPaymentTypeAppService : CrudAppService<
    ResPaymentType,
    ResPaymentTypeDto,
    Guid,
    GetResPaymentTypesInput,
    CreateResPaymentTypeDto,
    UpdateResPaymentTypeDto>, IResPaymentTypeAppService
{
    protected ResPaymentTypeManager Manager { get; }
    protected IResPaymentTypeRepository PaymentTypeRepository { get; }

    public ResPaymentTypeAppService(
        IResPaymentTypeRepository repository,
        ResPaymentTypeManager manager)
        : base(repository)
    {
        Manager = manager;
        PaymentTypeRepository = repository;
        LocalizationResource = typeof(MasterResource);
        GetPolicyName = ResPaymentTypePermissions.View;
        GetListPolicyName = ResPaymentTypePermissions.View;
        CreatePolicyName = ResPaymentTypePermissions.Create;
        UpdatePolicyName = ResPaymentTypePermissions.Edit;
        DeletePolicyName = ResPaymentTypePermissions.Delete;
    }

    public override async Task<ResPaymentTypeDto> CreateAsync(CreateResPaymentTypeDto input)
    {
        var normalizedCode = input.Code?.Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(normalizedCode))
        {
            if (await PaymentTypeRepository.IsCodeExistsAsync(normalizedCode))
            {
                throw new UserFriendlyException(
                    L["ResPaymentType:CodeExists"].Value.Replace("{Code}", normalizedCode)
                );
            }
        }

        var entity = new ResPaymentType(
            GuidGenerator.Create(),
            input.Code,
            input.Name,
            input.Description,
            input.Status
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<ResPaymentType, ResPaymentTypeDto>(entity);
    }

    public override async Task<ResPaymentTypeDto> UpdateAsync(Guid id, UpdateResPaymentTypeDto input)
    {
        var entity = await Repository.GetAsync(id);

        await Manager.UpdateAsync(entity, input.Name, input.Description, input.Status);

        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<ResPaymentType, ResPaymentTypeDto>(entity);
    }

    public override async Task DeleteAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);

        await Repository.DeleteAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();

        entity!.UpdateStatus(ResPaymentTypeStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<ResPaymentType>> CreateFilteredQueryAsync(GetResPaymentTypesInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.Code))
        {
            query = query.Where(x => EF.Functions.ILike(x.Code, $"%{input.Code}%"));
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

    [Authorize]
    public virtual async Task<List<ResPaymentTypeSelectDto>> GetSelectListAsync()
    {
        var query = (await ReadOnlyRepository.GetQueryableAsync())
            .Where(x => !x.IsDeleted && x.Status == ResPaymentTypeStatus.Active)
            .OrderBy(x => x.Name);
        var entities = await AsyncExecuter.ToListAsync(query);
        return entities.Select(x => new ResPaymentTypeSelectDto
        {
            Id = x.Id,
            Code = x.Code ?? string.Empty,
            Name = x.Name ?? string.Empty
        }).ToList();
    }
}
