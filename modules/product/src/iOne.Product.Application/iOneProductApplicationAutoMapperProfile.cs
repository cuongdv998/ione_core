using AutoMapper;
using iOne.Product.ProLineOfBusinesses;
using iOne.Product.ProCoverageTypes;
using iOne.Product.ProCoverageLevelTypes;
using iOne.Product.ProCoverageGroups;
using iOne.Product.ProProductCategorys;
using iOne.Product.ProProductTypes;
using iOne.Product.ProProducts;
using iOne.Product.ProRuleTypes;
using iOne.Product.ProRules;
using iOne.Product.ProProductPlanDefinitions;
using iOne.Product.ProProductDistributions;
using iOne.Product.ProAttributes;
using iOne.Product.ProTableRates;
using iOne.Product.ProTableRateVariables;
using iOne.Product.ProTableRateLines;
using iOne.ProLineOfBusinesses;
using iOne.ProCoverageTypes;
using iOne.ProCoverageLevelTypes;
using iOne.ProCoverageGroups;
using iOne.ProProductCategorys;
using iOne.ProProductTypes;
using iOne.ProProducts;
using iOne.ProRuleTypes;
using iOne.ProRules;
using iOne.ProProductPlanDefinitions;
using iOne.ProProductDistributions;
using iOne.ProAttributes;
using iOne.ProTableRates;
using iOne.ProTableRateVariables;
using iOne.ProTableRateLines;
using iOne.ProCoverages;
using iOne.Product.ProCoverageLevelBasis;
using iOne.Product.ProCoverages;
using iOne.Product.ResTaxes;
using iOne.ResTaxes;

namespace iOne.Product;

public class iOneProductApplicationAutoMapperProfile : Profile
{
    public iOneProductApplicationAutoMapperProfile()
    {
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */

        // ProLineOfBusiness mappings
        CreateMap<ProLineOfBusiness, ProLineOfBusinessDto>();
        CreateMap<CreateProLineOfBusinessDto, ProLineOfBusiness>();
        CreateMap<UpdateProLineOfBusinessDto, ProLineOfBusiness>();

        // ProCoverageType mappings
        CreateMap<ProCoverageType, ProCoverageTypeDto>();
        CreateMap<CreateProCoverageTypeDto, ProCoverageType>();
        CreateMap<UpdateProCoverageTypeDto, ProCoverageType>();

        // ProCoverageLevelType mappings
        CreateMap<ProCoverageLevelType, ProCoverageLevelTypeDto>();
        CreateMap<CreateProCoverageLevelTypeDto, ProCoverageLevelType>();
        CreateMap<UpdateProCoverageLevelTypeDto, ProCoverageLevelType>();
        
        // ProCoverageLevelBasis mappings
        CreateMap<ProCoverageLevelBases.ProCoverageLevelBasis, ProCoverageLevelBasisDto>();
        CreateMap<ProCoverageLevelBasisDto, ProCoverageLevelBases.ProCoverageLevelBasis>();
        CreateMap<ProCoverageLevelBasisDto, ProCoverageLevelBases.ProCoverageLevelBasis>();

        // ProCoverageGroup mappings
        CreateMap<ProCoverageGroup, ProCoverageGroupDto>();
        CreateMap<CreateProCoverageGroupDto, ProCoverageGroup>();
        CreateMap<UpdateProCoverageGroupDto, ProCoverageGroup>();

        // ProProductCategory mappings
        CreateMap<ProProductCategory, ProProductCategoryDto>();
        CreateMap<CreateProProductCategoryDto, ProProductCategory>();
        CreateMap<UpdateProProductCategoryDto, ProProductCategory>();

        // ProProductType mappings
        CreateMap<ProProductType, ProProductTypeDto>();
        CreateMap<CreateProProductTypeDto, ProProductType>();
        CreateMap<UpdateProProductTypeDto, ProProductType>();

        // ProRuleType mappings
        CreateMap<ProRuleType, ProRuleTypeDto>();
        CreateMap<CreateProRuleTypeDto, ProRuleType>();
        CreateMap<UpdateProRuleTypeDto, ProRuleType>();

        // ProRule mappings
        CreateMap<ProRule, ProRuleDto>();
        CreateMap<CreateProRuleDto, ProRule>();
        CreateMap<UpdateProRuleDto, ProRule>();

        // ProProductPlanDefinition mappings
        CreateMap<ProProductPlanDefinition, ProProductPlanDefinitionDto>();
        CreateMap<CreateProProductPlanDefinitionDto, ProProductPlanDefinition>();
        CreateMap<UpdateProProductPlanDefinitionDto, ProProductPlanDefinition>();

        // ProProductDistribution mappings
        CreateMap<ProProductDistribution, ProProductDistributionDto>();
        CreateMap<CreateProProductDistributionDto, ProProductDistribution>();

        // ProProduct mappings
        CreateMap<ProProduct, ProProductDto>();
        CreateMap<ProProduct, ProProductListDto>();
        CreateMap<CreateProProductDto, ProProduct>();
        CreateMap<UpdateProProductDto, ProProduct>();

        // ProProductAttribute mappings
        CreateMap<ProProductAttribute, ProProductAttributeDto>();

        // ProProductCoverage mappings
        CreateMap<ProProductCoverage, ProProductCoverageDto>();

        // ProProductCoverageInteraction mappings
        CreateMap<ProProductCoverageInteraction, ProProductCoverageInteractionDto>();

        // ProProductCoverageLevel mappings
        CreateMap<ProProductCoverageLevel, ProProductCoverageLevelDto>();

        // ProProductCoverageLevelTerm mappings
        CreateMap<ProProductCoverageLevelTerm, ProProductCoverageLevelTermDto>();

        // ProProductTableRate mappings
        CreateMap<ProProductTableRate, ProProductTableRateDto>();

        // ProAttribute mappings
        CreateMap<ProAttribute, ProAttributeDto>();
        CreateMap<CreateProAttributeDto, ProAttribute>();
        CreateMap<UpdateProAttributeDto, ProAttribute>();

        // ResTax mappings
        CreateMap<ResTax, ResTaxDto>();
        CreateMap<CreateResTaxDto, ResTax>();
        CreateMap<UpdateResTaxDto, ResTax>();
        
        // ProCoverage mappings
        CreateMap<ProCoverage, ProCoverageDto>();
        CreateMap<CreateProCoverageDto, ProCoverage>();
        CreateMap<UpdateProCoverageDto, ProCoverage>();

        // ProTableRate mappings
        CreateMap<ProTableRate, ProTableRateDto>();
        CreateMap<CreateProTableRateDto, ProTableRate>();
        CreateMap<UpdateProTableRateDto, ProTableRate>();

        // ProTableRateVariable mappings
        CreateMap<ProTableRateVariable, ProTableRateVariableDto>();
        CreateMap<CreateProTableRateVariableDto, ProTableRateVariable>();
        CreateMap<UpdateProTableRateVariableDto, ProTableRateVariable>();

        // ProTableRateLine mappings
        CreateMap<ProTableRateLine, ProTableRateLineDto>();
        CreateMap<CreateProTableRateLineDto, ProTableRateLine>();
        CreateMap<UpdateProTableRateLineDto, ProTableRateLine>();
    }
}

