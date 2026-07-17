using System;
using System.Linq;
using System.Threading.Tasks;
using iOne.Policy;
using iOne.Policy.Localization;
using iOne.Policy.Permissions;
using iOne.Policy.PolicyAmounts;
using iOne.Policies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace iOne.Policy.PolicyAmounts;

[RemoteService(Name = PolicyRemoteServiceConsts.RemoteServiceName)]
[Authorize(PolicyAmountPermissions.Default)]
public class PolicyAmountAppService : CrudAppService<
    PolicyAmount,
    PolicyAmountDto,
    Guid,
    GetPolicyAmountsInput,
    CreatePolicyAmountDto,
    UpdatePolicyAmountDto>, IPolicyAmountAppService
{
    protected PolicyAmountManager Manager { get; }
    protected IPolicyAmountRepository PolicyAmountRepository { get; }

    public PolicyAmountAppService(
        IRepository<PolicyAmount, Guid> repository,
        PolicyAmountManager manager,
        IPolicyAmountRepository policyAmountRepository)
        : base(repository)
    {
        Manager = manager;
        PolicyAmountRepository = policyAmountRepository;
        LocalizationResource = typeof(PolicyResource);
        GetPolicyName = PolicyAmountPermissions.View;
        GetListPolicyName = PolicyAmountPermissions.View;
        CreatePolicyName = PolicyAmountPermissions.Create;
        UpdatePolicyName = PolicyAmountPermissions.Edit;
        DeletePolicyName = PolicyAmountPermissions.Delete;
    }

    public override async Task<PolicyAmountDto> CreateAsync(CreatePolicyAmountDto input)
    {
        var entity = new PolicyAmount(
            GuidGenerator.Create(),
            input.PolicyId,
            input.PolicyVersionId,
            input.FeeItemId,
            input.IssueDate,
            input.AmountTotal,
            input.Amount,
            input.Vat,
            input.PaymentStatus,
            input.PaymentMethodId,
            input.PaymentDate
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<PolicyAmount, PolicyAmountDto>(entity);
    }

    public override async Task<PolicyAmountDto> UpdateAsync(Guid id, UpdatePolicyAmountDto input)
    {
        var entity = await Repository.GetAsync(id);

        await Manager.UpdateAsync(
            entity,
            input.IssueDate,
            input.AmountTotal,
            input.Amount,
            input.Vat,
            input.PaymentStatus,
            input.PaymentMethodId,
            input.PaymentDate
        );

        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<PolicyAmount, PolicyAmountDto>(entity);
    }

    public override async Task DeleteAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);
        await Repository.DeleteAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<PolicyAmount>> CreateFilteredQueryAsync(GetPolicyAmountsInput input)
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

        // Filter by FeeItemId
        if (input.FeeItemId.HasValue)
        {
            query = query.Where(x => x.FeeItemId == input.FeeItemId.Value);
        }

        // Filter by PaymentStatus (case-insensitive, contains)
        if (!string.IsNullOrWhiteSpace(input.PaymentStatus))
        {
            query = query.Where(x => EF.Functions.ILike(x.PaymentStatus, $"%{input.PaymentStatus}%"));
        }

        return query;
    }
}
