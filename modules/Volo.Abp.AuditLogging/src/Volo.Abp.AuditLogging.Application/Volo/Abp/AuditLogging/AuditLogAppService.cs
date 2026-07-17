using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Volo.Abp.AuditLogging;

[Authorize(AuditLoggingPermissions.AuditLogs.Default)]
public class AuditLogAppService : ApplicationService, IAuditLogAppService
{
    protected IAuditLogRepository AuditLogRepository { get; }

    public AuditLogAppService(IAuditLogRepository auditLogRepository)
    {
        AuditLogRepository = auditLogRepository;
    }

    [Authorize(AuditLoggingPermissions.AuditLogs.View)]
    public virtual async Task<PagedResultDto<AuditLogDto>> GetListAsync(GetAuditLogsInput input)
    {
        var (auditLogs, totalCount) = await AuditLogRepository.GetListWithCountAsync(
            sorting: input.Sorting,
            maxResultCount: input.MaxResultCount,
            skipCount: input.SkipCount,
            startTime: input.StartTime,
            endTime: input.EndTime,
            httpMethod: input.HttpMethod,
            url: input.Url,
            clientId: input.ClientId,
            userId: input.UserId,
            userName: input.UserName,
            applicationName: input.ApplicationName,
            clientIpAddress: input.ClientIpAddress,
            correlationId: input.CorrelationId,
            maxExecutionDuration: input.MaxExecutionDuration,
            minExecutionDuration: input.MinExecutionDuration,
            hasException: input.HasException,
            httpStatusCode: input.HttpStatusCode,
            includeDetails: input.IncludeDetails
        );

        return new PagedResultDto<AuditLogDto>(
            totalCount,
            ObjectMapper.Map<List<AuditLog>, List<AuditLogDto>>(auditLogs)
        );
    }

    [Authorize(AuditLoggingPermissions.AuditLogs.ViewDetails)]
    public virtual async Task<AuditLogDto> GetAsync(Guid id)
    {
        var auditLog = await AuditLogRepository.GetAsync(id);
        return ObjectMapper.Map<AuditLog, AuditLogDto>(auditLog);
    }

    [Authorize(AuditLoggingPermissions.AuditLogs.ViewEntityChanges)]
    public virtual async Task<EntityChangeDto> GetEntityChangeAsync(Guid entityChangeId)
    {
        var entityChange = await AuditLogRepository.GetEntityChange(entityChangeId);
        return ObjectMapper.Map<EntityChange, EntityChangeDto>(entityChange);
    }

    [Authorize(AuditLoggingPermissions.AuditLogs.ViewEntityChanges)]
    public virtual async Task<PagedResultDto<EntityChangeDto>> GetEntityChangesAsync(GetEntityChangesInput input)
    {
        var (entityChanges, totalCount) = await AuditLogRepository.GetEntityChangeListWithCountAsync(
            sorting: input.Sorting,
            maxResultCount: input.MaxResultCount,
            skipCount: input.SkipCount,
            auditLogId: input.AuditLogId,
            startTime: input.StartTime,
            endTime: input.EndTime,
            changeType: input.ChangeType,
            entityId: input.EntityId,
            entityTypeFullName: input.EntityTypeFullName,
            includeDetails: input.IncludeDetails
        );

        return new PagedResultDto<EntityChangeDto>(
            totalCount,
            ObjectMapper.Map<List<EntityChange>, List<EntityChangeDto>>(entityChanges)
        );
    }
}

