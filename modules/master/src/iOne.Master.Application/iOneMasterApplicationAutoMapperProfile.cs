using AutoMapper;
using iOne.Master.AdminConfigs;
using iOne.Master.ResCarBrands;
using iOne.Master.ResCarLines;
using iOne.Master.ResCarGroups;
using iOne.Master.ResCarModels;
using iOne.Master.ResCarTypes;
using iOne.Master.ResCarCategories;
using iOne.Master.ResAppChannels;
using iOne.Master.ResMotorClasses;
using iOne.Master.ResCountries;
using iOne.Master.ResDocumentTypes;
using iOne.Master.ResProvinces;
using iOne.Master.ResWards;
using iOne.Master.ResBanks;
using iOne.Master.ResObjectTypes;
using iOne.Master.ResRisks;
using iOne.Master.ResDamageLevels;
using iOne.Master.ResSequences;
using iOne.AdminConfigs;
using iOne.Master.ResCurrencies;
using iOne.Master.ResReasonGroups;
using iOne.Master.ResReasons;
using iOne.Master.ResFeeItems;
using iOne.Master.ResPaymentMethods;
using iOne.Master.ResPaymentTypes;
using iOne.Master.ResUomClasses;
using iOne.ResCarBrands;
using iOne.ResCarLines;
using iOne.ResCarGroups;
using iOne.ResCarModels;
using iOne.ResCarTypes;
using iOne.ResCarCategories;
using iOne.ResAppChannels;
using iOne.ResMotorClasses;
using iOne.ResCountries;
using iOne.ResDocumentTypes;
using iOne.ResProvinces;
using iOne.ResWards;
using iOne.ResBanks;
using iOne.ResCurrencies;
using iOne.ResObjectTypes;
using iOne.ResRisks;
using iOne.ResDamageLevels;
using iOne.ResSequences;
using iOne.ResReasonGroups;
using iOne.ResReasons;
using iOne.ResFeeItems;
using iOne.ResPaymentMethods;
using iOne.ResPaymentTypes;
using iOne.ResUomClasses;
using iOne.Master.ResUoms;
using iOne.ResUoms;
using iOne.Master.ResObjectItemTypes;
using iOne.ResObjectItemTypes;
using iOne.Master.ResObjectTypeItems;
using iOne.ResObjectTypeItems;
using iOne.Master.ResObjectItemDepreciations;
using iOne.ResObjectItemDepreciations;
using iOne.Master.InsurerDictionaries;
using iOne.InsurerDictionaries;
using iOne.Master.ResDocuments;
using iOne.ResDocuments;
using iOne.Master.ResEvents;
using iOne.ResEvents;
using iOne.Master.BusinessFlows;
using iOne.Master.ResBusinessAuthorities;
using iOne.Master.ResBusinessAssignees;
using iOne.Master.ResTaskCategories;
using iOne.Master.ResUserDevices;
using iOne.Master.SystemEventNotifies;
using iOne.BusinessFlows;
using iOne.ResBusinessAuthorities;
using iOne.ResBusinessAssignees;
using iOne.ResTaskCategories;
using iOne.ResUserDevices;
using iOne.SystemEventNotifies;

namespace iOne.Master;

public class iOneMasterApplicationAutoMapperProfile : Profile
{
    public iOneMasterApplicationAutoMapperProfile()
    {
        // ResCountry mappings
        CreateMap<ResCountry, ResCountryDto>();
        CreateMap<CreateResCountryDto, ResCountry>();
        CreateMap<UpdateResCountryDto, ResCountry>();

        // ResCarBrand mappings
        CreateMap<ResCarBrand, ResCarBrandDto>();
        CreateMap<CreateResCarBrandDto, ResCarBrand>();
        CreateMap<UpdateResCarBrandDto, ResCarBrand>();

        // ResCarLine mappings
        CreateMap<ResCarLine, ResCarLineDto>();
        CreateMap<CreateResCarLineDto, ResCarLine>();
        CreateMap<UpdateResCarLineDto, ResCarLine>();

        // ResCarGroup mappings
        CreateMap<ResCarGroup, ResCarGroupDto>();
        CreateMap<CreateResCarGroupDto, ResCarGroup>();
        CreateMap<UpdateResCarGroupDto, ResCarGroup>();

        // ResCarModel mappings
        CreateMap<ResCarModel, ResCarModelDto>();
        CreateMap<CreateResCarModelDto, ResCarModel>();
        CreateMap<UpdateResCarModelDto, ResCarModel>();

        // ResCarType mappings
        CreateMap<ResCarType, ResCarTypeDto>();
        CreateMap<CreateResCarTypeDto, ResCarType>();
        CreateMap<UpdateResCarTypeDto, ResCarType>();

        // ResCarCategory mappings
        CreateMap<ResCarCategory, ResCarCategoryDto>();
        CreateMap<CreateResCarCategoryDto, ResCarCategory>();
        CreateMap<UpdateResCarCategoryDto, ResCarCategory>();

        // ResAppChannel mappings
        CreateMap<ResAppChannel, ResAppChannelDto>();
        CreateMap<CreateResAppChannelDto, ResAppChannel>();
        CreateMap<UpdateResAppChannelDto, ResAppChannel>();

        // ResMotorClass mappings
        CreateMap<ResMotorClass, ResMotorClassDto>();
        CreateMap<CreateResMotorClassDto, ResMotorClass>();
        CreateMap<UpdateResMotorClassDto, ResMotorClass>();

        // ResProvince mappings
        CreateMap<ResProvince, ResProvinceDto>();
        CreateMap<CreateResProvinceDto, ResProvince>();
        CreateMap<UpdateResProvinceDto, ResProvince>();

        // ResWard mappings
        CreateMap<ResWard, ResWardDto>();
        CreateMap<CreateResWardDto, ResWard>();
        CreateMap<UpdateResWardDto, ResWard>();

        // ResDocumentType mappings
        CreateMap<ResDocumentType, ResDocumentTypeDto>();
        CreateMap<CreateResDocumentTypeDto, ResDocumentType>();
        CreateMap<UpdateResDocumentTypeDto, ResDocumentType>();

        // AdminConfig mappings
        CreateMap<AdminConfig, AdminConfigDto>();
        CreateMap<CreateAdminConfigDto, AdminConfig>();
        CreateMap<UpdateAdminConfigDto, AdminConfig>();

        // ResBank mappings
        CreateMap<ResBank, ResBankDto>();
        CreateMap<CreateResBankDto, ResBank>();
        CreateMap<UpdateResBankDto, ResBank>();

        // ResObjectType mappings
        // Note: Create/Update mappings are not used since we manually create/update entities in AppService
        CreateMap<ResObjectType, ResObjectTypeDto>()
            .ForMember(dest => dest.ObjectGroup, opt => opt.MapFrom(src => 
                ResObjectGroupHelper.FromString(src.ObjectGroup)));
        
        CreateMap<CreateResObjectTypeDto, ResObjectType>();
        CreateMap<UpdateResObjectTypeDto, ResObjectType>();

        // ResRisk mappings
        CreateMap<ResRisk, ResRiskDto>();
        CreateMap<CreateResRiskDto, ResRisk>();
        CreateMap<UpdateResRiskDto, ResRisk>();

        // ResDamageLevel mappings
        CreateMap<ResDamageLevel, ResDamageLevelDto>();
        CreateMap<CreateResDamageLevelDto, ResDamageLevel>();
        CreateMap<UpdateResDamageLevelDto, ResDamageLevel>();
        // ResSequence mappings
        CreateMap<ResSequence, ResSequenceDto>();
        CreateMap<CreateResSequenceDto, ResSequence>();
        CreateMap<UpdateResSequenceDto, ResSequence>();
        
        // ResCurrency mappings
        CreateMap<ResCurrency, ResCurrencyDto>();
        CreateMap<CreateResCurrencyDto, ResCurrency>();
        CreateMap<UpdateResCurrencyDto, ResCurrency>();
        
        // ResReasonGroup mappings
        CreateMap<ResReasonGroup, ResReasonGroupDto>();
        CreateMap<CreateResReasonGroupDto, ResReasonGroup>();
        CreateMap<UpdateResReasonGroupDto, ResReasonGroup>();
        
        // ResReason mappings
        CreateMap<ResReason, ResReasonDto>();
        CreateMap<CreateResReasonDto, ResReason>();
        CreateMap<UpdateResReasonDto, ResReason>();
        
        // ResFeeItem mappings
        CreateMap<ResFeeItem, ResFeeItemDto>();
        CreateMap<CreateResFeeItemDto, ResFeeItem>();
        CreateMap<UpdateResFeeItemDto, ResFeeItem>();
        
        // ResUomClass mappings
        CreateMap<ResUomClass, ResUomClassDto>();
        CreateMap<CreateResUomClassDto, ResUomClass>();
        CreateMap<UpdateResUomClassDto, ResUomClass>();

        // ResUom mappings
        CreateMap<ResUom, ResUomDto>();
        CreateMap<CreateResUomDto, ResUom>();
        CreateMap<UpdateResUomDto, ResUom>();
        
        // ResPaymentMethod mappings
        CreateMap<ResPaymentMethod, ResPaymentMethodDto>();
        CreateMap<CreateResPaymentMethodDto, ResPaymentMethod>();
        CreateMap<UpdateResPaymentMethodDto, ResPaymentMethod>();

        // ResPaymentType mappings
        CreateMap<ResPaymentType, ResPaymentTypeDto>();
        CreateMap<CreateResPaymentTypeDto, ResPaymentType>();
        CreateMap<UpdateResPaymentTypeDto, ResPaymentType>();
        
        // ResObjectItemType mappings
        CreateMap<ResObjectItemType, ResObjectItemTypeDto>();
        CreateMap<CreateResObjectItemTypeDto, ResObjectItemType>();
        CreateMap<UpdateResObjectItemTypeDto, ResObjectItemType>();
        
        // ResObjectTypeItem mappings
        CreateMap<ResObjectTypeItem, ResObjectTypeItemDto>();
        CreateMap<CreateResObjectTypeItemDto, ResObjectTypeItem>();
        CreateMap<UpdateResObjectTypeItemDto, ResObjectTypeItem>();
        
        // ResObjectItemDepreciation mappings
        CreateMap<ResObjectItemDepreciation, ResObjectItemDepreciationDto>();
        CreateMap<CreateResObjectItemDepreciationDto, ResObjectItemDepreciation>();
        CreateMap<UpdateResObjectItemDepreciationDto, ResObjectItemDepreciation>();
        
        // InsurerDictionary mappings
        CreateMap<InsurerDictionary, InsurerDictionaryDto>();
        CreateMap<CreateInsurerDictionaryDto, InsurerDictionary>();
        CreateMap<UpdateInsurerDictionaryDto, InsurerDictionary>();

        // ResDocument mappings
        CreateMap<ResDocument, ResDocumentDto>();

        // ResEvent mappings
        CreateMap<ResEvent, ResEventDto>();
        CreateMap<CreateResEventDto, ResEvent>();
        CreateMap<UpdateResEventDto, ResEvent>();

        // ResEventNotifyTemplate mappings
        CreateMap<ResEventNotifyTemplate, ResEventNotifyTemplateDto>();
        CreateMap<CreateResEventNotifyTemplateDto, ResEventNotifyTemplate>();
        CreateMap<UpdateResEventNotifyTemplateDto, ResEventNotifyTemplate>();

        // BusinessFlow mappings
        CreateMap<BusinessFlow, BusinessFlowDto>();
        CreateMap<CreateBusinessFlowDto, BusinessFlow>();
        CreateMap<UpdateBusinessFlowDto, BusinessFlow>();

        // ResBusinessAuthority mappings
        CreateMap<ResBusinessAuthority, ResBusinessAuthorityDto>();
        CreateMap<CreateResBusinessAuthorityDto, ResBusinessAuthority>();
        CreateMap<UpdateResBusinessAuthorityDto, ResBusinessAuthority>();

        // ResTaskCategory mappings
        CreateMap<ResTaskCategory, ResTaskCategoryDto>();
        CreateMap<CreateResTaskCategoryDto, ResTaskCategory>();
        CreateMap<UpdateResTaskCategoryDto, ResTaskCategory>();

        // ResBusinessAssignee mappings (entity created manually in AppService; update only AssigneeId)
        CreateMap<ResBusinessAssignee, ResBusinessAssigneeDto>();

        // ResUserDevice mappings
        CreateMap<ResUserDevice, ResUserDeviceDto>();
        CreateMap<CreateResUserDeviceDto, ResUserDevice>();
        // ⚠️ QUAN TRỌNG: UpdateResUserDeviceDto không có UserName, DeviceUid, AppChannelCode, EffectDate
        // Các trường này sẽ được ignore khi map (không được phép sửa)
        CreateMap<UpdateResUserDeviceDto, ResUserDevice>()
            .ForMember(dest => dest.UserName, opt => opt.Ignore())
            .ForMember(dest => dest.DeviceUid, opt => opt.Ignore())
            .ForMember(dest => dest.AppChannelCode, opt => opt.Ignore())
            .ForMember(dest => dest.EffectDate, opt => opt.Ignore());

        // SystemEventNotify mappings
        CreateMap<SystemEventNotify, SystemEventNotifyDto>();
        CreateMap<CreateSystemEventNotifyDto, SystemEventNotify>();
        CreateMap<UpdateSystemEventNotifyDto, SystemEventNotify>();
    }
}

