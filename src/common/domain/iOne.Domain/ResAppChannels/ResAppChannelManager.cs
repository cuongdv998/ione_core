using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ResAppChannels;

public class ResAppChannelManager : DomainService
{
    protected IResAppChannelRepository Repository { get; }

    public ResAppChannelManager(IResAppChannelRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ResAppChannel appChannel)
    {
        // Check code uniqueness
        if (await Repository.IsCodeExistsAsync(appChannel.Code))
        {
            throw new BusinessException("Master:ResAppChannel:CodeExists")
                .WithData("Code", appChannel.Code);
        }

        await Repository.InsertAsync(appChannel);
    }

    public virtual async Task UpdateAsync(ResAppChannel appChannel, string name, string? description, ResAppChannelStatus status, ResAppChannelType? type = null)
    {
        appChannel.UpdateName(name);
        appChannel.UpdateDescription(description);
        appChannel.UpdateStatus(status);
        appChannel.UpdateType(type);
        await Repository.UpdateAsync(appChannel);
    }
}
