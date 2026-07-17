using System;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using iOne.EntityFrameworkCore;
using iOne.AccountPaymentRequests;

namespace iOne.EntityFrameworkCore.AccountPaymentRequests;

public class EfCoreAccountPaymentRequestRepository : EfCoreRepository<iOneDbContext, AccountPaymentRequest, Guid>, IAccountPaymentRequestRepository
{
    public EfCoreAccountPaymentRequestRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }
}
