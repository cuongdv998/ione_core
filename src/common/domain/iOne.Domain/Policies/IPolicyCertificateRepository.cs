using System;
using Volo.Abp.Domain.Repositories;

namespace iOne.Policies;

public interface IPolicyCertificateRepository : IRepository<PolicyCertificate, Guid>
{
    // No custom methods for now, following basic CRUD pattern
}
