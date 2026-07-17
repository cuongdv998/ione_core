using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace iOne.Policies;

public class PolicyAmountManager : DomainService
{
    public PolicyAmountManager(IPolicyAmountRepository repository)
    {
        Repository = repository;
    }

    protected IPolicyAmountRepository Repository { get; }

    public virtual async Task CreateAsync(PolicyAmount policyAmount)
    {
        await Repository.InsertAsync(policyAmount);
    }

    public virtual async Task UpdateAsync(
        PolicyAmount policyAmount,
        DateTime issueDate,
        decimal amountTotal,
        decimal amount,
        decimal vat,
        string paymentStatus,
        Guid? paymentMethodId = null,
        DateTime? paymentDate = null)
    {
        policyAmount.UpdateIssueDate(issueDate);
        policyAmount.UpdateAmountTotal(amountTotal);
        policyAmount.UpdateAmount(amount);
        policyAmount.UpdateVat(vat);
        policyAmount.UpdatePaymentStatus(paymentStatus);
        policyAmount.UpdatePaymentMethodId(paymentMethodId);
        policyAmount.UpdatePaymentDate(paymentDate);

        await Repository.UpdateAsync(policyAmount);
    }

    public virtual async Task DeleteAsync(PolicyAmount policyAmount)
    {
        await Repository.DeleteAsync(policyAmount);
    }
}