using System;
using System.Linq;
using System.Threading.Tasks;
using iOne.Policy;
using iOne.Policy.Localization;
using iOne.Policy.Permissions;
using iOne.Policy.PolicyRiskObjects;
using iOne.Policies; // For Entity, Manager, Repository
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace iOne.Policy.PolicyRiskObjects;

[RemoteService(Name = PolicyRemoteServiceConsts.RemoteServiceName)]
[Authorize(PolicyRiskObjectPermissions.Default)]
public class PolicyRiskObjectAppService : CrudAppService<
    PolicyRiskObject,
    PolicyRiskObjectDto,
    Guid,
    GetPolicyRiskObjectsInput,
    CreatePolicyRiskObjectDto,
    UpdatePolicyRiskObjectDto>, IPolicyRiskObjectAppService
{
    protected PolicyRiskObjectManager Manager { get; }
    protected IPolicyRiskObjectRepository PolicyRiskObjectRepository { get; }

    public PolicyRiskObjectAppService(
        IRepository<PolicyRiskObject, Guid> repository,
        PolicyRiskObjectManager manager,
        IPolicyRiskObjectRepository policyRiskObjectRepository)
        : base(repository)
    {
        Manager = manager;
        PolicyRiskObjectRepository = policyRiskObjectRepository;
        LocalizationResource = typeof(PolicyResource);
        GetPolicyName = PolicyRiskObjectPermissions.View;
        GetListPolicyName = PolicyRiskObjectPermissions.View;
        CreatePolicyName = PolicyRiskObjectPermissions.Create;
        UpdatePolicyName = PolicyRiskObjectPermissions.Edit;
        DeletePolicyName = PolicyRiskObjectPermissions.Delete;
    }

    public override async Task<PolicyRiskObjectDto> CreateAsync(CreatePolicyRiskObjectDto input)
    {
        var entity = new PolicyRiskObject(
            GuidGenerator.Create(),
            input.PolicyId,
            input.PolicyVersionId,
            input.ObjectTypeId,
            input.RepName,
            input.RepIdNo,
            input.RepPassport,
            input.RepPhone,
            input.RepEmail,
            input.RepProvinceId,
            input.RepWardId,
            input.RepAddress,
            input.RepFullAddress,
            input.RiskObjectProvinceId,
            input.RiskObjectWardId,
            input.RiskObjectAddress,
            input.RiskObjectFullAddress,
            input.RiskObjectLat,
            input.RiskObjectLong
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<PolicyRiskObject, PolicyRiskObjectDto>(entity);
    }

    public override async Task<PolicyRiskObjectDto> UpdateAsync(Guid id, UpdatePolicyRiskObjectDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id);

        // Update entity properties
        entity.UpdateRepName(input.RepName);
        entity.UpdateRepIdNo(input.RepIdNo);
        entity.UpdateRepPassport(input.RepPassport);
        entity.UpdateRepPhone(input.RepPhone);
        entity.UpdateRepEmail(input.RepEmail);
        entity.UpdateRepProvinceId(input.RepProvinceId);
        entity.UpdateRepWardId(input.RepWardId);
        entity.UpdateRepAddress(input.RepAddress);
        entity.UpdateRepFullAddress(input.RepFullAddress);
        entity.UpdateRiskObjectProvinceId(input.RiskObjectProvinceId);
        entity.UpdateRiskObjectWardId(input.RiskObjectWardId);
        entity.UpdateRiskObjectAddress(input.RiskObjectAddress);
        entity.UpdateRiskObjectFullAddress(input.RiskObjectFullAddress);
        entity.UpdateRiskObjectLat(input.RiskObjectLat);
        entity.UpdateRiskObjectLong(input.RiskObjectLong);

        // Ensure changes are saved
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<PolicyRiskObject, PolicyRiskObjectDto>(entity);
    }

    public override async Task DeleteAsync(Guid id)
    {
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id);
        
        // Delete to trigger audit logging with ChangeType = Deleted
        // FullAuditedAggregateRoot uses soft delete, so entity is not actually removed from database
        await Repository.DeleteAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<PolicyRiskObject>> CreateFilteredQueryAsync(GetPolicyRiskObjectsInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();

        // Filter by PolicyId
        if (input.PolicyId.HasValue)
        {
            query = query.Where(x => x.PolicyId == input.PolicyId.Value);
        }

        // Filter by PolicyVersionId
        if (input.PolicyVersionId.HasValue)
        {
            query = query.Where(x => x.PolicyVersionId == input.PolicyVersionId.Value);
        }

        // Filter by ObjectTypeId
        if (input.ObjectTypeId.HasValue)
        {
            query = query.Where(x => x.ObjectTypeId == input.ObjectTypeId.Value);
        }

        return query;
    }
}
