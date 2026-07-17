using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ResAgreementTerms;

public interface IResAgreementTermRepository : IRepository<ResAgreementTerm, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null);
    Task<ResAgreementTerm?> FindByCodeAsync(string code);
}

