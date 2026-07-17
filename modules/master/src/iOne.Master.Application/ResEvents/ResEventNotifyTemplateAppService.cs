using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iOne.Master;
using iOne.Master.Localization;
using iOne.Master.Permissions;
using iOne.Master.ResEvents;
using iOne.ResAppChannels;
using iOne.ResEvents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResEvents;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Authorize(ResEventNotifyTemplatePermissions.Default)]
public class ResEventNotifyTemplateAppService : ApplicationService, IResEventNotifyTemplateAppService
{
    protected IResEventNotifyTemplateRepository TemplateRepository { get; }
    protected ResEventNotifyTemplateManager Manager { get; }
    protected IResAppChannelRepository AppChannelRepository { get; }

    public ResEventNotifyTemplateAppService(
        IResEventNotifyTemplateRepository templateRepository,
        ResEventNotifyTemplateManager manager,
        IResAppChannelRepository appChannelRepository)
    {
        TemplateRepository = templateRepository;
        Manager = manager;
        AppChannelRepository = appChannelRepository;
        LocalizationResource = typeof(MasterResource);
    }

    public async Task<PagedResultDto<ResEventNotifyTemplateDto>> GetListByEventIdAsync(Guid eventId)
    {
        var templates = await TemplateRepository.GetListByEventIdAsync(eventId);
        
        var dtos = new List<ResEventNotifyTemplateDto>();
        foreach (var template in templates)
        {
            var dto = ObjectMapper.Map<ResEventNotifyTemplate, ResEventNotifyTemplateDto>(template);
            
            // Map AppChannel info
            if (template.AppChannel != null)
            {
                dto.AppChannelCode = template.AppChannel.Code;
                dto.AppChannelName = template.AppChannel.Name;
            }
            
            dtos.Add(dto);
        }

        return new PagedResultDto<ResEventNotifyTemplateDto>
        {
            TotalCount = dtos.Count,
            Items = dtos
        };
    }

    [Authorize(ResEventNotifyTemplatePermissions.View)]
    public async Task<ResEventNotifyTemplateDto> GetAsync(Guid id)
    {
        // Repository.GetAsync will throw exception if not found, never returns null
        var template = await TemplateRepository.GetAsync(id);
        
        var dto = ObjectMapper.Map<ResEventNotifyTemplate, ResEventNotifyTemplateDto>(template);
        
        // Load AppChannel if not loaded
        if (template.AppChannel == null && template.AppChannelId != Guid.Empty)
        {
            var appChannel = await AppChannelRepository.GetAsync(template.AppChannelId);
            dto.AppChannelCode = appChannel.Code;
            dto.AppChannelName = appChannel.Name;
        }
        else if (template.AppChannel != null)
        {
            dto.AppChannelCode = template.AppChannel.Code;
            dto.AppChannelName = template.AppChannel.Name;
        }
        
        return dto;
    }

    [Authorize(ResEventNotifyTemplatePermissions.Create)]
    public async Task<ResEventNotifyTemplateDto> CreateAsync(CreateResEventNotifyTemplateDto input)
    {
        var entity = new ResEventNotifyTemplate(
            GuidGenerator.Create(),
            input.EventId,
            input.AppChannelId,
            input.RetryNumber,
            input.Title,
            input.Body,
            input.Data
        );

        await Manager.CreateAsync(entity);

        // Load AppChannel for mapping
        var appChannel = await AppChannelRepository.GetAsync(entity.AppChannelId);
        var dto = ObjectMapper.Map<ResEventNotifyTemplate, ResEventNotifyTemplateDto>(entity);
        dto.AppChannelCode = appChannel.Code;
        dto.AppChannelName = appChannel.Name;
        
        return dto;
    }

    [Authorize(ResEventNotifyTemplatePermissions.Edit)]
    public async Task<ResEventNotifyTemplateDto> UpdateAsync(Guid id, UpdateResEventNotifyTemplateDto input)
    {
        var entity = await TemplateRepository.GetAsync(id);

        await Manager.UpdateAsync(
            entity,
            input.AppChannelId,
            input.RetryNumber,
            input.Title,
            input.Body,
            input.Data
        );

        // Load AppChannel for mapping
        var appChannel = await AppChannelRepository.GetAsync(input.AppChannelId);
        var dto = ObjectMapper.Map<ResEventNotifyTemplate, ResEventNotifyTemplateDto>(entity);
        dto.AppChannelCode = appChannel.Code;
        dto.AppChannelName = appChannel.Name;
        
        return dto;
    }

    [Authorize(ResEventNotifyTemplatePermissions.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var entity = await TemplateRepository.GetAsync(id);
        await TemplateRepository.DeleteAsync(entity);
    }
}

