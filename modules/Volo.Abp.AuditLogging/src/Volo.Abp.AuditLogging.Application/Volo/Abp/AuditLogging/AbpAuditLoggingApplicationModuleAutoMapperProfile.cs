using AutoMapper;
using Volo.Abp.AutoMapper;

namespace Volo.Abp.AuditLogging;

public class AbpAuditLoggingApplicationModuleAutoMapperProfile : Profile
{
    public AbpAuditLoggingApplicationModuleAutoMapperProfile()
    {
        CreateMap<AuditLog, AuditLogDto>();
        CreateMap<EntityChange, EntityChangeDto>();
        CreateMap<EntityPropertyChange, EntityPropertyChangeDto>();
        CreateMap<AuditLogAction, AuditLogActionDto>();
    }
}

