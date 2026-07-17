using AutoMapper;
using iOne.Policy.Policies;
using iOne.Policy.PolicyAmounts;
using iOne.Policy.PolicyCertificates;
using iOne.Policy.PolicyContracts;
using iOne.Policy.PolicyCoverageLevels;
using iOne.Policy.PolicyCoverages;
using iOne.Policy.PolicyProducts;
using iOne.Policy.PolicyRiskMotors;
using iOne.Policy.PolicyRiskObjects;
using iOne.Policy.PolicyTypes;
using iOne.Policy.PolicyVersions;
using iOne.Policies;
using iOne.PolicyContracts;
using iOne.PolicyTypes;

namespace iOne.Policy;

public class iOnePolicyApplicationAutoMapperProfile : Profile
{
    public iOnePolicyApplicationAutoMapperProfile()
    {
        // PolicyType mappings
        CreateMap<PolicyType, PolicyTypeDto>();
        CreateMap<CreatePolicyTypeDto, PolicyType>();
        CreateMap<UpdatePolicyTypeDto, PolicyType>();

        // Policy mappings
        CreateMap<iOne.Policies.Policy, PolicyDto>()
            // These are populated manually in PolicyAppService.GetAsync (detail endpoint)
            .ForMember(d => d.Contract, opt => opt.Ignore())
            .ForMember(d => d.VersionDetail, opt => opt.Ignore())
            .ForMember(d => d.Amount, opt => opt.Ignore())
            .ForMember(d => d.Products, opt => opt.Ignore())
            .ForMember(d => d.RiskObject, opt => opt.Ignore())
            .ForMember(d => d.Documents, opt => opt.Ignore());
        CreateMap<CreatePolicyDto, iOne.Policies.Policy>();
        // UpdatePolicyDetailDto is handled manually in PolicyAppService.UpdateAsync

        // PolicyContract mappings (entity.PayerTin -> dto.PayerTaxCode)
        CreateMap<iOne.PolicyContracts.PolicyContract, PolicyContractDto>()
            .ForMember(d => d.PayerTaxCode, opt => opt.MapFrom(s => s.PayerTin));
        CreateMap<CreatePolicyContractDto, iOne.PolicyContracts.PolicyContract>();
        CreateMap<UpdatePolicyContractDto, iOne.PolicyContracts.PolicyContract>();

        // PolicyVersion mappings
        CreateMap<PolicyVersion, PolicyVersionDto>();
        CreateMap<CreatePolicyVersionDto, PolicyVersion>();
        CreateMap<UpdatePolicyVersionDto, PolicyVersion>();

        // PolicyCertificate mappings
        CreateMap<PolicyCertificate, PolicyCertificateDto>();
        CreateMap<CreatePolicyCertificateDto, PolicyCertificate>();
        CreateMap<UpdatePolicyCertificateDto, PolicyCertificate>();

        // PolicyRiskObject mappings
        CreateMap<PolicyRiskObject, PolicyRiskObjectDto>();
        CreateMap<CreatePolicyRiskObjectDto, PolicyRiskObject>();
        CreateMap<UpdatePolicyRiskObjectDto, PolicyRiskObject>();

        // PolicyRiskMotor mappings
        CreateMap<PolicyRiskMotor, PolicyRiskMotorDto>();
        CreateMap<CreatePolicyRiskMotorDto, PolicyRiskMotor>();
        CreateMap<UpdatePolicyRiskMotorDto, PolicyRiskMotor>();

        // PolicyProduct mappings
        CreateMap<PolicyProduct, PolicyProductDto>();
        CreateMap<CreatePolicyProductDto, PolicyProduct>();
        CreateMap<UpdatePolicyProductDto, PolicyProduct>();

        // PolicyCoverage mappings
        CreateMap<PolicyCoverage, PolicyCoverageDto>();
        CreateMap<CreatePolicyCoverageDto, PolicyCoverage>();
        CreateMap<UpdatePolicyCoverageDto, PolicyCoverage>();

        // PolicyCoverageLevel mappings
        CreateMap<PolicyCoverageLevel, PolicyCoverageLevelDto>();
        CreateMap<CreatePolicyCoverageLevelDto, PolicyCoverageLevel>();
        CreateMap<UpdatePolicyCoverageLevelDto, PolicyCoverageLevel>();

        // PolicyAmount mappings
        CreateMap<PolicyAmount, PolicyAmountDto>();
        CreateMap<CreatePolicyAmountDto, PolicyAmount>();
        CreateMap<UpdatePolicyAmountDto, PolicyAmount>();
    }
}

