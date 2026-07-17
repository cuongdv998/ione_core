using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using iOne.Product;
using iOne.Product.ProProducts;
using iOne.Product.Localization;
using iOne.Product.Permissions;
using iOne.ProProducts; // For Entity, Manager, Repository
using iOne.ProAttributes;
using iOne.ProCoverages;
using iOne.Product.ProCoverages;
using iOne.ResTaxes;
using iOne.Product.ResTaxes;
using iOne.ProTableRates;
using iOne.ProRules;
using iOne.Product.ProRules;
using iOne.ProRuleTypes;
using iOne.Product.ProProductPlanDefinitions;
using iOne.ProProductPlanDefinitions;
using iOne.Product.ProProductDistributions;
using iOne.ProProductDistributions;
using iOne.ProProductTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using iOne.ResAppChannels;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Http;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using iOne.ResPartners;
using iOne.HrEmployees;

namespace iOne.Product.ProProducts;

[RemoteService(Name = ProductRemoteServiceConsts.RemoteServiceName)]
[Authorize(ProProductPermissions.Default)]
public class ProProductAppService : CrudAppService<
    ProProduct,
    ProProductDto,
    Guid,
    GetProProductsInput,
    CreateProProductDto,
    UpdateProProductDto>, IProProductAppService
{
    protected ProProductManager Manager { get; }
    protected IProProductRepository ProductRepository { get; }
    protected IRepository<ProProductAttribute, Guid> ProductAttributeRepository { get; }
    protected IRepository<ProAttribute, Guid> AttributeRepository { get; }
    protected IRepository<ProProductCoverage, Guid> ProductCoverageRepository { get; }
    protected IRepository<ProProductCoverageInteraction, Guid> ProductCoverageInteractionRepository { get; }
    protected IRepository<ProProductCoverageLevel, Guid> ProductCoverageLevelRepository { get; }
    protected IRepository<ProProductCoverageLevelTerm, Guid> ProductCoverageLevelTermRepository { get; }
    protected IRepository<ProCoverage, Guid> CoverageRepository { get; }
    protected IRepository<ResTax, Guid> TaxRepository { get; }
    protected IRepository<ProProductTableRate, Guid> ProductTableRateRepository { get; }
    protected IRepository<ProTableRate, Guid> TableRateRepository { get; }
    protected IRepository<ProRule, Guid> ProRuleRepository { get; }
    protected IRepository<ProProductPlanDefinition, Guid> ProductPlanDefinitionRepository { get; }
    protected IRepository<ProProductDistribution, Guid> ProductDistributionRepository { get; }
    protected IRepository<ProProductType, Guid> ProductTypeRepository { get; }
    protected IRepository<IdentityUser, Guid> UserRepository { get; }
    protected IRepository<ResAppChannel, Guid> ResAppChannelRepository { get; }
    protected IResPartnerRepository ResPartnerRepository { get; }
    protected IRepository<HrEmployee, Guid> HrEmployeeRepository { get; }
    protected IRepository<HrEmployeeRoleRel, Guid> HrEmployeeRoleRelRepository { get; }

    public ProProductAppService(
        IProProductRepository repository,
        ProProductManager manager,
        IRepository<ProProductAttribute, Guid> productAttributeRepository,
        IRepository<ProAttribute, Guid> attributeRepository,
        IRepository<ProProductCoverage, Guid> productCoverageRepository,
        IRepository<ProProductCoverageInteraction, Guid> productCoverageInteractionRepository,
        IRepository<ProProductCoverageLevel, Guid> productCoverageLevelRepository,
        IRepository<ProProductCoverageLevelTerm, Guid> productCoverageLevelTermRepository,
        IRepository<ProCoverage, Guid> coverageRepository,
        IRepository<ResTax, Guid> taxRepository,
        IRepository<ProProductTableRate, Guid> productTableRateRepository,
        IRepository<ProTableRate, Guid> tableRateRepository,
        IRepository<ProRule, Guid> proRuleRepository,
        IRepository<ProProductPlanDefinition, Guid> productPlanDefinitionRepository,
        IRepository<ProProductDistribution, Guid> productDistributionRepository,
        IRepository<ProProductType, Guid> productTypeRepository,
        IRepository<IdentityUser, Guid> userRepository,
        IRepository<ResAppChannel, Guid> resAppChannelRepository,
        IResPartnerRepository resPartnerRepository,
        IRepository<HrEmployee, Guid> hrEmployeeRepository,
        IRepository<HrEmployeeRoleRel, Guid> hrEmployeeRoleRelRepository)
        : base(repository)
    {
        Manager = manager;
        ProductRepository = repository;
        ProductAttributeRepository = productAttributeRepository;
        AttributeRepository = attributeRepository;
        ProductCoverageRepository = productCoverageRepository;
        ProductCoverageInteractionRepository = productCoverageInteractionRepository;
        ProductCoverageLevelRepository = productCoverageLevelRepository;
        ProductCoverageLevelTermRepository = productCoverageLevelTermRepository;
        CoverageRepository = coverageRepository;
        TaxRepository = taxRepository;
        ProductTableRateRepository = productTableRateRepository;
        TableRateRepository = tableRateRepository;
        ProRuleRepository = proRuleRepository;
        ProductPlanDefinitionRepository = productPlanDefinitionRepository;
        ProductDistributionRepository = productDistributionRepository;
        ProductTypeRepository = productTypeRepository;
        UserRepository = userRepository;
        ResAppChannelRepository = resAppChannelRepository;
        ResPartnerRepository = resPartnerRepository;
        HrEmployeeRepository = hrEmployeeRepository;
        HrEmployeeRoleRelRepository = hrEmployeeRoleRelRepository;
        LocalizationResource = typeof(ProductResource);
        GetPolicyName = ProProductPermissions.View;
        GetListPolicyName = ProProductPermissions.View;
        CreatePolicyName = ProProductPermissions.Create;
        UpdatePolicyName = ProProductPermissions.Edit;
        DeletePolicyName = ProProductPermissions.Delete;
    }

    public override async Task<ProProductDto> CreateAsync(CreateProProductDto input)
    {
        // Validate duplicate code
        var normalizedCode = input.Code?.Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(normalizedCode))
        {
            if (await ProductRepository.IsCodeExistsAsync(normalizedCode))
            {
                throw new UserFriendlyException(
                    L["ProProduct:CodeExists"].Value.Replace("{Code}", normalizedCode));
            }
        }

        var entity = new ProProduct(
            GuidGenerator.Create(),
            input.LobId,
            input.Code,
            input.ShortName,
            input.Name,
            input.Status,
            input.EffectDate,
            input.ProductTypeId,
            input.PartnerId,
            input.TableRateId,
            input.RootProductId,
            input.IsRootProduct,
            input.ProductCategoryId,
            input.CurrencyId,
            input.InsurerProductCode,
            input.Description,
            input.InternalNote,
            input.RateType,
            input.SeqNumber,
            input.ExpireDate,
            input.PlanDefinitionId,
            input.IsPlan,
            input.ImageDocumentId,
            input.CertificateTemplateDocumentId
        );

        await Manager.CreateAsync(entity);

        // Create ProProductAttributes if provided
        if (input.ProductAttributes != null && input.ProductAttributes.Any())
        {
            foreach (var attributeDto in input.ProductAttributes)
            {
                // Validate AttributeId
                await AttributeRepository.GetAsync(attributeDto.AttributeId);

                var productAttribute = new ProProductAttribute(
                    GuidGenerator.Create(),
                    entity.Id,
                    attributeDto.AttributeId,
                    !string.IsNullOrWhiteSpace(attributeDto.IsRequired) ? attributeDto.IsRequired : "N",
                    attributeDto.Status
                );

                await ProductAttributeRepository.InsertAsync(productAttribute);
            }
        }

        // Create ProProductCoverages if provided
        if (input.ProductCoverages != null && input.ProductCoverages.Any())
        {
            ValidateProductCoveragesNoOverlappingDates(input.ProductCoverages);

            // Sort coverages by dependency order (parents before children)
            var sortedCoverages = SortCoveragesByDependency(input.ProductCoverages);
            
            // Dictionary to map coverage DTO to created entity
            // Use a combination of CoverageId and SeqNumber as key since new coverages don't have stable IDs
            var coverageMapping = new Dictionary<ProProductCoverageDto, ProProductCoverage>();
            
            // First pass: Create all coverages without ParentId to avoid foreign key violations
            foreach (var coverageDto in sortedCoverages)
            {
                // Validate CoverageId
                await CoverageRepository.GetAsync(coverageDto.CoverageId);
                
                // Validate TaxId
                await TaxRepository.GetAsync(coverageDto.TaxId);

                // Create coverage without ParentId first
                var productCoverage = new ProProductCoverage(
                    GuidGenerator.Create(),
                    entity.Id,
                    coverageDto.CoverageId,
                    coverageDto.AvailabilityType,
                    coverageDto.EffectDate,
                    coverageDto.TaxId,
                    coverageDto.SeqNumber,
                    null, // Set ParentId to null initially
                    coverageDto.InsurerCoverageCode,
                    coverageDto.UomId,
                    coverageDto.ExpireDate,
                    coverageDto.EnableQuantity
                );

                await ProductCoverageRepository.InsertAsync(productCoverage);
                coverageMapping[coverageDto] = productCoverage;
            }
            
            // Second pass: Update ParentId for coverages that have parent references
            foreach (var coverageDto in sortedCoverages)
            {
                var productCoverage = coverageMapping[coverageDto];
                
                // Resolve ParentId
                if (coverageDto.ParentId.HasValue)
                {
                    Guid? resolvedParentId = null;
                    
                    // Strategy 1: Check if ParentId matches any created entity's ID in this batch
                    // (This handles cases where frontend might use temporary IDs that match generated IDs)
                    var parentEntityInBatch = coverageMapping.Values
                        .FirstOrDefault(c => c.Id == coverageDto.ParentId.Value && c.Id != productCoverage.Id);
                    
                    if (parentEntityInBatch != null)
                    {
                        resolvedParentId = parentEntityInBatch.Id;
                    }
                    else
                    {
                        // Strategy 2: Check if ParentId matches another coverage DTO's id field
                        var parentCoverageDto = input.ProductCoverages
                            .FirstOrDefault(c => c.Id == coverageDto.ParentId.Value && c != coverageDto);
                        
                        if (parentCoverageDto != null && coverageMapping.ContainsKey(parentCoverageDto))
                        {
                            resolvedParentId = coverageMapping[parentCoverageDto].Id;
                        }
                        else
                        {
                            // Strategy 3: Check if ParentId exists in database (for this product)
                            var parentCoverageQuery = await ProductCoverageRepository.GetQueryableAsync();
                            var parentCoverageInDb = await parentCoverageQuery
                                .FirstOrDefaultAsync(x => x.Id == coverageDto.ParentId.Value && x.ProductId == entity.Id);
                            
                            if (parentCoverageInDb != null)
                            {
                                resolvedParentId = coverageDto.ParentId.Value;
                            }
                        }
                    }
                    
                    if (resolvedParentId.HasValue)
                    {
                        productCoverage.UpdateParentId(resolvedParentId.Value);
                        await ProductCoverageRepository.UpdateAsync(productCoverage);
                    }
                    else
                    {
                        // ParentId doesn't exist in database or in the same batch
                        throw new UserFriendlyException(
                            L["Product:ProProductCoverage:InvalidParentId", coverageDto.ParentId.Value, coverageDto.CoverageId]);
                    }
                }

                // Create ProProductCoverageInteractions if provided
                if (coverageDto.ProductCoverageInteractions != null && coverageDto.ProductCoverageInteractions.Any())
                {
                    foreach (var interactionDto in coverageDto.ProductCoverageInteractions)
                    {
                        // Skip interactions with invalid InteractionCoverageId (empty, null, or Guid.Empty)
                        if (interactionDto.InteractionCoverageId == Guid.Empty)
                        {
                            continue;
                        }

                        // Validate InteractionCoverageId (must be a valid ProProductCoverage)
                        Guid? resolvedInteractionCoverageId = null;

                        // Strategy 1: Check if it's in the coverageMapping (created in this batch)
                        var interactionCoverage = coverageMapping.Values
                            .FirstOrDefault(c => c.Id == interactionDto.InteractionCoverageId);

                        if (interactionCoverage != null)
                        {
                            resolvedInteractionCoverageId = interactionCoverage.Id;
                        }
                        else
                        {
                            // Strategy 2: Check if InteractionCoverageId matches another coverage DTO's id field
                            var interactionCoverageDto = input.ProductCoverages
                                .FirstOrDefault(c => c.Id == interactionDto.InteractionCoverageId && c != coverageDto);
                            
                            if (interactionCoverageDto != null && coverageMapping.ContainsKey(interactionCoverageDto))
                            {
                                resolvedInteractionCoverageId = coverageMapping[interactionCoverageDto].Id;
                            }
                            else
                            {
                                // Strategy 3: Check if InteractionCoverageId exists in database (for this product)
                                var interactionCoverageQuery = await ProductCoverageRepository.GetQueryableAsync();
                                interactionCoverage = await interactionCoverageQuery
                                    .FirstOrDefaultAsync(x => x.Id == interactionDto.InteractionCoverageId && x.ProductId == entity.Id);
                                
                                if (interactionCoverage != null)
                                {
                                    resolvedInteractionCoverageId = interactionCoverage.Id;
                                }
                            }
                        }

                        if (!resolvedInteractionCoverageId.HasValue)
                        {
                            throw new UserFriendlyException(
                                L["Product:ProProductCoverageInteraction:InvalidInteractionCoverageId", interactionDto.InteractionCoverageId]);
                        }

                        var productCoverageInteraction = new ProProductCoverageInteraction(
                            GuidGenerator.Create(),
                            productCoverage.Id,
                            interactionDto.InteractionType,
                            resolvedInteractionCoverageId.Value,
                            interactionDto.EffectDate,
                            interactionDto.ExpireDate
                        );

                        await ProductCoverageInteractionRepository.InsertAsync(productCoverageInteraction);
                    }
                }

                // Create ProProductCoverageLevels if provided
                if (coverageDto.ProductCoverageLevels != null && coverageDto.ProductCoverageLevels.Any())
                {
                    foreach (var levelDto in coverageDto.ProductCoverageLevels)
                    {
                        var productCoverageLevel = new ProProductCoverageLevel(
                            GuidGenerator.Create(),
                            levelDto.Code,
                            levelDto.Name,
                            levelDto.EffectDate,
                            productCoverage.Id,
                            levelDto.ExpireDate,
                            levelDto.ConditionalScript
                        );

                        await ProductCoverageLevelRepository.InsertAsync(productCoverageLevel);

                        // Create ProProductCoverageLevelTerms if provided
                        if (levelDto.Terms != null && levelDto.Terms.Any())
                        {
                            foreach (var termDto in levelDto.Terms)
                            {
                                // Validate CoverageLevelTypeId
                                var coverageLevelTypeQuery = await ProductCoverageLevelTermRepository.GetQueryableAsync();
                                // Note: We need to validate against ProCoverageLevelType repository, but for now we'll skip validation
                                // as it's a foreign key constraint that will be enforced by the database

                                var productCoverageLevelTerm = new ProProductCoverageLevelTerm(
                                    GuidGenerator.Create(),
                                    termDto.CoverageLevelTypeId,
                                    termDto.AmountType,
                                    termDto.FromAmount,
                                    termDto.ToAmount,
                                    productCoverageLevel.Id,
                                    termDto.CoverageLevelBasisId,
                                    termDto.ConditionScript,
                                    termDto.ComputeScript,
                                    termDto.IsDefault
                                );

                                await ProductCoverageLevelTermRepository.InsertAsync(productCoverageLevelTerm);
                            }
                        }
                    }
                }
            }
        }

        // Create ProProductTableRates if provided
        if (input.ProductTableRates != null && input.ProductTableRates.Any())
        {
            foreach (var tableRateDto in input.ProductTableRates)
            {
                // Validate TableRateId
                await TableRateRepository.GetAsync(tableRateDto.TableRateId);

                var productTableRate = new ProProductTableRate(
                    GuidGenerator.Create(),
                    entity.Id,
                    tableRateDto.TableRateId,
                    tableRateDto.EffectDate,
                    tableRateDto.ExpireDate
                );

                await ProductTableRateRepository.InsertAsync(productTableRate);
            }
        }

        // Create ProRules if provided
        if (input.ProRules != null && input.ProRules.Any())
        {
            foreach (var ruleDto in input.ProRules)
            {
                // Validate RuleTypeId
                var ruleTypeRepository = LazyServiceProvider.LazyGetRequiredService<IRepository<iOne.ProRuleTypes.ProRuleType, Guid>>();
                await ruleTypeRepository.GetAsync(ruleDto.RuleTypeId);

                var proRule = new ProRule(
                    GuidGenerator.Create(),
                    "product", // ApplyTo = "product" for ProProduct
                    entity.Id, // ApplyToId = ProProduct.id
                    ruleDto.RuleTypeId,
                    ruleDto.Code,
                    ruleDto.Name,
                    ruleDto.RuleScript,
                    ruleDto.Status,
                    ruleDto.EffectDate,
                    ruleDto.Description,
                    ruleDto.Priority,
                    ruleDto.ExpireDate
                );

                await ProRuleRepository.InsertAsync(proRule);
            }
        }

        // Create ProProductPlanDefinitions if provided
        // Only able to create ProProductPlanDefinition if ProProduct.PartnerId = null
        if (input.ProductPlanDefinitions != null && input.ProductPlanDefinitions.Any())
        {
            if (entity.PartnerId != null)
            {
                throw new UserFriendlyException(
                    L["Product:ProProductPlanDefinition:ProductHasPartner", entity.Id]);
            }

            foreach (var planDefinitionDto in input.ProductPlanDefinitions)
            {
                var productPlanDefinition = new ProProductPlanDefinition(
                    GuidGenerator.Create(),
                    entity.Id, // ProductId
                    planDefinitionDto.PlanCode,
                    planDefinitionDto.PlanName,
                    planDefinitionDto.Status
                );

                await ProductPlanDefinitionRepository.InsertAsync(productPlanDefinition);
            }
        }

        // Create ProProductDistributions if provided
        if (input.ProductDistributions != null && input.ProductDistributions.Any())
        {
            foreach (var distributionDto in input.ProductDistributions)
            {
                // Validate ChannelId if provided
                if (distributionDto.ChannelId.HasValue)
                {
                    var channelRepository = LazyServiceProvider.LazyGetRequiredService<IRepository<iOne.ResChannels.ResChannel, Guid>>();
                    await channelRepository.GetAsync(distributionDto.ChannelId.Value);
                }

                // Validate AppChannelId if provided
                if (distributionDto.AppChannelId.HasValue)
                {
                    var appChannelRepository = LazyServiceProvider.LazyGetRequiredService<IRepository<iOne.ResAppChannels.ResAppChannel, Guid>>();
                    await appChannelRepository.GetAsync(distributionDto.AppChannelId.Value);
                }

                // Validate EmployeeRoleId if provided
                if (distributionDto.EmployeeRoleId.HasValue)
                {
                    var employeeRoleRepository = LazyServiceProvider.LazyGetRequiredService<IRepository<iOne.HrEmployeeRoles.HrEmployeeRole, Guid>>();
                    await employeeRoleRepository.GetAsync(distributionDto.EmployeeRoleId.Value);
                }

                var productDistribution = new ProProductDistribution(
                    GuidGenerator.Create(),
                    entity.Id, // ProductId
                    distributionDto.Status,
                    distributionDto.ChannelId,
                    distributionDto.AppChannelId,
                    distributionDto.EmployeeRoleId
                );

                await ProductDistributionRepository.InsertAsync(productDistribution);
            }
        }

        await CurrentUnitOfWork.SaveChangesAsync();

        return await MapToDtoWithAttributesAsync(entity);
    }

    public override async Task<ProProductDto> UpdateAsync(Guid id, UpdateProProductDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id);

        // ⚠️ QUAN TRỌNG: Chỉ update các fields được phép, không update Code
        // Code is immutable, so no duplicate validation needed
        await Manager.UpdateAsync(
            entity,
            input.ProductTypeId,
            input.PartnerId,
            input.TableRateId,
            input.RootProductId,
            input.IsRootProduct,
            input.LobId,
            input.ProductCategoryId,
            input.CurrencyId,
            input.ShortName,
            input.Name,
            input.Description,
            input.InternalNote,
            input.InsurerProductCode,
            input.RateType,
            input.Status,
            input.SeqNumber,
            input.EffectDate,
            input.ExpireDate,
            input.PlanDefinitionId,
            input.IsPlan,
            input.ImageDocumentId,
            input.CertificateTemplateDocumentId
        );

        // Update ProProductAttributes
        if (input.ProductAttributes != null)
        {
            // Load existing ProductAttributes
            var existingAttributesQuery = await ProductAttributeRepository.GetQueryableAsync();
            var existingAttributes = await existingAttributesQuery
                .Where(x => x.ProductId == entity.Id)
                .ToListAsync();

            // Get IDs from input
            var inputAttributeIds = input.ProductAttributes
                .Where(x => x.Id != Guid.Empty)
                .Select(x => x.Id)
                .ToHashSet();

            // Remove attributes that are not in the input
            var attributesToRemove = existingAttributes
                .Where(x => !inputAttributeIds.Contains(x.Id))
                .ToList();

            foreach (var attributeToRemove in attributesToRemove)
            {
                await ProductAttributeRepository.DeleteAsync(attributeToRemove);
            }

            // Update or create attributes
            foreach (var attributeDto in input.ProductAttributes)
            {
                // Validate AttributeId
                await AttributeRepository.GetAsync(attributeDto.AttributeId);

                if (attributeDto.Id != Guid.Empty)
                {
                    // Update existing
                    var existingAttribute = existingAttributes.FirstOrDefault(x => x.Id == attributeDto.Id);
                    if (existingAttribute != null)
                    {
                        existingAttribute.UpdateIsRequired(!string.IsNullOrWhiteSpace(attributeDto.IsRequired) ? attributeDto.IsRequired : "N");
                        existingAttribute.UpdateStatus(attributeDto.Status);
                        await ProductAttributeRepository.UpdateAsync(existingAttribute);
                    }
                }
                else
                {
                    // Create new
                    var productAttribute = new ProProductAttribute(
                        GuidGenerator.Create(),
                        entity.Id,
                        attributeDto.AttributeId,
                        !string.IsNullOrWhiteSpace(attributeDto.IsRequired) ? attributeDto.IsRequired : "N",
                        attributeDto.Status
                    );

                    await ProductAttributeRepository.InsertAsync(productAttribute);
                }
            }
        }

        // Update ProProductCoverages
        if (input.ProductCoverages != null)
        {
            if (input.ProductCoverages.Any())
            {
                ValidateProductCoveragesNoOverlappingDates(input.ProductCoverages);
            }

            // Load existing ProductCoverages
            var existingCoveragesQuery = await ProductCoverageRepository.GetQueryableAsync();
            var existingCoverages = await existingCoveragesQuery
                .Where(x => x.ProductId == entity.Id)
                .ToListAsync();

            // Get IDs from input
            var inputCoverageIds = input.ProductCoverages
                .Where(x => x.Id != Guid.Empty)
                .Select(x => x.Id)
                .ToHashSet();

            // Remove coverages that are not in the input
            var coveragesToRemove = existingCoverages
                .Where(x => !inputCoverageIds.Contains(x.Id))
                .ToList();

            foreach (var coverageToRemove in coveragesToRemove)
            {
                await ProductCoverageRepository.DeleteAsync(coverageToRemove);
            }

            // Update or create coverages
            foreach (var coverageDto in input.ProductCoverages)
            {
                // Validate CoverageId
                await CoverageRepository.GetAsync(coverageDto.CoverageId);
                
                // Validate TaxId
                await TaxRepository.GetAsync(coverageDto.TaxId);

                Guid coverageId;
                if (coverageDto.Id != Guid.Empty)
                {
                    // Update existing
                    var existingCoverage = existingCoverages.FirstOrDefault(x => x.Id == coverageDto.Id);
                    if (existingCoverage != null)
                    {
                        existingCoverage.UpdateCoverageId(coverageDto.CoverageId);
                        existingCoverage.UpdateParentId(coverageDto.ParentId);
                        existingCoverage.UpdateInsurerCoverageCode(coverageDto.InsurerCoverageCode);
                        existingCoverage.UpdateUomId(coverageDto.UomId);
                        existingCoverage.UpdateAvailabilityType(coverageDto.AvailabilityType);
                        existingCoverage.UpdateEffectDate(coverageDto.EffectDate);
                        existingCoverage.UpdateExpireDate(coverageDto.ExpireDate);
                        existingCoverage.UpdateSeqNumber(coverageDto.SeqNumber);
                        existingCoverage.UpdateTaxId(coverageDto.TaxId);
                        existingCoverage.UpdateEnableQuantity(coverageDto.EnableQuantity);
                        await ProductCoverageRepository.UpdateAsync(existingCoverage);
                        coverageId = existingCoverage.Id;
                    }
                    else
                    {
                        // Create new coverage with client-provided ID
                        var productCoverage = new ProProductCoverage(
                            coverageDto.Id,
                            entity.Id,
                            coverageDto.CoverageId,
                            coverageDto.AvailabilityType,
                            coverageDto.EffectDate,
                            coverageDto.TaxId,
                            coverageDto.SeqNumber,
                            coverageDto.ParentId,
                            coverageDto.InsurerCoverageCode,
                            coverageDto.UomId,
                            coverageDto.ExpireDate,
                            coverageDto.EnableQuantity
                        );

                        await ProductCoverageRepository.InsertAsync(productCoverage);
                        coverageId = productCoverage.Id;
                    }
                }
                else
                {
                    // Create new
                    var productCoverage = new ProProductCoverage(
                        GuidGenerator.Create(),
                        entity.Id,
                        coverageDto.CoverageId,
                        coverageDto.AvailabilityType,
                        coverageDto.EffectDate,
                        coverageDto.TaxId,
                        coverageDto.SeqNumber,
                        coverageDto.ParentId,
                        coverageDto.InsurerCoverageCode,
                        coverageDto.UomId,
                        coverageDto.ExpireDate,
                        coverageDto.EnableQuantity
                    );

                    await ProductCoverageRepository.InsertAsync(productCoverage);
                    coverageId = productCoverage.Id;
                }

                // Update ProProductCoverageInteractions for this coverage
                if (coverageDto.ProductCoverageInteractions != null)
                {
                    // Load existing interactions for this coverage
                    var existingInteractionsQuery = await ProductCoverageInteractionRepository.GetQueryableAsync();
                    var existingInteractions = await existingInteractionsQuery
                        .Where(x => x.ProductCoverageId == coverageId)
                        .ToListAsync();

                    // Get IDs from input
                    var inputInteractionIds = coverageDto.ProductCoverageInteractions
                        .Where(x => x.Id != Guid.Empty)
                        .Select(x => x.Id)
                        .ToHashSet();

                    // Remove interactions that are not in the input
                    var interactionsToRemove = existingInteractions
                        .Where(x => !inputInteractionIds.Contains(x.Id))
                        .ToList();

                    foreach (var interactionToRemove in interactionsToRemove)
                    {
                        await ProductCoverageInteractionRepository.DeleteAsync(interactionToRemove);
                    }

                    // Update or create interactions
                    foreach (var interactionDto in coverageDto.ProductCoverageInteractions)
                    {
                        // Skip interactions with invalid InteractionCoverageId (empty, null, or Guid.Empty)
                        if (interactionDto.InteractionCoverageId == Guid.Empty)
                        {
                            continue;
                        }

                        // Validate InteractionCoverageId (must be a valid ProProductCoverage for this product)
                        var interactionCoverageQuery = await ProductCoverageRepository.GetQueryableAsync();
                        var interactionCoverage = await interactionCoverageQuery
                            .FirstOrDefaultAsync(x => x.Id == interactionDto.InteractionCoverageId && x.ProductId == entity.Id);
                        
                        if (interactionCoverage == null)
                        {
                            throw new UserFriendlyException(
                                L["Product:ProProductCoverageInteraction:InvalidInteractionCoverageId", interactionDto.InteractionCoverageId]);
                        }

                        if (interactionDto.Id != Guid.Empty)
                        {
                            // Update existing
                            var existingInteraction = existingInteractions.FirstOrDefault(x => x.Id == interactionDto.Id);
                            if (existingInteraction != null)
                            {
                                existingInteraction.UpdateInteractionType(interactionDto.InteractionType);
                                existingInteraction.UpdateInteractionCoverageId(interactionDto.InteractionCoverageId);
                                existingInteraction.UpdateEffectDate(interactionDto.EffectDate);
                                existingInteraction.UpdateExpireDate(interactionDto.ExpireDate);
                                await ProductCoverageInteractionRepository.UpdateAsync(existingInteraction);
                            }
                        }
                        else
                        {
                            // Create new
                            var productCoverageInteraction = new ProProductCoverageInteraction(
                                GuidGenerator.Create(),
                                coverageId,
                                interactionDto.InteractionType,
                                interactionDto.InteractionCoverageId,
                                interactionDto.EffectDate,
                                interactionDto.ExpireDate
                            );

                            await ProductCoverageInteractionRepository.InsertAsync(productCoverageInteraction);
                        }
                    }
                }

                // Update ProProductCoverageLevels for this coverage
                if (coverageDto.ProductCoverageLevels != null)
                {
                    // Load existing levels for this coverage
                    var existingLevelsQuery = await ProductCoverageLevelRepository.GetQueryableAsync();
                    var existingLevels = await existingLevelsQuery
                        .Where(x => x.ProductCoverageId == coverageId)
                        .ToListAsync();

                    // Get IDs from input
                    var inputLevelIds = coverageDto.ProductCoverageLevels
                        .Where(x => x.Id != Guid.Empty)
                        .Select(x => x.Id)
                        .ToHashSet();

                    // Remove levels that are not in the input
                    var levelsToRemove = existingLevels
                        .Where(x => !inputLevelIds.Contains(x.Id))
                        .ToList();

                    foreach (var levelToRemove in levelsToRemove)
                    {
                        await ProductCoverageLevelRepository.DeleteAsync(levelToRemove);
                    }

                    // Update or create levels
                    foreach (var levelDto in coverageDto.ProductCoverageLevels)
                    {
                        Guid levelId;
                        if (levelDto.Id != Guid.Empty)
                        {
                            // Update existing
                            var existingLevel = existingLevels.FirstOrDefault(x => x.Id == levelDto.Id);
                            if (existingLevel != null)
                            {
                                existingLevel.UpdateProductCoverageId(coverageId);
                                existingLevel.UpdateCode(levelDto.Code);
                                existingLevel.UpdateName(levelDto.Name);
                                existingLevel.UpdateEffectDate(levelDto.EffectDate);
                                existingLevel.UpdateExpireDate(levelDto.ExpireDate);
                                existingLevel.UpdateConditionalScript(levelDto.ConditionalScript);
                                await ProductCoverageLevelRepository.UpdateAsync(existingLevel);
                                levelId = existingLevel.Id;
                            }
                            else
                            {
                                // Create new level with client-provided ID
                                var productCoverageLevel = new ProProductCoverageLevel(
                                    levelDto.Id,
                                    levelDto.Code,
                                    levelDto.Name,
                                    levelDto.EffectDate,
                                    coverageId,
                                    levelDto.ExpireDate,
                                    levelDto.ConditionalScript
                                );

                                await ProductCoverageLevelRepository.InsertAsync(productCoverageLevel);
                                levelId = productCoverageLevel.Id;
                            }
                        }
                        else
                        {
                            // Create new
                            var productCoverageLevel = new ProProductCoverageLevel(
                                GuidGenerator.Create(),
                                levelDto.Code,
                                levelDto.Name,
                                levelDto.EffectDate,
                                coverageId,
                                levelDto.ExpireDate,
                                levelDto.ConditionalScript
                            );

                            await ProductCoverageLevelRepository.InsertAsync(productCoverageLevel);
                            levelId = productCoverageLevel.Id;
                        }

                        // Update ProProductCoverageLevelTerms for this level
                        if (levelDto.Terms != null)
                        {
                            // Load existing terms for this level
                            var existingTermsQuery = await ProductCoverageLevelTermRepository.GetQueryableAsync();
                            var existingTerms = await existingTermsQuery
                                .Where(x => x.ProductCoverageLevelId == levelId)
                                .ToListAsync();

                            // Get IDs from input
                            var inputTermIds = levelDto.Terms
                                .Where(x => x.Id != Guid.Empty)
                                .Select(x => x.Id)
                                .ToHashSet();

                            // Remove terms that are not in the input
                            var termsToRemove = existingTerms
                                .Where(x => !inputTermIds.Contains(x.Id))
                                .ToList();

                            foreach (var termToRemove in termsToRemove)
                            {
                                await ProductCoverageLevelTermRepository.DeleteAsync(termToRemove);
                            }

                            // Update or create terms
                            foreach (var termDto in levelDto.Terms)
                            {
                                if (termDto.Id != Guid.Empty)
                                {
                                    // Update existing
                                    var existingTerm = existingTerms.FirstOrDefault(x => x.Id == termDto.Id);
                                    if (existingTerm != null)
                                    {
                                        existingTerm.UpdateProductCoverageLevelId(levelId);
                                        existingTerm.UpdateCoverageLevelTypeId(termDto.CoverageLevelTypeId);
                                        existingTerm.UpdateCoverageLevelBasisId(termDto.CoverageLevelBasisId);
                                        existingTerm.UpdateAmountType(termDto.AmountType);
                                        existingTerm.UpdateFromAmount(termDto.FromAmount);
                                        existingTerm.UpdateToAmount(termDto.ToAmount);
                                        existingTerm.UpdateConditionScript(termDto.ConditionScript);
                                        existingTerm.UpdateComputeScript(termDto.ComputeScript);
                                        existingTerm.UpdateIsDefault(termDto.IsDefault);
                                        await ProductCoverageLevelTermRepository.UpdateAsync(existingTerm);
                                    }
                                }
                                else
                                {
                                    // Create new
                                    var productCoverageLevelTerm = new ProProductCoverageLevelTerm(
                                        GuidGenerator.Create(),
                                        termDto.CoverageLevelTypeId,
                                        termDto.AmountType,
                                        termDto.FromAmount,
                                        termDto.ToAmount,
                                        levelId,
                                        termDto.CoverageLevelBasisId,
                                        termDto.ConditionScript,
                                        termDto.ComputeScript,
                                        termDto.IsDefault
                                    );

                                    await ProductCoverageLevelTermRepository.InsertAsync(productCoverageLevelTerm);
                                }
                            }
                        }
                    }
                }
            }
        }

        // Update ProProductTableRates
        if (input.ProductTableRates != null)
        {
            // Load existing ProductTableRates
            var existingTableRatesQuery = await ProductTableRateRepository.GetQueryableAsync();
            var existingTableRates = await existingTableRatesQuery
                .Where(x => x.ProductId == entity.Id)
                .ToListAsync();

            // Get IDs from input
            var inputTableRateIds = input.ProductTableRates
                .Where(x => x.Id != Guid.Empty)
                .Select(x => x.Id)
                .ToHashSet();

            // Remove table rates that are not in the input
            var tableRatesToRemove = existingTableRates
                .Where(x => !inputTableRateIds.Contains(x.Id))
                .ToList();

            foreach (var tableRateToRemove in tableRatesToRemove)
            {
                await ProductTableRateRepository.DeleteAsync(tableRateToRemove);
            }

            // Update or create table rates
            foreach (var tableRateDto in input.ProductTableRates)
            {
                // Validate TableRateId
                await TableRateRepository.GetAsync(tableRateDto.TableRateId);

                if (tableRateDto.Id != Guid.Empty)
                {
                    // Update existing
                    var existingTableRate = existingTableRates.FirstOrDefault(x => x.Id == tableRateDto.Id);
                    if (existingTableRate != null)
                    {
                        // Validate that TableRateId hasn't changed (it's immutable)
                        if (existingTableRate.TableRateId != tableRateDto.TableRateId)
                        {
                            throw new UserFriendlyException(
                                L["Product:ProProductTableRate:TableRateIdCannotBeChanged", tableRateDto.Id, tableRateDto.TableRateId]);
                        }

                        existingTableRate.UpdateEffectDate(tableRateDto.EffectDate);
                        existingTableRate.UpdateExpireDate(tableRateDto.ExpireDate);
                        await ProductTableRateRepository.UpdateAsync(existingTableRate);
                    }
                }
                else
                {
                    // Create new
                    var productTableRate = new ProProductTableRate(
                        GuidGenerator.Create(),
                        entity.Id,
                        tableRateDto.TableRateId,
                        tableRateDto.EffectDate,
                        tableRateDto.ExpireDate
                    );

                    await ProductTableRateRepository.InsertAsync(productTableRate);
                }
            }
        }

        // Update ProRules
        if (input.ProRules != null)
        {
            // Load existing ProRules for this product
            var existingRulesQuery = await ProRuleRepository.GetQueryableAsync();
            var existingRules = await existingRulesQuery
                .Where(x => x.ApplyTo == "product" && x.ApplyToId == entity.Id)
                .ToListAsync();

            // Get IDs from input
            var inputRuleIds = input.ProRules
                .Where(x => x.Id != Guid.Empty)
                .Select(x => x.Id)
                .ToHashSet();

            // Remove rules that are not in the input
            var rulesToRemove = existingRules
                .Where(x => !inputRuleIds.Contains(x.Id))
                .ToList();

            foreach (var ruleToRemove in rulesToRemove)
            {
                await ProRuleRepository.DeleteAsync(ruleToRemove);
            }

            // Update or create rules
            foreach (var ruleDto in input.ProRules)
            {
                // Validate RuleTypeId
                var ruleTypeRepository = LazyServiceProvider.LazyGetRequiredService<IRepository<iOne.ProRuleTypes.ProRuleType, Guid>>();
                await ruleTypeRepository.GetAsync(ruleDto.RuleTypeId);

                if (ruleDto.Id != Guid.Empty)
                {
                    // Update existing
                    var existingRule = existingRules.FirstOrDefault(x => x.Id == ruleDto.Id);
                    if (existingRule != null)
                    {
                        existingRule.UpdateApplyTo("product");
                        existingRule.UpdateApplyToId(entity.Id);
                        existingRule.UpdateRuleTypeId(ruleDto.RuleTypeId);
                        existingRule.UpdateName(ruleDto.Name);
                        existingRule.UpdateDescription(ruleDto.Description);
                        existingRule.UpdateRuleScript(ruleDto.RuleScript);
                        existingRule.UpdatePriority(ruleDto.Priority);
                        existingRule.UpdateStatus(ruleDto.Status);
                        existingRule.UpdateEffectDate(ruleDto.EffectDate);
                        existingRule.UpdateExpireDate(ruleDto.ExpireDate);
                        await ProRuleRepository.UpdateAsync(existingRule);
                    }
                }
                else
                {
                    // Create new - validate code uniqueness
                    var normalizedCode = ruleDto.Code?.Trim().ToUpperInvariant();
                    if (!string.IsNullOrWhiteSpace(normalizedCode))
                    {
                        var ruleRepository = LazyServiceProvider.LazyGetRequiredService<IProRuleRepository>();
                        if (await ruleRepository.IsCodeExistsAsync(normalizedCode))
                        {
                            throw new UserFriendlyException(
                                L["ProRule:CodeExists"].Value.Replace("{Code}", normalizedCode)
                            );
                        }
                    }

                    var proRule = new ProRule(
                        GuidGenerator.Create(),
                        "product", // ApplyTo = "product" for ProProduct
                        entity.Id, // ApplyToId = ProProduct.id
                        ruleDto.RuleTypeId,
                        ruleDto.Code,
                        ruleDto.Name,
                        ruleDto.RuleScript,
                        ruleDto.Status,
                        ruleDto.EffectDate,
                        ruleDto.Description,
                        ruleDto.Priority,
                        ruleDto.ExpireDate
                    );

                    await ProRuleRepository.InsertAsync(proRule);
                }
            }
        }

        // Update ProProductPlanDefinitions
        // Only able to create/update ProProductPlanDefinition if ProProduct.PartnerId = null
        if (input.ProductPlanDefinitions != null)
        {
            if (entity.PartnerId != null && !input.ProductPlanDefinitions.IsNullOrEmpty())
            {
                throw new UserFriendlyException(
                    L["Product:ProProductPlanDefinition:ProductHasPartner", entity.Id]);
            }

            // Load existing ProProductPlanDefinitions for this product
            var existingPlanDefinitionsQuery = await ProductPlanDefinitionRepository.GetQueryableAsync();
            var existingPlanDefinitions = await existingPlanDefinitionsQuery
                .Where(x => x.ProductId == entity.Id)
                .ToListAsync();

            // Get IDs from input
            var inputPlanDefinitionIds = input.ProductPlanDefinitions
                .Where(x => x.Id != Guid.Empty)
                .Select(x => x.Id)
                .ToHashSet();

            // Remove plan definitions that are not in the input
            var planDefinitionsToRemove = existingPlanDefinitions
                .Where(x => !inputPlanDefinitionIds.Contains(x.Id))
                .ToList();

            foreach (var planDefinitionToRemove in planDefinitionsToRemove)
            {
                if (await ProductRepository.AnyProductReferencesPlanDefinitionAsync(planDefinitionToRemove.Id))
                {
                    var displayName = !string.IsNullOrWhiteSpace(planDefinitionToRemove.PlanName)
                        ? planDefinitionToRemove.PlanName
                        : planDefinitionToRemove.PlanCode;
                    throw new UserFriendlyException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            L["ProProductPlanDefinition:CannotDeleteInUse"].Value,
                            displayName));
                }

                await ProductPlanDefinitionRepository.DeleteAsync(planDefinitionToRemove);
            }

            // Update or create plan definitions
            foreach (var planDefinitionDto in input.ProductPlanDefinitions)
            {
                if (planDefinitionDto.Id != Guid.Empty)
                {
                    // Update existing
                    var existingPlanDefinition = existingPlanDefinitions.FirstOrDefault(x => x.Id == planDefinitionDto.Id);
                    if (existingPlanDefinition != null)
                    {
                        existingPlanDefinition.UpdateProductId(entity.Id);
                        existingPlanDefinition.UpdatePlanName(planDefinitionDto.PlanName);
                        existingPlanDefinition.UpdateStatus(planDefinitionDto.Status);
                        await ProductPlanDefinitionRepository.UpdateAsync(existingPlanDefinition);
                    }
                }
                else
                {
                    // Create new - validate code uniqueness
                    var normalizedCode = planDefinitionDto.PlanCode?.Trim().ToUpperInvariant();
                    if (!string.IsNullOrWhiteSpace(normalizedCode))
                    {
                        var planDefinitionRepository = LazyServiceProvider.LazyGetRequiredService<IProProductPlanDefinitionRepository>();
                        if (await planDefinitionRepository.IsCodeExistsAsync(normalizedCode))
                        {
                            throw new UserFriendlyException(
                                L["ProProductPlanDefinition:CodeExists"].Value.Replace("{Code}", normalizedCode));
                        }
                    }

                    var productPlanDefinition = new ProProductPlanDefinition(
                        GuidGenerator.Create(),
                        entity.Id, // ProductId
                        planDefinitionDto.PlanCode,
                        planDefinitionDto.PlanName,
                        planDefinitionDto.Status
                    );

                    await ProductPlanDefinitionRepository.InsertAsync(productPlanDefinition);
                }
            }
        }

        // Update ProProductDistributions
        if (input.ProductDistributions != null)
        {
            Logger.LogDebug("Product update: ProductDistributions received, count = {Count}", input.ProductDistributions.Count);
            // Load existing ProProductDistributions for this product
            var existingDistributionsQuery = await ProductDistributionRepository.GetQueryableAsync();
            var existingDistributions = await existingDistributionsQuery
                .Where(x => x.ProductId == entity.Id)
                .ToListAsync();

            // Get IDs from input
            var inputDistributionIds = input.ProductDistributions
                .Where(x => x.Id != Guid.Empty)
                .Select(x => x.Id)
                .ToHashSet();

            // Remove distributions that are not in the input
            var distributionsToRemove = existingDistributions
                .Where(x => !inputDistributionIds.Contains(x.Id))
                .ToList();

            foreach (var distributionToRemove in distributionsToRemove)
            {
                await ProductDistributionRepository.DeleteAsync(distributionToRemove);
            }

            // Update or create distributions
            foreach (var distributionDto in input.ProductDistributions)
            {
                // Validate ChannelId if provided
                if (distributionDto.ChannelId.HasValue)
                {
                    var channelRepository = LazyServiceProvider.LazyGetRequiredService<IRepository<iOne.ResChannels.ResChannel, Guid>>();
                    await channelRepository.GetAsync(distributionDto.ChannelId.Value);
                }

                // Validate AppChannelId if provided
                if (distributionDto.AppChannelId.HasValue)
                {
                    var appChannelRepository = LazyServiceProvider.LazyGetRequiredService<IRepository<iOne.ResAppChannels.ResAppChannel, Guid>>();
                    await appChannelRepository.GetAsync(distributionDto.AppChannelId.Value);
                }

                // Validate EmployeeRoleId if provided
                if (distributionDto.EmployeeRoleId.HasValue)
                {
                    var employeeRoleRepository = LazyServiceProvider.LazyGetRequiredService<IRepository<iOne.HrEmployeeRoles.HrEmployeeRole, Guid>>();
                    await employeeRoleRepository.GetAsync(distributionDto.EmployeeRoleId.Value);
                }

                if (distributionDto.Id != Guid.Empty)
                {
                    // Update existing
                    var existingDistribution = existingDistributions.FirstOrDefault(x => x.Id == distributionDto.Id);
                    if (existingDistribution != null)
                    {
                        existingDistribution.UpdateProductId(entity.Id);
                        existingDistribution.UpdateChannelId(distributionDto.ChannelId);
                        existingDistribution.UpdateAppChannelId(distributionDto.AppChannelId);
                        existingDistribution.UpdateEmployeeRoleId(distributionDto.EmployeeRoleId);
                        existingDistribution.UpdateStatus(distributionDto.Status);
                        await ProductDistributionRepository.UpdateAsync(existingDistribution);
                    }
                }
                else
                {
                    // Create new
                    var productDistribution = new ProProductDistribution(
                        GuidGenerator.Create(),
                        entity.Id, // ProductId
                        distributionDto.Status,
                        distributionDto.ChannelId,
                        distributionDto.AppChannelId,
                        distributionDto.EmployeeRoleId
                    );

                    await ProductDistributionRepository.InsertAsync(productDistribution);
                }
            }
        }
        else
        {
            Logger.LogDebug("Product update: ProductDistributions is null, skipping distribution sync");
        }

        // Ensure changes are saved (Repository.UpdateAsync should handle this, but explicitly save to ensure audit logging)
        await CurrentUnitOfWork.SaveChangesAsync();

        return await MapToDtoWithAttributesAsync(entity);
    }

    public override async Task DeleteAsync(Guid id)
    {
        // Load entity with ProductAttributes, ProductCoverages, and ProductTableRates
        var query = await Repository.GetQueryableAsync();
        var entity = await query
            .Include(x => x.ProductAttributes)
            .Include(x => x.ProductCoverages)
            .Include(x => x.ProductTableRates)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(ProProduct), id);
        }

        // Delete all related ProProductAttributes
        if (entity.ProductAttributes != null && entity.ProductAttributes.Any())
        {
            foreach (var productAttribute in entity.ProductAttributes.ToList())
            {
                await ProductAttributeRepository.DeleteAsync(productAttribute);
            }
        }

        // Delete all related ProProductCoverages (this will cascade delete ProProductCoverageInteractions and ProProductCoverageLevels)
        if (entity.ProductCoverages != null && entity.ProductCoverages.Any())
        {
            foreach (var productCoverage in entity.ProductCoverages.ToList())
            {
                // Delete all interactions for this coverage first
                var interactionsQuery = await ProductCoverageInteractionRepository.GetQueryableAsync();
                var interactions = await interactionsQuery
                    .Where(x => x.ProductCoverageId == productCoverage.Id)
                    .ToListAsync();

                foreach (var interaction in interactions)
                {
                    await ProductCoverageInteractionRepository.DeleteAsync(interaction);
                }

                // Delete all levels for this coverage
                var levelsQuery = await ProductCoverageLevelRepository.GetQueryableAsync();
                var levels = await levelsQuery
                    .Where(x => x.ProductCoverageId == productCoverage.Id)
                    .ToListAsync();

                foreach (var level in levels)
                {
                    // Delete all terms for this level first
                    var termsQuery = await ProductCoverageLevelTermRepository.GetQueryableAsync();
                    var terms = await termsQuery
                        .Where(x => x.ProductCoverageLevelId == level.Id)
                        .ToListAsync();

                    foreach (var term in terms)
                    {
                        await ProductCoverageLevelTermRepository.DeleteAsync(term);
                    }

                    await ProductCoverageLevelRepository.DeleteAsync(level);
                }

                await ProductCoverageRepository.DeleteAsync(productCoverage);
            }
        }

        // Delete all related ProProductTableRates
        if (entity.ProductTableRates != null && entity.ProductTableRates.Any())
        {
            foreach (var productTableRate in entity.ProductTableRates.ToList())
            {
                await ProductTableRateRepository.DeleteAsync(productTableRate);
            }
        }

        // Delete all related ProRules
        var rulesQuery = await ProRuleRepository.GetQueryableAsync();
        var rules = await rulesQuery
            .Where(x => x.ApplyTo == "product" && x.ApplyToId == entity.Id)
            .ToListAsync();

        foreach (var rule in rules)
        {
            await ProRuleRepository.DeleteAsync(rule);
        }

        // Delete all related ProProductPlanDefinitions
        var planDefinitionsQuery = await ProductPlanDefinitionRepository.GetQueryableAsync();
        var planDefinitions = await planDefinitionsQuery
            .Where(x => x.ProductId == entity.Id)
            .ToListAsync();

        foreach (var planDefinition in planDefinitions)
        {
            await ProductPlanDefinitionRepository.DeleteAsync(planDefinition);
        }

        // Delete all related ProProductDistributions
        var distributionsQuery = await ProductDistributionRepository.GetQueryableAsync();
        var distributions = await distributionsQuery
            .Where(x => x.ProductId == entity.Id)
            .ToListAsync();

        foreach (var distribution in distributions)
        {
            await ProductDistributionRepository.DeleteAsync(distribution);
        }

        // Delete to trigger audit logging with ChangeType = Deleted
        // FullAuditedAggregateRoot uses soft delete, so entity is not actually removed from database
        await Repository.DeleteAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
        
        // Update status to Deactive (entity still exists in database due to soft delete)
        entity!.UpdateStatus(ProProductStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<ProProduct>> CreateFilteredQueryAsync(GetProProductsInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();

        // Filter by ProductTypeId
        if (input.ProductTypeId.HasValue)
        {
            query = query.Where(x => x.ProductTypeId == input.ProductTypeId.Value);
        }

        // Filter by PartnerId
        if (input.PartnerId.HasValue)
        {
            query = query.Where(x => x.PartnerId == input.PartnerId.Value);
        }

        // Filter by LobId
        if (input.LobId.HasValue)
        {
            query = query.Where(x => x.LobId == input.LobId.Value);
        }

        // Filter by ProductCategoryId
        if (input.ProductCategoryId.HasValue)
        {
            query = query.Where(x => x.ProductCategoryId == input.ProductCategoryId.Value);
        }

        // Filter by CurrencyId
        if (input.CurrencyId.HasValue)
        {
            query = query.Where(x => x.CurrencyId == input.CurrencyId.Value);
        }

        // Filter by Code (case-insensitive, contains)
        if (!string.IsNullOrWhiteSpace(input.Code))
        {
            query = query.Where(x => EF.Functions.ILike(x.Code, $"%{input.Code}%"));
        }

        // Filter by Name (case-insensitive, contains)
        if (!string.IsNullOrWhiteSpace(input.Name))
        {
            query = query.Where(x => EF.Functions.ILike(x.Name, $"%{input.Name}%"));
        }

        // Filter by ShortName (case-insensitive, contains)
        if (!string.IsNullOrWhiteSpace(input.ShortName))
        {
            query = query.Where(x => EF.Functions.ILike(x.ShortName, $"%{input.ShortName}%"));
        }

        // Filter by Status
        if (input.Status.HasValue)
        {
            query = query.Where(x => x.Status == input.Status.Value);
        }

        return query;
    }

    public virtual async Task<PagedResultDto<ProProductListDto>> GetTreeListAsync(GetProProductsInput input)
    {
        var query = await CreateFilteredQueryAsync(input);
        var totalCount = await AsyncExecuter.CountAsync(query);
        query = ApplySorting(query, input);
        var entities = await AsyncExecuter.ToListAsync(ApplyPaging(query, input));
        var dtos = ObjectMapper.Map<List<ProProduct>, List<ProProductListDto>>(entities);
        return new PagedResultDto<ProProductListDto>(totalCount, dtos);
    }

    protected override async Task<ProProductDto> MapToGetOutputDtoAsync(ProProduct entity)
    {
        return await MapToDtoWithAttributesAsync(entity);
    }

    protected override async Task<ProProductDto> MapToGetListOutputDtoAsync(ProProduct entity)
    {
        return await MapToDtoWithAttributesAsync(entity);
    }

    private async Task<ProProductDto> MapToDtoWithAttributesAsync(ProProduct entity)
    {
        var dto = ObjectMapper.Map<ProProduct, ProProductDto>(entity);

        // Load ProductAttributes
        var attributesQuery = await ProductAttributeRepository.GetQueryableAsync();
        var productAttributes = await attributesQuery
            .Where(x => x.ProductId == entity.Id)
            .ToListAsync();

        dto.ProductAttributes = ObjectMapper.Map<List<ProProductAttribute>, List<ProProductAttributeDto>>(productAttributes);

        // Load ProductCoverages
        var coveragesQuery = await ProductCoverageRepository.GetQueryableAsync();
        var productCoverages = await coveragesQuery
            .Where(x => x.ProductId == entity.Id)
            .ToListAsync();

        dto.ProductCoverages = ObjectMapper.Map<List<ProProductCoverage>, List<ProProductCoverageDto>>(productCoverages);

        // Load all Coverages for ProductCoverages in a single query
        var coverageQuery = await CoverageRepository.GetQueryableAsync();
        var coverageIds = dto.ProductCoverages.Select(x => x.CoverageId).Distinct().ToList();
        var coverages = await coverageQuery
            .Where(x => coverageIds.Contains(x.Id))
            .ToListAsync();
        var coverageDict = coverages.ToDictionary(x => x.Id);

        // Load ProductCoverageInteractions for each ProductCoverage
        var interactionsQuery = await ProductCoverageInteractionRepository.GetQueryableAsync();
        var levelsQuery = await ProductCoverageLevelRepository.GetQueryableAsync();
        var termsQuery = await ProductCoverageLevelTermRepository.GetQueryableAsync();
        foreach (var coverageDto in dto.ProductCoverages)
        {
            var interactions = await interactionsQuery
                .Where(x => x.ProductCoverageId == coverageDto.Id)
                .ToListAsync();

            coverageDto.ProductCoverageInteractions = ObjectMapper.Map<List<ProProductCoverageInteraction>, List<ProProductCoverageInteractionDto>>(interactions);
            foreach (var interaction in coverageDto.ProductCoverageInteractions)
            {
                interaction.InteractionTypeCode = interaction.InteractionType.ToString().ToLowerInvariant();
            }

            var levels = await levelsQuery
                .Where(x => x.ProductCoverageId == coverageDto.Id)
                .ToListAsync();

            coverageDto.ProductCoverageLevels = ObjectMapper.Map<List<ProProductCoverageLevel>, List<ProProductCoverageLevelDto>>(levels);

            // Load Terms for each ProductCoverageLevel
            foreach (var levelDto in coverageDto.ProductCoverageLevels)
            {
                var terms = await termsQuery
                    .Where(x => x.ProductCoverageLevelId == levelDto.Id)
                    .ToListAsync();

                levelDto.Terms = ObjectMapper.Map<List<ProProductCoverageLevelTerm>, List<ProProductCoverageLevelTermDto>>(terms);
            }

            // Map Coverage for each ProductCoverage
            if (coverageDict.TryGetValue(coverageDto.CoverageId, out var coverage))
            {
                coverageDto.Coverage = ObjectMapper.Map<ProCoverage, ProCoverageDto>(coverage);
            }
        }

        // Load ProductTableRates
        var tableRatesQuery = await ProductTableRateRepository.GetQueryableAsync();
        var productTableRates = await tableRatesQuery
            .Where(x => x.ProductId == entity.Id)
            .ToListAsync();

        dto.ProductTableRates = ObjectMapper.Map<List<ProProductTableRate>, List<ProProductTableRateDto>>(productTableRates);

        // Load ProRules for this product
        var rulesQuery = await ProRuleRepository.GetQueryableAsync();
        var proRules = await rulesQuery
            .Where(x => x.ApplyTo == "product" && x.ApplyToId == entity.Id)
            .ToListAsync();

        dto.ProRules = ObjectMapper.Map<List<ProRule>, List<ProRuleDto>>(proRules);

        // Load ProProductPlanDefinitions for this product
        var planDefinitionsQuery = await ProductPlanDefinitionRepository.GetQueryableAsync();
        var productPlanDefinitions = await planDefinitionsQuery
            .Where(x => x.ProductId == entity.Id)
            .ToListAsync();

        dto.ProductPlanDefinitions = ObjectMapper.Map<List<ProProductPlanDefinition>, List<ProProductPlanDefinitionDto>>(productPlanDefinitions);

        // Load ProProductDistributions for this product
        var distributionsQuery = await ProductDistributionRepository.GetQueryableAsync();
        var productDistributions = await distributionsQuery
            .Where(x => x.ProductId == entity.Id)
            .ToListAsync();

        dto.ProductDistributions = ObjectMapper.Map<List<ProProductDistribution>, List<ProProductDistributionDto>>(productDistributions);

        // Collect all unique creator IDs from nested DTOs
        var creatorIds = new HashSet<Guid?>();

        // From ProductTableRates
        foreach (var tableRate in dto.ProductTableRates)
        {
            if (tableRate.CreatorId.HasValue)
                creatorIds.Add(tableRate.CreatorId.Value);
        }

        // From ProRules
        foreach (var rule in dto.ProRules)
        {
            if (rule.CreatorId.HasValue)
                creatorIds.Add(rule.CreatorId.Value);
        }

        // From ProductPlanDefinitions
        foreach (var plan in dto.ProductPlanDefinitions)
        {
            if (plan.CreatorId.HasValue)
                creatorIds.Add(plan.CreatorId.Value);
        }

        // From ProductCoverages -> Interactions
        foreach (var coverage in dto.ProductCoverages)
        {
            foreach (var interaction in coverage.ProductCoverageInteractions)
            {
                if (interaction.CreatorId.HasValue)
                    creatorIds.Add(interaction.CreatorId.Value);
            }

            // From ProductCoverages -> Levels
            foreach (var level in coverage.ProductCoverageLevels)
            {
                if (level.CreatorId.HasValue)
                    creatorIds.Add(level.CreatorId.Value);

                // From Levels -> Terms
                if (level.Terms != null)
                {
                    foreach (var term in level.Terms)
                    {
                        if (term.CreatorId.HasValue)
                            creatorIds.Add(term.CreatorId.Value);
                    }
                }
            }
        }

        // Batch load all users in a single query
        var userIds = creatorIds.Where(id => id.HasValue).Select(id => id.Value).ToList();
        var userDict = new Dictionary<Guid, IdentityUser>();
        if (userIds.Any())
        {
            var usersQuery = await UserRepository.GetQueryableAsync();
            var users = await usersQuery
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();
            userDict = users.ToDictionary(u => u.Id, u => u);
        }

        // Populate CreatorName for ProductTableRates
        foreach (var tableRate in dto.ProductTableRates)
        {
            if (tableRate.CreatorId.HasValue &&
                userDict.TryGetValue(tableRate.CreatorId.Value, out var user))
            {
                tableRate.CreatorName = user.UserName;
            }
        }

        // Populate CreatorName for ProRules
        foreach (var rule in dto.ProRules)
        {
            if (rule.CreatorId.HasValue &&
                userDict.TryGetValue(rule.CreatorId.Value, out var user))
            {
                rule.CreatorName = user.UserName;
            }
        }

        // Populate CreatorName for ProductPlanDefinitions
        foreach (var plan in dto.ProductPlanDefinitions)
        {
            if (plan.CreatorId.HasValue &&
                userDict.TryGetValue(plan.CreatorId.Value, out var user))
            {
                plan.CreatorName = user.UserName;
            }
        }

        // Populate CreatorName for ProductCoverageInteractions
        foreach (var coverage in dto.ProductCoverages)
        {
            foreach (var interaction in coverage.ProductCoverageInteractions)
            {
                if (interaction.CreatorId.HasValue &&
                    userDict.TryGetValue(interaction.CreatorId.Value, out var user))
                {
                    interaction.CreatorName = user.UserName;
                }
            }

            // Populate CreatorName for ProductCoverageLevels
            foreach (var level in coverage.ProductCoverageLevels)
            {
                if (level.CreatorId.HasValue &&
                    userDict.TryGetValue(level.CreatorId.Value, out var user))
                {
                    level.CreatorName = user.UserName;
                }

                // Populate CreatorName for ProductCoverageLevelTerms
                if (level.Terms != null)
                {
                    foreach (var term in level.Terms)
                    {
                        if (term.CreatorId.HasValue &&
                            userDict.TryGetValue(term.CreatorId.Value, out var userTerm))
                        {
                            term.CreatorName = userTerm.UserName;
                        }
                    }
                }
            }
        }

        return dto;
    }

    /// <summary>
    /// Ensures no two rows for the same master coverage (<see cref="ProProductCoverageDto.CoverageId"/>)
    /// have overlapping [EffectDate, ExpireDate] ranges. Null expire date means open-ended.
    /// </summary>
    private void ValidateProductCoveragesNoOverlappingDates(IReadOnlyList<ProProductCoverageDto> coverages)
    {
        if (coverages == null || coverages.Count < 2)
        {
            return;
        }

        for (var i = 0; i < coverages.Count; i++)
        {
            var a = coverages[i];
            for (var j = i + 1; j < coverages.Count; j++)
            {
                var b = coverages[j];
                if (a.CoverageId != b.CoverageId)
                {
                    continue;
                }

                if (a.Id == b.Id)
                {
                    continue;
                }

                if (ProductCoverageDateRangesOverlap(a.EffectDate, a.ExpireDate, b.EffectDate, b.ExpireDate))
                {
                    throw new UserFriendlyException(L["Product:ProProductCoverage:OverlappingDateRange"].Value);
                }
            }
        }
    }

    private static bool ProductCoverageDateRangesOverlap(
        DateTime effectA,
        DateTime? expireA,
        DateTime effectB,
        DateTime? expireB)
    {
        var startA = DateOnly.FromDateTime(effectA.Date);
        var endA = expireA.HasValue ? DateOnly.FromDateTime(expireA.Value.Date) : DateOnly.MaxValue;
        var startB = DateOnly.FromDateTime(effectB.Date);
        var endB = expireB.HasValue ? DateOnly.FromDateTime(expireB.Value.Date) : DateOnly.MaxValue;
        return startA <= endB && startB <= endA;
    }

    /// <summary>
    /// Sorts coverages by dependency order (parents before children) using topological sort.
    /// This ensures that when creating coverages, parent coverages are created before child coverages.
    /// </summary>
    private List<ProProductCoverageDto> SortCoveragesByDependency(List<ProProductCoverageDto> coverages)
    {
        if (coverages == null || !coverages.Any())
        {
            return coverages?.ToList() ?? new List<ProProductCoverageDto>();
        }

        // Create a mapping of coverage by a unique key (using CoverageId + SeqNumber as identifier)
        // For new coverages, we'll use index as a fallback
        var coverageByKey = new Dictionary<string, ProProductCoverageDto>();
        var coverageIndex = new Dictionary<ProProductCoverageDto, int>();
        
        for (int i = 0; i < coverages.Count; i++)
        {
            var coverage = coverages[i];
            var key = $"{coverage.CoverageId}_{coverage.SeqNumber}_{i}";
            coverageByKey[key] = coverage;
            coverageIndex[coverage] = i;
        }

        // Build dependency graph: coverage -> list of coverages that depend on it (children)
        var children = new Dictionary<ProProductCoverageDto, List<ProProductCoverageDto>>();
        var inDegree = new Dictionary<ProProductCoverageDto, int>();
        
        foreach (var coverage in coverages)
        {
            children[coverage] = new List<ProProductCoverageDto>();
            inDegree[coverage] = 0;
        }

        // Build edges: if coverage A has ParentId = B's ID, then B is parent of A
        // Note: For new coverages, ParentId won't match any coverage in the batch since they don't have IDs yet
        // So we'll primarily rely on the order and handle ParentId validation separately
        foreach (var coverage in coverages)
        {
            if (coverage.ParentId.HasValue)
            {
                // Try to find parent in the same batch
                // Since new coverages don't have stable IDs, we can't reliably match by ID
                // This sorting is mainly for cases where ParentId references existing coverages
                // or for maintaining a reasonable order
                var parent = coverages.FirstOrDefault(c => c.Id == coverage.ParentId.Value && c != coverage);
                if (parent != null)
                {
                    children[parent].Add(coverage);
                    inDegree[coverage]++;
                }
            }
        }

        // Topological sort: start with coverages that have no parent (inDegree = 0)
        var queue = new Queue<ProProductCoverageDto>();
        foreach (var coverage in coverages)
        {
            if (inDegree[coverage] == 0)
            {
                queue.Enqueue(coverage);
            }
        }

        var result = new List<ProProductCoverageDto>();
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            result.Add(current);

            // Process children
            foreach (var child in children[current])
            {
                inDegree[child]--;
                if (inDegree[child] == 0)
                {
                    queue.Enqueue(child);
                }
            }
        }

        // If there are cycles or unmatched dependencies, add remaining coverages
        // This handles cases where ParentId references coverages outside this batch
        foreach (var coverage in coverages)
        {
            if (!result.Contains(coverage))
            {
                result.Add(coverage);
            }
        }

        return result;
    }

    /// <summary>
    /// Role IDs from <c>hr_employee_role_rel</c> that are valid for the current calendar day (via <c>Clock.Now.Date</c>).
    /// Empty when there is no <see cref="HrEmployee"/> for the current user.
    /// </summary>
    private async Task<List<Guid>> GetCurrentUserValidEmployeeRoleIdsAsync()
    {
        if (!CurrentUser.Id.HasValue)
        {
            return new List<Guid>();
        }

        var employeeQuery = await HrEmployeeRepository.GetQueryableAsync();
        var employee = await employeeQuery
            .Where(e => e.UserId == CurrentUser.Id.Value && !e.IsDeleted)
            .FirstOrDefaultAsync();
        if (employee == null)
        {
            return new List<Guid>();
        }

        var today = Clock.Now.Date;
        var roleRelQuery = await HrEmployeeRoleRelRepository.GetQueryableAsync();
        var rels = await roleRelQuery
            .Where(r => r.EmployeeId == employee.Id)
            .Select(r => new { r.RoleId, r.EffectDate, r.ExpireDate })
            .ToListAsync();

        return rels
            .Where(r =>
                r.EffectDate.Date <= today
                && (!r.ExpireDate.HasValue || r.ExpireDate.Value.Date >= today))
            .Select(r => r.RoleId)
            .Distinct()
            .ToList();
    }

    public virtual async Task<List<ProProductDto>> GetByLobIdAndPartnerIdAsync(
        Guid lobId,
        Guid partnerId,
        Guid? channelId = null,
        Guid? appChannelId = null,
        bool applyChannelDistributionFilter = true,
        string? insurerCode = null)
    {
        if (applyChannelDistributionFilter)
        {
            if (!channelId.HasValue || channelId.Value == Guid.Empty)
            {
                return new List<ProProductDto>();
            }
        }

        var effectivePartnerId = partnerId;
        if (effectivePartnerId == Guid.Empty && !string.IsNullOrWhiteSpace(insurerCode))
        {
            var code = insurerCode.Trim();
            var partnerQ = await ResPartnerRepository.GetQueryableAsync();
            var partner = await partnerQ
                .Where(x => !x.IsDeleted && x.Code == code)
                .FirstOrDefaultAsync();
            if (partner == null)
            {
                return new List<ProProductDto>();
            }

            effectivePartnerId = partner.Id;
        }
        else if (effectivePartnerId == Guid.Empty)
        {
            return new List<ProProductDto>();
        }

        var query = await Repository.GetQueryableAsync();
        
        // Use eager loading with Include to fetch all related data in a single query with joins
        var entities = await query
            .Where(x => x.LobId == lobId
                        && x.PartnerId == effectivePartnerId
                        && x.IsPlan == "N"
                        && x.Status == ProProductStatus.Active)
            .ToListAsync();

        // Phân phối: lọc theo ChannelId (kênh bán). Tạm không lọc AppChannelId — dữ liệu PROPRODUCTDISTRIBUTION
        // chưa khớp res_app_channel WEB; bật lại khi đã chuẩn hóa AppChannelId.
        _ = appChannelId;

        if (applyChannelDistributionFilter)
        {
            var userValidRoleIds = await GetCurrentUserValidEmployeeRoleIdsAsync();

            var distributionQuery = await ProductDistributionRepository.GetQueryableAsync();

            // Step 1: products with an active channel distribution for the given channelId
            var channelAllowedProductIds = await distributionQuery
                .Where(d =>
                    !d.IsDeleted
                    && d.Status == ProProductDistributionStatus.Active
                    && d.ChannelId == channelId!.Value)
                .Select(d => d.ProductId)
                .Distinct()
                .ToListAsync();

            // Step 2: among those, apply employee role filter.
            // Role-only rows (ChannelId == null && AppChannelId == null && EmployeeRoleId != null)
            // define which roles are allowed for a product.
            // A product passes only if it has role-only rows and at least one matches the user's roles.
            var roleDistributions = await distributionQuery
                .Where(d =>
                    !d.IsDeleted
                    && d.Status == ProProductDistributionStatus.Active
                    && d.EmployeeRoleId.HasValue
                    && !d.ChannelId.HasValue
                    && !d.AppChannelId.HasValue
                    && channelAllowedProductIds.Contains(d.ProductId))
                .Select(d => new { d.ProductId, d.EmployeeRoleId })
                .ToListAsync();

            var productsWithRoleRestriction = roleDistributions
                .Select(d => d.ProductId)
                .Distinct()
                .ToHashSet();

            var productsPassingRoleCheck = roleDistributions
                .Where(d => userValidRoleIds.Contains(d.EmployeeRoleId!.Value))
                .Select(d => d.ProductId)
                .Distinct()
                .ToHashSet();

            var allowedProductIds = channelAllowedProductIds
                .Where(pid => productsWithRoleRestriction.Contains(pid) && productsPassingRoleCheck.Contains(pid))
                .ToList();

            entities = entities
                .Where(p => allowedProductIds.Contains(p.Id))
                .ToList();
        }

        if (entities.Count == 0)
        {
            return new List<ProProductDto>();
        }

        // Load pro_product_type for ProductTypeCode (VCX / TNDS)
        var productTypeIds = entities.Where(x => x.ProductTypeId.HasValue).Select(x => x.ProductTypeId!.Value).Distinct().ToList();
        var productTypeCodeById = new Dictionary<Guid, string>();
        if (productTypeIds.Count > 0)
        {
            var productTypes = await ProductTypeRepository.GetQueryableAsync();
            var types = await productTypes.Where(x => productTypeIds.Contains(x.Id)).ToListAsync();
            foreach (var pt in types)
                productTypeCodeById[pt.Id] = pt.Code ?? string.Empty;
        }

        // Get all product IDs
        var productIds = entities.Select(x => x.Id).ToList();

        // Load all ProductCoverages with eager loading for Levels and Terms
        var coveragesQuery = await ProductCoverageRepository.GetQueryableAsync();
        var productCoverages = await coveragesQuery
            .Where(x => productIds.Contains(x.ProductId))
            .Include(x => x.Interactions)
            .Include(x => x.CoverageLevels)
                .ThenInclude(level => level.Terms)
                    .ThenInclude(term => term.CoverageLevelType)
            .ToListAsync();

        // Group coverages by product ID
        var coveragesByProductId = productCoverages.GroupBy(x => x.ProductId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Load all related data in batch queries
        var coverageIds = productCoverages.Select(x => x.CoverageId).Distinct().ToList();
        var coverageQuery = await CoverageRepository.GetQueryableAsync();
        var coverages = await coverageQuery
            .Where(x => coverageIds.Contains(x.Id))
            .ToListAsync();
        var coverageDict = coverages.ToDictionary(x => x.Id);

        // Load taxes for coverages (TaxId -> ResTax)
        var taxIds = productCoverages.Select(x => x.TaxId).Distinct().ToList();
        var taxesQuery = await TaxRepository.GetQueryableAsync();
        var taxes = await taxesQuery
            .Where(x => taxIds.Contains(x.Id))
            .ToListAsync();
        var taxDtoDict = taxes.ToDictionary(x => x.Id, x => ObjectMapper.Map<ResTax, ResTaxDto>(x));

        // Load all attributes
        var attributesQuery = await ProductAttributeRepository.GetQueryableAsync();
        var attributes = await attributesQuery
            .Where(x => productIds.Contains(x.ProductId))
            .ToListAsync();
        var attributesByProductId = attributes.GroupBy(x => x.ProductId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Load joined ProAttribute (pro_attribute) for product attributes
        var attributeIds = attributes.Select(x => x.AttributeId).Distinct().ToList();
        var proAttributeById = new Dictionary<Guid, ProAttribute>();
        if (attributeIds.Count > 0)
        {
            var proAttrQuery = await AttributeRepository.GetQueryableAsync();
            var proAttrs = await proAttrQuery
                .Where(x => attributeIds.Contains(x.Id))
                .ToListAsync();

            proAttributeById = proAttrs.ToDictionary(x => x.Id, x => x);
        }

        // Load all table rates
        var tableRatesQuery = await ProductTableRateRepository.GetQueryableAsync();
        var tableRates = await tableRatesQuery
            .Where(x => productIds.Contains(x.ProductId))
            .ToListAsync();
        var tableRatesByProductId = tableRates.GroupBy(x => x.ProductId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Load all rules
        var rulesQuery = await ProRuleRepository.GetQueryableAsync();
        var rules = await rulesQuery
            .Where(x => x.ApplyTo == "product" && productIds.Contains(x.ApplyToId))
            .ToListAsync();
        var rulesByProductId = rules.GroupBy(x => x.ApplyToId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Load all plan definitions
        var planDefinitionsQuery = await ProductPlanDefinitionRepository.GetQueryableAsync();
        var planDefinitions = await planDefinitionsQuery
            .Where(x => productIds.Contains(x.ProductId))
            .ToListAsync();
        var planDefinitionsByProductId = planDefinitions.GroupBy(x => x.ProductId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Load all distributions
        var distributionsQuery = await ProductDistributionRepository.GetQueryableAsync();
        var distributions = await distributionsQuery
            .Where(x => productIds.Contains(x.ProductId))
            .ToListAsync();
        var distributionsByProductId = distributions.GroupBy(x => x.ProductId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Collect all creator IDs
        var creatorIds = new HashSet<Guid?>();
        foreach (var tableRate in tableRates)
            if (tableRate.CreatorId.HasValue) creatorIds.Add(tableRate.CreatorId.Value);
        foreach (var rule in rules)
            if (rule.CreatorId.HasValue) creatorIds.Add(rule.CreatorId.Value);
        foreach (var plan in planDefinitions)
            if (plan.CreatorId.HasValue) creatorIds.Add(plan.CreatorId.Value);
        foreach (var coverage in productCoverages)
        {
            foreach (var interaction in coverage.Interactions)
                if (interaction.CreatorId.HasValue) creatorIds.Add(interaction.CreatorId.Value);
            foreach (var level in coverage.CoverageLevels)
            {
                if (level.CreatorId.HasValue) creatorIds.Add(level.CreatorId.Value);
                foreach (var term in level.Terms)
                    if (term.CreatorId.HasValue) creatorIds.Add(term.CreatorId.Value);
            }
        }

        // Load all users
        var userIds = creatorIds.Where(id => id.HasValue).Select(id => id.Value).ToList();
        var userDict = new Dictionary<Guid, IdentityUser>();
        if (userIds.Any())
        {
            var usersQuery = await UserRepository.GetQueryableAsync();
            var users = await usersQuery
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();
            userDict = users.ToDictionary(u => u.Id, u => u);
        }

        // Map entities to DTOs
        var result = new List<ProProductDto>();
        foreach (var entity in entities)
        {
            var dto = ObjectMapper.Map<ProProduct, ProProductDto>(entity);
            if (entity.ProductTypeId.HasValue && productTypeCodeById.TryGetValue(entity.ProductTypeId.Value, out var typeCode))
                dto.ProductTypeCode = typeCode;

            // Map attributes
            if (attributesByProductId.TryGetValue(entity.Id, out var productAttributes))
            {
                dto.ProductAttributes = ObjectMapper.Map<List<ProProductAttribute>, List<ProProductAttributeDto>>(productAttributes);

                // Attach joined pro_attribute info for each product attribute (flattened onto DTO)
                foreach (var pa in dto.ProductAttributes)
                {
                    if (proAttributeById.TryGetValue(pa.AttributeId, out var attr))
                    {
                        pa.AttributeCode = attr.Code;
                        pa.AttributeName = attr.Name;
                        pa.AttributeStatus = attr.Status;
                        pa.AttributeSpec = attr.Spec;
                        pa.AttributeDescription = attr.Description;
                        pa.AttributeDataPath = attr.DataPath;
                        pa.AttributeDataType = attr.DataType;
                        pa.AttributeComputeScript = attr.ComputeScript;
                        pa.AttributeClearDataScript = attr.ClearDataScript;
                    }
                }
            }

            // Map coverages with levels and terms (already loaded with Include)
            if (coveragesByProductId.TryGetValue(entity.Id, out var productCoveragesList))
            {
                dto.ProductCoverages = ObjectMapper.Map<List<ProProductCoverage>, List<ProProductCoverageDto>>(productCoveragesList);
                
                foreach (var coverageDto in dto.ProductCoverages)
                {
                    var sourceCoverage = productCoveragesList.FirstOrDefault(c => c.Id == coverageDto.Id);

                    // Map tax info from TaxId
                    if (taxDtoDict.TryGetValue(coverageDto.TaxId, out var taxDto))
                    {
                        coverageDto.Tax = taxDto;
                    }

                    // Map interactions
                    if (sourceCoverage != null && sourceCoverage.Interactions != null && sourceCoverage.Interactions.Any())
                    {
                        coverageDto.ProductCoverageInteractions = ObjectMapper.Map<List<ProProductCoverageInteraction>, List<ProProductCoverageInteractionDto>>(sourceCoverage.Interactions.ToList());
                        
                        // Populate creator names for interactions
                        foreach (var interaction in coverageDto.ProductCoverageInteractions)
                        {
                            interaction.InteractionTypeCode = interaction.InteractionType.ToString().ToLowerInvariant();
                            if (interaction.CreatorId.HasValue && userDict.TryGetValue(interaction.CreatorId.Value, out var user))
                            {
                                interaction.CreatorName = user.UserName;
                            }
                        }
                    }

                    // Map levels and terms (already loaded with Include)
                    if (sourceCoverage != null)
                    {
                        // Ensure levels are mapped (they should be automatically mapped by ObjectMapper, but we ensure they're set)
                        if (sourceCoverage.CoverageLevels != null && sourceCoverage.CoverageLevels.Any())
                        {
                            coverageDto.ProductCoverageLevels = ObjectMapper.Map<List<ProProductCoverageLevel>, List<ProProductCoverageLevelDto>>(sourceCoverage.CoverageLevels.ToList());
                            
                            // Map terms for each level (already loaded with Include via ThenInclude)
                            foreach (var levelDto in coverageDto.ProductCoverageLevels)
                            {
                                var sourceLevel = sourceCoverage.CoverageLevels.FirstOrDefault(l => l.Id == levelDto.Id);
                                if (sourceLevel != null && sourceLevel.Terms != null && sourceLevel.Terms.Any())
                                {
                                    levelDto.Terms = ObjectMapper.Map<List<ProProductCoverageLevelTerm>, List<ProProductCoverageLevelTermDto>>(sourceLevel.Terms.ToList());
                                    
                                    // Populate creator names for terms
                                    foreach (var term in levelDto.Terms)
                                    {
                                        if (term.CreatorId.HasValue && userDict.TryGetValue(term.CreatorId.Value, out var userTerm))
                                        {
                                            term.CreatorName = userTerm.UserName;
                                        }

                                        // Attach joined pro_coverage_level_type info (flattened)
                                        var sourceTerm = sourceLevel.Terms.FirstOrDefault(t => t.Id == term.Id);
                                        var clt = sourceTerm?.CoverageLevelType;
                                        if (clt != null)
                                        {
                                            term.CoverageLevelTypeCode = clt.Code;
                                            term.CoverageLevelTypeName = clt.Name;
                                            term.CoverageLevelTypeDescription = clt.Description;
                                            term.CoverageLevelTypeStatus = clt.Status;
                                        }
                                    }
                                }
                                
                                // Populate creator name for level
                                if (levelDto.CreatorId.HasValue && userDict.TryGetValue(levelDto.CreatorId.Value, out var userLevel))
                                {
                                    levelDto.CreatorName = userLevel.UserName;
                                }
                            }
                        }
                    }

                    // Map Coverage
                    if (coverageDict.TryGetValue(coverageDto.CoverageId, out var coverage))
                    {
                        coverageDto.Coverage = ObjectMapper.Map<ProCoverage, ProCoverageDto>(coverage);
                    }
                }
            }

            // Map table rates
            if (tableRatesByProductId.TryGetValue(entity.Id, out var productTableRates))
            {
                dto.ProductTableRates = ObjectMapper.Map<List<ProProductTableRate>, List<ProProductTableRateDto>>(productTableRates);
                foreach (var tableRate in dto.ProductTableRates)
                {
                    if (tableRate.CreatorId.HasValue && userDict.TryGetValue(tableRate.CreatorId.Value, out var user))
                    {
                        tableRate.CreatorName = user.UserName;
                    }
                }
            }

            // Map rules
            if (rulesByProductId.TryGetValue(entity.Id, out var proRules))
            {
                dto.ProRules = ObjectMapper.Map<List<ProRule>, List<ProRuleDto>>(proRules);
                foreach (var rule in dto.ProRules)
                {
                    if (rule.CreatorId.HasValue && userDict.TryGetValue(rule.CreatorId.Value, out var user))
                    {
                        rule.CreatorName = user.UserName;
                    }
                }
            }

            // Map plan definitions
            if (planDefinitionsByProductId.TryGetValue(entity.Id, out var productPlanDefinitions))
            {
                dto.ProductPlanDefinitions = ObjectMapper.Map<List<ProProductPlanDefinition>, List<ProProductPlanDefinitionDto>>(productPlanDefinitions);
                foreach (var plan in dto.ProductPlanDefinitions)
                {
                    if (plan.CreatorId.HasValue && userDict.TryGetValue(plan.CreatorId.Value, out var user))
                    {
                        plan.CreatorName = user.UserName;
                    }
                }
            }

            // Map distributions
            if (distributionsByProductId.TryGetValue(entity.Id, out var productDistributions))
            {
                dto.ProductDistributions = ObjectMapper.Map<List<ProProductDistribution>, List<ProProductDistributionDto>>(productDistributions);
            }

            result.Add(dto);
        }

        return result;
    }

    private static readonly string DeductibleTypeCode = "DEDUCTIBLE";

    public virtual async Task<List<DeductibleOptionsForCoverageDto>> GetDeductibleOptionsAsync(GetDeductibleOptionsInput input)
    {
        if (input.ProductIds == null || input.ProductIds.Count == 0)
            return new List<DeductibleOptionsForCoverageDto>();

        var productIds = input.ProductIds.Where(x => x != Guid.Empty).Distinct().ToList();
        if (productIds.Count == 0)
            return new List<DeductibleOptionsForCoverageDto>();

        var issueDate = (input.IssueDate ?? Clock.Now).Date;

        var coveragesQuery = await ProductCoverageRepository.GetQueryableAsync();
        var productCoverages = await coveragesQuery
            .Where(x => productIds.Contains(x.ProductId))
            .Include(x => x.CoverageLevels)
                .ThenInclude(level => level.Terms)
                    .ThenInclude(term => term.CoverageLevelType)
            .ToListAsync();

        var result = new List<DeductibleOptionsForCoverageDto>();

        foreach (var coverage in productCoverages)
        {
            if (coverage.CoverageLevels == null || !coverage.CoverageLevels.Any())
                continue;

            var termByValue = new Dictionary<string, DeductibleTermMetaDto>();
            // 1 CoverageLevel có n Terms = n mức miễn thường; mỗi term là 1 option (key = term.Id). Label = pro_product_coverage_level.Name.
            var termOptions = new List<(Guid TermId, decimal FromAmount, bool ConditionEvalResult, string LevelName)>();

            foreach (var level in coverage.CoverageLevels)
            {
                if (level.EffectDate > issueDate)
                    continue;
                if (level.ExpireDate.HasValue && level.ExpireDate.Value.Date < issueDate)
                    continue;
                if (level.Terms == null)
                    continue;

                var levelName = level.Name?.Trim() ?? string.Empty;
                var levelConditionScript = level.ConditionalScript?.Trim();
                var hasLevelCondition = !string.IsNullOrEmpty(levelConditionScript);

                foreach (var term in level.Terms)
                {
                    var code = term.CoverageLevelType?.Code?.Trim().ToUpperInvariant();
                    if (code != DeductibleTypeCode)
                        continue;
                    if (!term.CoverageLevelBasisId.HasValue || term.CoverageLevelBasisId.Value == Guid.Empty)
                        continue;

                    var fromAmount = term.FromAmount;
                    var toAmount = term.ToAmount;
                    var termId = term.Id;
                    var key = termId.ToString();

                    // Chỉ khi có condition_script mới eval; không có script thì ConditionEvalResult = false (không coi là true).
                    bool conditionResult = false;
                    if (hasLevelCondition)
                    {
                        try
                        {
                            conditionResult = await EvaluateConditionScriptAsync(
                                levelConditionScript,
                                issueDate,
                                fromAmount,
                                toAmount,
                                input.CarGroup,
                                input.CarType,
                                input.SeatingCapacity,
                                input.Weight,
                                input.CarPurpose);
                        }
                        catch
                        {
                            conditionResult = false;
                        }
                    }

                    termByValue[key] = new DeductibleTermMetaDto
                    {
                        CoverageLevelTypeId = term.CoverageLevelTypeId,
                        CoverageLevelBasisId = term.CoverageLevelBasisId!.Value,
                        AmountType = term.AmountType ?? "FIXED",
                        FromAmount = fromAmount,
                        ToAmount = toAmount,
                        ConditionScript = term.ConditionScript,
                        ComputeScript = term.ComputeScript
                    };
                    termOptions.Add((termId, fromAmount, conditionResult, levelName));
                }
            }

            if (termOptions.Count == 0)
                continue;

            // Sort: condition_script eval true lên đầu, sau đó theo FromAmount tăng dần
            var inv = System.Globalization.CultureInfo.InvariantCulture;
            var sortedTerms = termOptions
                .OrderByDescending(t => t.ConditionEvalResult)
                .ThenBy(t => t.FromAmount)
                .ToList();
            var options = sortedTerms.Select(t =>
            {
                var key = t.TermId.ToString();
                // Label = name của pro_product_coverage_level; fallback = FromAmount format (giữ tương thích). Value = term id (gửi lên API tính phí và lưu policy_coverage_level).
                var label = !string.IsNullOrEmpty(t.LevelName)
                    ? t.LevelName
                    : t.FromAmount.ToString("N0", System.Globalization.CultureInfo.InvariantCulture);
                return new DeductibleOptionItemDto
                {
                    Value = key,
                    Label = label,
                    ConditionEvalResult = t.ConditionEvalResult
                };
            }).ToList();

            result.Add(new DeductibleOptionsForCoverageDto
            {
                ProductId = coverage.ProductId,
                ProductCoverageId = coverage.Id,
                CoverageId = coverage.CoverageId,
                Options = options,
                TermByValue = termByValue
            });
        }

        return result;
    }

    /// <summary>
    /// Evaluate pro_product_coverage_level.condition_script (Python); returns true/false.
    /// If script is null/empty returns true. On Python error or missing runtime returns false.
    /// Khi eval, truyền vào script: carGroup, carType, seatingCapacity, weight, carPurpose.
    /// </summary>
    private static async Task<bool> EvaluateConditionScriptAsync(
        string conditionalScript,
        DateTime issueDate,
        decimal fromAmount,
        decimal toAmount,
        string? carGroup = null,
        string? carType = null,
        int? seatingCapacity = null,
        decimal? weight = null,
        string? carPurpose = null)
    {
        if (string.IsNullOrWhiteSpace(conditionalScript))
            return true;

        // Payload gửi sang Python: script + issueDate, fromAmount, toAmount + tham số cho eval (carGroup, carType, seatingCapacity, weight, carPurpose)
        var payload = new
        {
            script = conditionalScript.Trim(),
            issueDate = issueDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
            fromAmount,
            toAmount,
            carGroup = carGroup ?? string.Empty,
            carType = carType ?? string.Empty,
            seatingCapacity = seatingCapacity ?? 0,
            weight = weight ?? 0m,
            carPurpose = carPurpose ?? string.Empty
        };
        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var b64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

        // eval(script, ctx, ctx): ctx có sẵn issueDate, fromAmount, toAmount, carGroup, carType, seatingCapacity, weight
        const string pythonProgram = @"
import base64
import json
import sys

payload = json.loads(base64.b64decode(sys.argv[1]).decode('utf-8'))
script = (payload.get('script') or '').strip()
if not script:
    print('true')
    sys.exit(0)

issue_date = payload.get('issueDate') or ''
from_amount = float(payload.get('fromAmount', 0))
to_amount = float(payload.get('toAmount', 0))
car_group = payload.get('carGroup') or ''
car_type = payload.get('carType') or ''
seating_capacity = int(payload.get('seatingCapacity', 0) or 0)
weight = float(payload.get('weight', 0) or 0)
car_purpose = payload.get('carPurpose') or ''
# Variables for condition_script eval: carGroup, carType, seatingCapacity, weight, carPurpose (+ issueDate, fromAmount, toAmount). Alias carGroupCode/carTypeCode for backward compatibility.
ctx = {'__builtins__': __builtins__, 'issueDate': issue_date, 'fromAmount': from_amount, 'toAmount': to_amount, 'carGroup': car_group, 'carType': car_type, 'carGroupCode': car_group, 'carTypeCode': car_type, 'seatingCapacity': seating_capacity, 'weight': weight, 'carPurpose': car_purpose, 'result': False}
try:
    try:
        ctx['result'] = bool(eval(script, ctx, ctx))
    except SyntaxError:
        exec(script, ctx, ctx)
        ctx['result'] = bool(ctx.get('result', False))
    print('true' if ctx['result'] else 'false')
except Exception as e:
    sys.stderr.write(str(e) + '\n')
    print('false')
";

        try
        {
            var stdout = await RunPythonAsync(b64, pythonProgram);
            var s = (stdout ?? string.Empty).Trim().ToLowerInvariant();
            return s == "true" || s == "1";
        }
        catch
        {
            return false;
        }
    }

    private static async Task<string> RunPythonAsync(string payloadB64, string pythonProgram)
    {
        var overrideExe = (Environment.GetEnvironmentVariable("IONE_PYTHON") ?? string.Empty).Trim();
        var candidates = new List<string>();
        if (!string.IsNullOrWhiteSpace(overrideExe))
            candidates.Add(overrideExe);
        else
        {
            candidates.Add("python3");
            candidates.Add("python");
            candidates.Add("/usr/bin/python3");
            candidates.Add("/usr/local/bin/python3");
            candidates.Add("/opt/homebrew/bin/python3");
        }

        var workingDir = AppContext.BaseDirectory;
        if (string.IsNullOrWhiteSpace(workingDir) || !Directory.Exists(workingDir))
            workingDir = Directory.GetCurrentDirectory();
        if (string.IsNullOrWhiteSpace(workingDir) || !Directory.Exists(workingDir))
            workingDir = "/";

        foreach (var exe in candidates.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            try
            {
                using var p = new Process();
                p.StartInfo = new ProcessStartInfo
                {
                    FileName = exe,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = workingDir
                };
                p.StartInfo.ArgumentList.Add("-c");
                p.StartInfo.ArgumentList.Add(pythonProgram);
                p.StartInfo.ArgumentList.Add(payloadB64);

                if (!p.Start()) continue;

                var stdoutTask = p.StandardOutput.ReadToEndAsync();
                var stderrTask = p.StandardError.ReadToEndAsync();
                await p.WaitForExitAsync();
                var stdout = await stdoutTask ?? string.Empty;
                var stderr = await stderrTask ?? string.Empty;
                if (p.ExitCode != 0)
                    throw new UserFriendlyException($"Condition script failed (Python '{exe}' exit {p.ExitCode}). {stderr}".Trim());
                return stdout;
            }
            catch (UserFriendlyException) { throw; }
            catch { /* try next */ }
        }

        throw new UserFriendlyException("Python runtime not available for condition_script evaluation. Set IONE_PYTHON to the Python executable path.");
    }

    public virtual async Task<List<ProProductDto>> GetByRootProductIdAndPlanDefinitionIdAsync(Guid rootProductId, Guid planDefinitionId)
    {
        var query = await Repository.GetQueryableAsync();
        var entities = await query
            .Where(x => x.RootProductId == rootProductId && x.IsPlan == "Y" && x.PlanDefinitionId == planDefinitionId)
            .ToListAsync();

        var result = new List<ProProductDto>();
        foreach (var entity in entities)
        {
            result.Add(await MapToDtoWithAttributesAsync(entity));
        }

        return result;
    }
}
