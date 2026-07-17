using System;
using Volo.Abp.Domain.Repositories;

namespace iOne.Policies;

public interface IPolicyDocumentRepository : IRepository<PolicyDocument, Guid>
{
}
