using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ProProducts;

public interface IProProductRepository : IRepository<ProProduct, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null);

    /// <summary>True if any (non-deleted) product references this plan definition.</summary>
    Task<bool> AnyProductReferencesPlanDefinitionAsync(Guid planDefinitionId);
}
