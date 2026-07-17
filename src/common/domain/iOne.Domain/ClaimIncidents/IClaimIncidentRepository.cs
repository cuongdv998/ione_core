using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ClaimIncidents;

public interface IClaimIncidentRepository : IRepository<ClaimIncident, Guid>
{
}
