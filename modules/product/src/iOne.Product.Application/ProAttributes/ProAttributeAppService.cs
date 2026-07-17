using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iOne.Product;
using iOne.Product.ProAttributes;
using iOne.Product.Localization;
using iOne.Product.Permissions;
using iOne.ProAttributes; // For Entity, Manager, Repository
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Identity;
using Volo.Abp;

namespace iOne.Product.ProAttributes;

[RemoteService(Name = ProductRemoteServiceConsts.RemoteServiceName)]
[Authorize(ProAttributePermissions.Default)]
public class ProAttributeAppService : CrudAppService<
    ProAttribute,
    ProAttributeDto,
    Guid,
    GetProAttributesInput,
    CreateProAttributeDto,
    UpdateProAttributeDto>, IProAttributeAppService
{
    protected ProAttributeManager Manager { get; }
    protected IIdentityUserRepository UserRepository { get; }

    public ProAttributeAppService(
        IProAttributeRepository repository,
        ProAttributeManager manager,
        IIdentityUserRepository userRepository)
        : base(repository)
    {
        Manager = manager;
        UserRepository = userRepository;
        LocalizationResource = typeof(ProductResource);
        GetPolicyName = ProAttributePermissions.View;
        GetListPolicyName = ProAttributePermissions.View;
        CreatePolicyName = ProAttributePermissions.Create;
        UpdatePolicyName = ProAttributePermissions.Edit;
        DeletePolicyName = ProAttributePermissions.Delete;
    }

    public override async Task<ProAttributeDto> CreateAsync(CreateProAttributeDto input)
    {
        var entity = new ProAttribute(
            GuidGenerator.Create(),
            input.Code,
            input.Name,
            input.Status,
            input.Spec,
            input.DataType,
            input.Description,
            input.DataPath,
            input.ComputeScript,
            input.ClearDataScript
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<ProAttribute, ProAttributeDto>(entity);
    }

    public override async Task<ProAttributeDto> UpdateAsync(Guid id, UpdateProAttributeDto input)
    {
        var entity = await Repository.GetAsync(id);

        await Manager.UpdateAsync(
            entity,
            input.Name,
            input.Status,
            input.Spec,
            input.DataType,
            input.Description,
            input.DataPath,
            input.ComputeScript,
            input.ClearDataScript
        );

        return ObjectMapper.Map<ProAttribute, ProAttributeDto>(entity);
    }

    public override async Task<PagedResultDto<ProAttributeDto>> GetListAsync(GetProAttributesInput input)
    {
        var result = await base.GetListAsync(input);

        var creatorIds = result.Items
            .Where(x => x.CreatorId.HasValue)
            .Select(x => x.CreatorId!.Value)
            .Distinct()
            .ToList();

        if (creatorIds.Count > 0)
        {
            var users = await UserRepository.GetListByIdsAsync(creatorIds);
            var userDictionary = users.ToDictionary(u => u.Id, u => u.Name ?? u.UserName);

            foreach (var item in result.Items)
            {
                if (item.CreatorId.HasValue && userDictionary.TryGetValue(item.CreatorId.Value, out var userName))
                {
                    item.CreatorName = userName;
                }
            }
        }

        return result;
    }

    protected override async Task<IQueryable<ProAttribute>> CreateFilteredQueryAsync(GetProAttributesInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);

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
}
