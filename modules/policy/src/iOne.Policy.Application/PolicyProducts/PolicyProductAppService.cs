using System;
using System.Linq;
using System.Threading.Tasks;
using iOne.Policy;
using iOne.Policy.Localization;
using iOne.Policy.Permissions;
using iOne.Policy.PolicyProducts;
using iOne.Policies; // For Entity, Manager, Repository
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace iOne.Policy.PolicyProducts;

[RemoteService(Name = PolicyRemoteServiceConsts.RemoteServiceName)]
[Authorize(PolicyProductPermissions.Default)]
public class PolicyProductAppService : CrudAppService<
    PolicyProduct,
    PolicyProductDto,
    Guid,
    GetPolicyProductsInput,
    CreatePolicyProductDto,
    UpdatePolicyProductDto>, IPolicyProductAppService
{
    protected PolicyProductManager Manager { get; }
    protected IPolicyProductRepository PolicyProductRepository { get; }

    public PolicyProductAppService(
        IRepository<PolicyProduct, Guid> repository,
        PolicyProductManager manager,
        IPolicyProductRepository policyProductRepository)
        : base(repository)
    {
        Manager = manager;
        PolicyProductRepository = policyProductRepository;
        LocalizationResource = typeof(PolicyResource);
        GetPolicyName = PolicyProductPermissions.View;
        GetListPolicyName = PolicyProductPermissions.View;
        CreatePolicyName = PolicyProductPermissions.Create;
        UpdatePolicyName = PolicyProductPermissions.Edit;
        DeletePolicyName = PolicyProductPermissions.Delete;
    }

    public override async Task<PolicyProductDto> CreateAsync(CreatePolicyProductDto input)
    {
        var entity = new PolicyProduct(
            GuidGenerator.Create(),
            input.PolicyVersionId,
            input.ProductId,
            input.PremiumTotal,
            input.Premium,
            input.Vat,
            input.InsurerProductCode,
            input.AmountLiability,
            input.Discount,
            input.DiscountRate,
            input.Markup
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<PolicyProduct, PolicyProductDto>(entity);
    }

    public override async Task<PolicyProductDto> UpdateAsync(Guid id, UpdatePolicyProductDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id);

        // Update entity properties
        entity.UpdateInsurerProductCode(input.InsurerProductCode);
        entity.UpdateAmountLiability(input.AmountLiability);
        entity.UpdatePremiumTotal(input.PremiumTotal);
        entity.UpdatePremium(input.Premium);
        entity.UpdateVat(input.Vat);
        entity.UpdateDiscount(input.Discount);
        entity.UpdateDiscountRate(input.DiscountRate);
        entity.UpdateMarkup(input.Markup);

        // Ensure changes are saved
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<PolicyProduct, PolicyProductDto>(entity);
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

    protected override async Task<IQueryable<PolicyProduct>> CreateFilteredQueryAsync(GetPolicyProductsInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();

        // Filter by PolicyVersionId
        if (input.PolicyVersionId.HasValue)
        {
            query = query.Where(x => x.PolicyVersionId == input.PolicyVersionId.Value);
        }

        // Filter by ProductId
        if (input.ProductId.HasValue)
        {
            query = query.Where(x => x.ProductId == input.ProductId.Value);
        }

        return query;
    }
}
