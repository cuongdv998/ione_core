using System;
using AutoMapper;

namespace iOne.Claim;

public class iOneClaimApplicationAutoMapperProfile : Profile
{
    public iOneClaimApplicationAutoMapperProfile()
    {
        CreateMap<iOne.Claims.Claim, Claim.Claims.ClaimDto>();
        CreateMap<iOne.ClaimFolders.ClaimFolder, Claim.Claims.ClaimFolderDto>()
            .ForMember(d => d.InsurerName, opt => opt.Ignore())
            .ForMember(d => d.OpenEmployeeName, opt => opt.Ignore());
        CreateMap<iOne.ClaimFolders.ClaimFolder, Claim.Claims.ClaimFolderListDto>()
            .ForMember(d => d.FolderNo, opt => opt.MapFrom(s => s.FolderNo))
            .ForMember(d => d.FolderName, opt => opt.MapFrom(s => s.FolderName))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status))
            .ForMember(d => d.Priority, opt => opt.MapFrom(s => s.Priority))
            .ForMember(d => d.OpenDate, opt => opt.MapFrom(s => s.OpenDate));
        CreateMap<iOne.SystemEventNotifies.SystemEventNotify, Claim.Claims.ClaimSentMessageDto>()
            .ForMember(d => d.CreationTime, opt => opt.MapFrom(s => s.CreationTime))
            .ForMember(d => d.Recipient, opt => opt.MapFrom(s => s.Recipient))
            .ForMember(d => d.ChannelName, opt => opt.Ignore());
        CreateMap<iOne.ClaimDocuments.ClaimDocument, Claim.Claims.ClaimDocumentDto>()
            .ForMember(d => d.DocumentId, opt => opt.MapFrom(s => s.DocumentId ?? Guid.Empty))
            .ForMember(d => d.DocumentGroupCode, opt => opt.Ignore());
    }
}

