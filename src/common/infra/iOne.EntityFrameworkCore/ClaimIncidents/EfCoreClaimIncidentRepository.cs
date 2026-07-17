using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using iOne.EntityFrameworkCore;
using iOne.ClaimIncidents;

namespace iOne.EntityFrameworkCore.ClaimIncidents;

public class EfCoreClaimIncidentRepository : EfCoreRepository<iOneDbContext, ClaimIncident, Guid>, IClaimIncidentRepository
{
    public EfCoreClaimIncidentRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }
}
