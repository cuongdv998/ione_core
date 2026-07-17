using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.AccountPaymentRequests;

public class AccountPaymentRequestManager : DomainService
{
    protected IAccountPaymentRequestRepository Repository { get; }

    public AccountPaymentRequestManager(IAccountPaymentRequestRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task<AccountPaymentRequest> CreateAsync(AccountPaymentRequest entity)
    {
        await Repository.InsertAsync(entity);
        return entity;
    }

    public virtual async Task UpdateAsync(AccountPaymentRequest entity)
    {
        await Repository.UpdateAsync(entity);
    }

    public virtual async Task DeleteAsync(AccountPaymentRequest entity)
    {
        await Repository.DeleteAsync(entity);
    }
}
