using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ResTaxes;

public class ResTaxManager : DomainService
{
    protected IResTaxRepository Repository { get; }

    public ResTaxManager(IResTaxRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ResTax tax)
    {
        // Check code uniqueness
        if (await Repository.IsCodeExistsAsync(tax.Code))
        {
            throw new BusinessException("ResTax:CodeExists")
                .WithData("Code", tax.Code);
        }

        await Repository.InsertAsync(tax);
    }

    public virtual async Task UpdateAsync(
        ResTax tax,
        string name,
        decimal value,
        ResTaxStatus status,
        string? description = null)
    {
        // ⚠️ QUAN TRỌNG: Không có parameter code - Code không được phép sửa
        tax.UpdateName(name);
        tax.UpdateValue(value);
        tax.UpdateStatus(status);
        tax.UpdateDescription(description);
        await Repository.UpdateAsync(tax);
    }
}

