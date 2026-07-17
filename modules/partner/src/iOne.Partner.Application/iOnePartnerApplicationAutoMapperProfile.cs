using AutoMapper;
using iOne.Partner.ResAgreementTerms;
using iOne.Partner.ResChannels;
using iOne.Partner.ResOrganizationTypes;
using iOne.Partner.ResPartners;
using iOne.Partner.ResPartnerTypes;
using iOne.ResAgreementTerms;
using iOne.ResChannels;
using iOne.ResOrganizationTypes;
using iOne.ResPartners;
using iOne.ResPartnerTypes;

namespace iOne.Partner;

public class iOnePartnerApplicationAutoMapperProfile : Profile
{
    public iOnePartnerApplicationAutoMapperProfile()
    {
        // ResPartnerType mappings
        CreateMap<ResPartnerType, ResPartnerTypeDto>();
        CreateMap<CreateResPartnerTypeDto, ResPartnerType>();
        CreateMap<UpdateResPartnerTypeDto, ResPartnerType>();

        // ResOrganizationType mappings
        CreateMap<ResOrganizationType, ResOrganizationTypeDto>();
        CreateMap<CreateResOrganizationTypeDto, ResOrganizationType>();
        CreateMap<UpdateResOrganizationTypeDto, ResOrganizationType>();

        // ResChannel mappings
        CreateMap<ResChannel, ResChannelDto>();
        CreateMap<CreateResChannelDto, ResChannel>();
        CreateMap<UpdateResChannelDto, ResChannel>();

        // ResAgreementTerm mappings
        CreateMap<ResAgreementTerm, ResAgreementTermDto>();
        CreateMap<CreateResAgreementTermDto, ResAgreementTerm>();
        CreateMap<UpdateResAgreementTermDto, ResAgreementTerm>();

        // ResPartner mappings
        CreateMap<ResPartner, ResPartnerDto>();
        CreateMap<CreateResPartnerDto, ResPartner>();
        CreateMap<UpdateResPartnerDto, ResPartner>();

        // ResPartnerAgreement mappings
        CreateMap<ResPartnerAgreement, ResPartnerAgreementDto>();
        CreateMap<CreateResPartnerAgreementDto, ResPartnerAgreement>();
        CreateMap<UpdateResPartnerAgreementDto, ResPartnerAgreement>();
    }
}

