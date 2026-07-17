using System;
using iOne.PolicyContracts;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace iOne.EntityFrameworkCore.PolicyContracts;

public class EfCorePolicyContractDocumentRepository
    : EfCoreRepository<iOneDbContext, PolicyContractDocument, Guid>, IPolicyContractDocumentRepository
{
    public EfCorePolicyContractDocumentRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }
}
