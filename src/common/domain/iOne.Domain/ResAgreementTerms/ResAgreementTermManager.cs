using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ResAgreementTerms;

public class ResAgreementTermManager : DomainService
{
    protected IResAgreementTermRepository Repository { get; }

    public ResAgreementTermManager(IResAgreementTermRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ResAgreementTerm term)
    {
        // Check code uniqueness
        if (await Repository.FindByCodeAsync(term.Code) != null)
        {
            throw new BusinessException("Partner:ResAgreementTerm:CodeExists")
                .WithData("Code", term.Code);
        }

        await Repository.InsertAsync(term);
    }

    public virtual async Task UpdateAsync(
        ResAgreementTerm term,
        string name,
        ResAgreementTermStatus status)
    {
        // ⚠️ QUAN TRỌNG: Không có parameter code - Code không được phép sửa

        term.UpdateName(name);
        term.UpdateStatus(status);
        await Repository.UpdateAsync(term);
    }
}

