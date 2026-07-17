using System;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using iOne.EntityFrameworkCore;
using iOne.Policies;

namespace iOne.EntityFrameworkCore.Policies;

public class EfCorePolicyCertificateRepository : EfCoreRepository<iOneDbContext, PolicyCertificate, Guid>, IPolicyCertificateRepository
{
    public EfCorePolicyCertificateRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }
}
