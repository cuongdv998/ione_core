using System;
using Volo.Abp.Domain.Repositories;

namespace iOne.ProProducts;

public interface IProProductCoverageLevelTermRepository : IRepository<ProProductCoverageLevelTerm, Guid>
{
    // No custom methods for now, following basic CRUD pattern
}
