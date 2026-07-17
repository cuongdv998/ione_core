using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Volo.Abp.AuditLogging;

public interface IAuditLogAppService : IApplicationService
{
    Task<PagedResultDto<AuditLogDto>> GetListAsync(GetAuditLogsInput input);
    
    Task<AuditLogDto> GetAsync(Guid id);
    
    Task<EntityChangeDto> GetEntityChangeAsync(Guid entityChangeId);
    
    Task<PagedResultDto<EntityChangeDto>> GetEntityChangesAsync(GetEntityChangesInput input);
}

