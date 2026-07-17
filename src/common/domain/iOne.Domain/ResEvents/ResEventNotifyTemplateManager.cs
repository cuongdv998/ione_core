using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ResEvents;

public class ResEventNotifyTemplateManager : DomainService
{
    protected IResEventNotifyTemplateRepository Repository { get; }

    public ResEventNotifyTemplateManager(IResEventNotifyTemplateRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ResEventNotifyTemplate template)
    {
        // Check unique constraint: EventId + AppChannelId
        if (await Repository.IsTemplateExistsAsync(template.EventId, template.AppChannelId))
        {
            throw new BusinessException("Master:ResEventNotifyTemplate:TemplateExists")
                .WithData("EventId", template.EventId)
                .WithData("AppChannelId", template.AppChannelId);
        }

        await Repository.InsertAsync(template);
    }

    public virtual async Task UpdateAsync(
        ResEventNotifyTemplate template,
        Guid appChannelId,
        int retryNumber,
        string title,
        string body,
        string? data)
    {
        // Check unique constraint: EventId + AppChannelId (exclude current entity)
        if (await Repository.IsTemplateExistsAsync(template.EventId, appChannelId, template.Id))
        {
            throw new BusinessException("Master:ResEventNotifyTemplate:TemplateExists")
                .WithData("EventId", template.EventId)
                .WithData("AppChannelId", appChannelId);
        }

        template.UpdateAppChannelId(appChannelId);
        template.UpdateRetryNumber(retryNumber);
        template.UpdateTitle(title);
        template.UpdateBody(body);
        template.UpdateData(data);
        await Repository.UpdateAsync(template);
    }
}

