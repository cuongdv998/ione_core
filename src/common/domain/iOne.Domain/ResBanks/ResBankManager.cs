using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ResBanks;

public class ResBankManager : DomainService
{
    protected IResBankRepository Repository { get; }

    public ResBankManager(IResBankRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ResBank bank)
    {
        // Check code uniqueness
        if (await Repository.IsCodeExistsAsync(bank.Code))
        {
            throw new BusinessException("Master:ResBank:CodeExists")
                .WithData("Code", bank.Code);
        }

        await Repository.InsertAsync(bank);
    }

    public virtual async Task UpdateAsync(ResBank bank, string name, ResBankStatus status)
    {
        // ⚠️ QUAN TRỌNG: Không có parameter code - Code không được phép sửa
        bank.UpdateName(name);
        bank.UpdateStatus(status);
        await Repository.UpdateAsync(bank);
    }
}

