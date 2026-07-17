using System;
using Volo.Abp.Domain.Repositories;

namespace iOne.ProProductDistributions;

public interface IProProductDistributionRepository : IRepository<ProProductDistribution, Guid>
{
}
