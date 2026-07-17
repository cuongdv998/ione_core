using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ResChannels;

public class ResChannelManager : DomainService
{
    protected IResChannelRepository Repository { get; }

    public ResChannelManager(IResChannelRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ResChannel channel)
    {
        // Check code uniqueness
        if (await Repository.FindByCodeAsync(channel.Code) != null)
        {
            throw new BusinessException("Partner:ResChannel:CodeExists")
                .WithData("Code", channel.Code);
        }

        await Repository.InsertAsync(channel);
    }

    public virtual async Task UpdateAsync(
        ResChannel channel,
        string name,
        ResChannelStatus status,
        string? description = null)
    {
        // ⚠️ QUAN TRỌNG: Không có parameter code - Code không được phép sửa

        channel.UpdateName(name);
        channel.UpdateStatus(status);
        channel.UpdateDescription(description);
        await Repository.UpdateAsync(channel);
    }
}

