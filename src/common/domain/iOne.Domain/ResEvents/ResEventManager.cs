using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ResEvents;

public class ResEventManager : DomainService
{
    protected IResEventRepository Repository { get; }

    public ResEventManager(IResEventRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ResEvent resEvent)
    {
        // Check code uniqueness
        if (await Repository.IsCodeExistsAsync(resEvent.Code))
        {
            throw new BusinessException("ResEvent:CodeExists")
                .WithData("Code", resEvent.Code);
        }

        await Repository.InsertAsync(resEvent);
    }

    public virtual async Task UpdateAsync(ResEvent resEvent, string name, string? description, ResEventStatus status)
    {
        resEvent.UpdateName(name);
        resEvent.UpdateDescription(description);
        resEvent.UpdateStatus(status);
        await Repository.UpdateAsync(resEvent);
    }
}

