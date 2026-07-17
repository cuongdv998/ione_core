using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ResCarLines;

public class ResCarLineManager : DomainService
{
    protected IResCarLineRepository Repository { get; }

    public ResCarLineManager(IResCarLineRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ResCarLine carLine)
    {
        // Check code uniqueness
        if (await Repository.IsCodeExistsAsync(carLine.Code))
        {
            throw new BusinessException("Master:ResCarLine:CodeExists")
                .WithData("Code", carLine.Code);
        }

        await Repository.InsertAsync(carLine);
    }

    public virtual async Task UpdateAsync(ResCarLine carLine, string name, string? description, ResCarLineStatus status)
    {
        carLine.UpdateName(name);
        carLine.UpdateDescription(description);
        carLine.UpdateStatus(status);
        await Repository.UpdateAsync(carLine);
    }
}


