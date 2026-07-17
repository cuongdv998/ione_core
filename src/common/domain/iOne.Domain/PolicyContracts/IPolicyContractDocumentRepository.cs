using System;
using Volo.Abp.Domain.Repositories;

namespace iOne.PolicyContracts;

public interface IPolicyContractDocumentRepository : IRepository<PolicyContractDocument, Guid>
{
}
