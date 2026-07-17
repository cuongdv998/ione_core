using System;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace Volo.Abp.AuditLogging;

[RemoteService(Name = AuditLoggingRemoteServiceConsts.RemoteServiceName)]
[Area(AuditLoggingRemoteServiceConsts.ModuleName)]
[ControllerName(AuditLoggingRemoteServiceConsts.ModuleName)]
[Route("api/audit-logging/audit-logs")]
public class AuditLogController : AbpControllerBase, IAuditLogAppService
{
    protected IAuditLogAppService AuditLogAppService { get; }

    public AuditLogController(IAuditLogAppService auditLogAppService)
    {
        AuditLogAppService = auditLogAppService;
    }

    [HttpGet]
    public virtual Task<PagedResultDto<AuditLogDto>> GetListAsync(GetAuditLogsInput input)
    {
        return AuditLogAppService.GetListAsync(input);
    }

    [HttpGet]
    [Route("{id}")]
    public virtual Task<AuditLogDto> GetAsync(Guid id)
    {
        return AuditLogAppService.GetAsync(id);
    }

    [HttpGet]
    [Route("entity-changes/{entityChangeId}")]
    public virtual Task<EntityChangeDto> GetEntityChangeAsync(Guid entityChangeId)
    {
        return AuditLogAppService.GetEntityChangeAsync(entityChangeId);
    }

    [HttpGet]
    [Route("entity-changes")]
    public virtual Task<PagedResultDto<EntityChangeDto>> GetEntityChangesAsync(GetEntityChangesInput input)
    {
        return AuditLogAppService.GetEntityChangesAsync(input);
    }
}

