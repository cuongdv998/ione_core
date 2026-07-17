using iOne.Master.SystemEventNotifies;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResEvents;

public interface IResEventNotifyTemplateAppService
{
    Task<PagedResultDto<ResEventNotifyTemplateDto>> GetListByEventIdAsync(Guid eventId);

    Task<ResEventNotifyTemplateDto> GetAsync(Guid id);

    Task<ResEventNotifyTemplateDto> CreateAsync(CreateResEventNotifyTemplateDto input);

    Task<ResEventNotifyTemplateDto> UpdateAsync(Guid id, UpdateResEventNotifyTemplateDto input);

    Task DeleteAsync(Guid id);
}

