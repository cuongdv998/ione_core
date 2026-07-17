using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Security.Cryptography;
using ClosedXML.Excel;
using iOne.AccountPaymentRequests;
using iOne.HrDepartments;
using iOne.HrEmployees;
using iOne.InsurerDictionaries;
using iOne.Policies;
using iOne.Policy.Payments;
using iOne.PolicyTypes;
using iOne.ResObjectTypes;
using iOne.ResPaymentMethods;
using iOne.ResPaymentTypes;
using iOne.Policy.Localization;
using iOne.Policy.Permissions;
using iOne.Policy.PolicyContracts;
using iOne.PolicyContracts;
using iOne.ProAttributes;
using iOne.ProCoverages;
using iOne.ProLineOfBusinesses;
using iOne.ProProducts;
using iOne.ProProductTypes;
using iOne.ProRules;
using iOne.ResCurrencies;
using iOne.ResChannels;
using iOne.ResCarBrands;
using iOne.ResPartners;
using iOne.ResProvinces;
using iOne.ResWards;
using iOne.Product.ProductPricing;
using iOne.Product.ProProducts;
using iOne.ResCarCategories;
using iOne.ResCarGroups;
using iOne.ResCarLines;
using iOne.ResCarModels;
using iOne.ResCarTypes;
using iOne.ResCustomers;
using iOne.ResDocuments;
using iOne.ResFeeItems;
using iOne.ResMotorClasses;
using iOne.ResSequences;
using iOne.Workflow;
using iOne.AdminConfigs;
using iOne.WorkTasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using System.Text.Json.Serialization;

namespace iOne.Policy.Policies;

[RemoteService(Name = PolicyRemoteServiceConsts.RemoteServiceName)]
[Authorize(PolicyPermissions.Default)]
public class PolicyAppService : CrudAppService<
    iOne.Policies.Policy,
    PolicyDto,
    Guid,
    GetPoliciesInput,
    CreatePolicyDto,
    UpdatePolicyDetailDto>, IPolicyAppService
{
    private const string PolicyAmountFeeItemCode = "POLICY_AMOUNT";
    private const string PaymentTypeDebtCode = "PAYMENT_DEBT";
    private const string CustomerCodeSequenceCode = "CUSTOMER_SEQ";

    private const string MotorbikePolicyLobCode = "MOTOR";

    private const string MotorbikeCreatePolicyApprovalBusinessCode = "MOTORBIKE_CREATE_POLICY_APPROVAL";

    private static bool IsMotorbikePolicyLob(iOne.Policies.Policy policy)
    {
        var code = policy.Lob?.Code;
        return !string.IsNullOrWhiteSpace(code)
               && string.Equals(code.Trim(), MotorbikePolicyLobCode, StringComparison.OrdinalIgnoreCase);
    }

    public PolicyAppService(
        IRepository<iOne.Policies.Policy, Guid> repository,
        PolicyManager manager,
        IPolicyRepository policyRepository,
        PolicyContractManager contractManager,
        PolicyVersionManager policyVersionManager,
        PolicyProductManager policyProductManager,
        PolicyCoverageManager policyCoverageManager,
        PolicyCoverageLevelManager policyCoverageLevelManager,
        PolicyRiskObjectManager policyRiskObjectManager,
        PolicyRiskMotorManager policyRiskMotorManager,
        PolicyDocumentManager policyDocumentManager,
        PolicyAmountManager policyAmountManager,
        IResDocumentRepository resDocumentRepository,
        IResSequenceRepository resSequenceRepository,
        IRepository<ResFeeItem, Guid> resFeeItemRepository,
        IRepository<ProProduct, Guid> proProductRepository,
        IRepository<ProProductType, Guid> proProductTypeRepository,
        IPolicyDocumentRepository policyDocumentRepository,
        IPolicyProductRepository policyProductRepository,
        IPolicyCoverageRepository policyCoverageRepository,
        IPolicyCoverageLevelRepository policyCoverageLevelRepository,
        IPolicyRiskObjectRepository policyRiskObjectRepository,
        IRepository<PolicyRiskObjectDocument> policyRiskObjectDocumentRepository,
        IPolicyRiskMotorRepository policyRiskMotorRepository,
        IPolicyAmountRepository policyAmountRepository,
        IPolicyContractDocumentRepository policyContractDocumentRepository,
        PolicyContractDocumentManager policyContractDocumentManager,
        IRepository<ResCarBrand, Guid> resCarBrandRepository,
        IRepository<ResCarModel, Guid> resCarModelRepository,
        IRepository<ResCarLine, Guid> resCarLineRepository,
        IRepository<ResCarGroup, Guid> resCarGroupRepository,
        IRepository<ResCarType, Guid> resCarTypeRepository,
        IRepository<ResMotorClass, Guid> resMotorClassRepository,
        IRepository<ResCarCategory, Guid> resCarCategoryRepository,
        IRepository<HrEmployee, Guid> hrEmployeeRepository,
        IRepository<HrDepartment, Guid> hrDepartmentRepository,
        IResChannelRepository resChannelRepository,
        IPolicyContractRepository policyContractRepository,
        IPolicyVersionRepository policyVersionRepository,
        IPolicyCertificateRepository policyCertificateRepository,
        IRepository<ProRule, Guid> proRuleRepository,
        IRepository<IdentityUser, Guid> userRepository,
        IRepository<AccountPaymentRequest, Guid> accountPaymentRequestRepository,
        IRepository<ResPaymentMethod, Guid> resPaymentMethodRepository,
        IRepository<ResPaymentType, Guid> resPaymentTypeRepository,
        AccountPaymentRequestManager accountPaymentRequestManager,
        IElsaWorkflowService elsaWorkflowService,
        IRepository<ResPartner, Guid> resPartnerRepository,
        IRepository<ResCurrency, Guid> resCurrencyRepository,
        IRepository<ProLineOfBusiness, Guid> proLineOfBusinessRepository,
        IRepository<ResProvince, Guid> resProvinceRepository,
        IRepository<ResWard, Guid> resWardRepository,
        IRepository<ProCoverage, Guid> proCoverageRepository,
        IRepository<ProProductCoverage, Guid> proProductCoverageRepository,
        IProductPriceAppService productPriceAppService,
        IProProductAppService proProductAppService,
        IPolicyTypeRepository policyTypeRepository,
        IRepository<ResObjectType, Guid> resObjectTypeRepository,
        IWorkTaskRepository workTaskRepository,
        IRepository<AdminConfig, Guid> adminConfigRepository,
        IRepository<ResCustomer, Guid> resCustomerRepository,
        IRepository<ProProductAttribute, Guid> proProductAttributeRepository,
        IRepository<ProAttribute, Guid> proAttributeRepository,
        IRepository<InsurerDictionary, Guid> insurerDictionaryRepository)
        : base(repository)
    {
        Manager = manager;
        PolicyRepository = policyRepository;
        ContractManager = contractManager;
        PolicyVersionManager = policyVersionManager;
        PolicyProductManager = policyProductManager;
        PolicyCoverageManager = policyCoverageManager;
        PolicyCoverageLevelManager = policyCoverageLevelManager;
        PolicyRiskObjectManager = policyRiskObjectManager;
        PolicyRiskMotorManager = policyRiskMotorManager;
        PolicyDocumentManager = policyDocumentManager;
        PolicyAmountManager = policyAmountManager;
        ResDocumentRepository = resDocumentRepository;
        ResSequenceRepository = resSequenceRepository;
        ResFeeItemRepository = resFeeItemRepository;
        ProProductRepository = proProductRepository;
        ProProductTypeRepository = proProductTypeRepository;
        PolicyDocumentRepository = policyDocumentRepository;
        PolicyProductRepository = policyProductRepository;
        PolicyCoverageRepository = policyCoverageRepository;
        PolicyCoverageLevelRepository = policyCoverageLevelRepository;
        PolicyRiskObjectRepository = policyRiskObjectRepository;
        PolicyRiskObjectDocumentRepository = policyRiskObjectDocumentRepository;
        PolicyRiskMotorRepository = policyRiskMotorRepository;
        PolicyAmountRepository = policyAmountRepository;
        PolicyContractDocumentRepository = policyContractDocumentRepository;
        PolicyContractDocumentManager = policyContractDocumentManager;
        ResCarBrandRepository = resCarBrandRepository;
        ResCarModelRepository = resCarModelRepository;
        ResCarLineRepository = resCarLineRepository;
        ResCarGroupRepository = resCarGroupRepository;
        ResCarTypeRepository = resCarTypeRepository;
        ResMotorClassRepository = resMotorClassRepository;
        ResCarCategoryRepository = resCarCategoryRepository;
        HrEmployeeRepository = hrEmployeeRepository;
        HrDepartmentRepository = hrDepartmentRepository;
        ResChannelRepository = resChannelRepository;
        PolicyContractRepository = policyContractRepository;
        PolicyVersionRepository = policyVersionRepository;
        PolicyCertificateRepository = policyCertificateRepository;
        ProRuleRepository = proRuleRepository;
        UserRepository = userRepository;
        AccountPaymentRequestRepository = accountPaymentRequestRepository;
        ResPaymentMethodRepository = resPaymentMethodRepository;
        ResPaymentTypeRepository = resPaymentTypeRepository;
        AccountPaymentRequestManager = accountPaymentRequestManager;
        _elsaWorkflowService = elsaWorkflowService;
        ResPartnerRepository = resPartnerRepository;
        ResCurrencyRepository = resCurrencyRepository;
        ProLineOfBusinessRepository = proLineOfBusinessRepository;
        ResProvinceRepository = resProvinceRepository;
        ResWardRepository = resWardRepository;
        ProCoverageRepository = proCoverageRepository;
        ProProductCoverageRepository = proProductCoverageRepository;
        ProductPriceAppService = productPriceAppService;
        _proProductAppService = proProductAppService;
        PolicyTypeRepository = policyTypeRepository;
        ResObjectTypeRepository = resObjectTypeRepository;
        _workTaskRepository = workTaskRepository;
        _adminConfigRepository = adminConfigRepository;
        ResCustomerRepository = resCustomerRepository;
        ProProductAttributeRepository = proProductAttributeRepository;
        ProAttributeRepository = proAttributeRepository;
        InsurerDictionaryRepository = insurerDictionaryRepository;
        LocalizationResource = typeof(PolicyResource);
        GetPolicyName = PolicyPermissions.View;
        GetListPolicyName = PolicyPermissions.View;
        CreatePolicyName = PolicyPermissions.Create;
        UpdatePolicyName = PolicyPermissions.Edit;
        DeletePolicyName = PolicyPermissions.Delete;
    }

    protected PolicyManager Manager { get; }
    protected IPolicyRepository PolicyRepository { get; }
    protected PolicyContractManager ContractManager { get; }
    protected PolicyVersionManager PolicyVersionManager { get; }
    protected PolicyProductManager PolicyProductManager { get; }
    protected PolicyCoverageManager PolicyCoverageManager { get; }
    protected PolicyCoverageLevelManager PolicyCoverageLevelManager { get; }
    protected PolicyRiskObjectManager PolicyRiskObjectManager { get; }
    protected PolicyRiskMotorManager PolicyRiskMotorManager { get; }
    protected PolicyDocumentManager PolicyDocumentManager { get; }
    protected PolicyAmountManager PolicyAmountManager { get; }
    protected IResDocumentRepository ResDocumentRepository { get; }
    protected IResSequenceRepository ResSequenceRepository { get; }
    protected IRepository<ResFeeItem, Guid> ResFeeItemRepository { get; }
    protected IRepository<ProProduct, Guid> ProProductRepository { get; }
    protected IRepository<ProProductType, Guid> ProProductTypeRepository { get; }
    protected IPolicyDocumentRepository PolicyDocumentRepository { get; }
    protected IPolicyProductRepository PolicyProductRepository { get; }
    protected IPolicyCoverageRepository PolicyCoverageRepository { get; }
    protected IPolicyCoverageLevelRepository PolicyCoverageLevelRepository { get; }
    protected IPolicyRiskObjectRepository PolicyRiskObjectRepository { get; }
    protected IRepository<PolicyRiskObjectDocument> PolicyRiskObjectDocumentRepository { get; }
    protected IPolicyRiskMotorRepository PolicyRiskMotorRepository { get; }
    protected IPolicyAmountRepository PolicyAmountRepository { get; }
    protected IPolicyContractDocumentRepository PolicyContractDocumentRepository { get; }
    protected PolicyContractDocumentManager PolicyContractDocumentManager { get; }
    protected IRepository<ResCarBrand, Guid> ResCarBrandRepository { get; }
    protected IRepository<ResCarModel, Guid> ResCarModelRepository { get; }
    protected IRepository<ResCarLine, Guid> ResCarLineRepository { get; }
    protected IRepository<ResCarGroup, Guid> ResCarGroupRepository { get; }
    protected IRepository<ResCarType, Guid> ResCarTypeRepository { get; }
    protected IRepository<ResMotorClass, Guid> ResMotorClassRepository { get; }
    protected IRepository<ResCarCategory, Guid> ResCarCategoryRepository { get; }
    protected IRepository<HrEmployee, Guid> HrEmployeeRepository { get; }
    protected IRepository<HrDepartment, Guid> HrDepartmentRepository { get; }
    protected IResChannelRepository ResChannelRepository { get; }
    protected IPolicyContractRepository PolicyContractRepository { get; }
    protected IPolicyVersionRepository PolicyVersionRepository { get; }
    protected IPolicyCertificateRepository PolicyCertificateRepository { get; }
    protected IRepository<ProRule, Guid> ProRuleRepository { get; }
    protected IRepository<IdentityUser, Guid> UserRepository { get; }
    protected IRepository<AccountPaymentRequest, Guid> AccountPaymentRequestRepository { get; }
    protected IRepository<ResPaymentMethod, Guid> ResPaymentMethodRepository { get; }
    protected IRepository<ResPaymentType, Guid> ResPaymentTypeRepository { get; }
    protected AccountPaymentRequestManager AccountPaymentRequestManager { get; }
    protected IRepository<ResPartner, Guid> ResPartnerRepository { get; }
    protected IRepository<ResCurrency, Guid> ResCurrencyRepository { get; }
    protected IRepository<ProLineOfBusiness, Guid> ProLineOfBusinessRepository { get; }
    protected IRepository<ResProvince, Guid> ResProvinceRepository { get; }
    protected IRepository<ResWard, Guid> ResWardRepository { get; }
    protected IRepository<ProCoverage, Guid> ProCoverageRepository { get; }
    protected IRepository<ProProductCoverage, Guid> ProProductCoverageRepository { get; }
    protected IProductPriceAppService ProductPriceAppService { get; }
    private readonly IProProductAppService _proProductAppService;
    protected IPolicyTypeRepository PolicyTypeRepository { get; }
    protected IRepository<ResObjectType, Guid> ResObjectTypeRepository { get; }
    private readonly IElsaWorkflowService _elsaWorkflowService;
    private readonly IWorkTaskRepository _workTaskRepository;
    private readonly IRepository<AdminConfig, Guid> _adminConfigRepository;
    protected IRepository<ResCustomer, Guid> ResCustomerRepository { get; }
    protected IRepository<ProProductAttribute, Guid> ProProductAttributeRepository { get; }
    protected IRepository<ProAttribute, Guid> ProAttributeRepository { get; }

    protected IRepository<InsurerDictionary, Guid> InsurerDictionaryRepository { get; }

    public virtual async Task<List<Dictionary<string, string>>> ExtractAttributeParametersAsync(
        ExtractPolicyAttributeParametersInputDto input)
    {
        // Response shape: a list containing one map<code, value>
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var riskObject = input?.RiskObject;
        var riskMotor = riskObject?.RiskObjectMotor;
        var attrs = input?.Product?.Attributes ?? new List<ExtractPolicyAttributeDefinitionInputDto>();

        // 1) Extract raw values first
        var values = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        var orderedCodes = new List<string>();
        foreach (var attr in attrs)
        {
            if (string.IsNullOrWhiteSpace(attr?.Code))
            {
                continue;
            }

            var code = attr.Code.Trim();
            var dataPath = attr.DataPath?.Trim();

            object? raw = null;
            var found = false;

            if (!string.IsNullOrWhiteSpace(dataPath))
            {
                // Primary: extract from the root input (supports "amountLiability", "$.amountLiability", etc.)
                if (input != null)
                {
                    found = TryGetValueByPath(input, dataPath!, out raw);
                }

                // Secondary: extract from riskObject (supports "riskObject.riskObjectMotor.carVin", etc.)
                if (!found && riskObject != null)
                {
                    found = TryGetValueByPath(riskObject, dataPath!, out raw);
                }

                // Fallback: allow direct extraction from motor for short paths like "carVin"
                if (!found && riskMotor != null)
                {
                    found = TryGetValueByPath(riskMotor, dataPath!, out raw);
                }
            }

            _ = found;
            orderedCodes.Add(code);
            values[code] = ConvertToPythonValue(raw);
        }

        // 2) Evaluate computeScript (Python) if present
        if (attrs.Any(a => !string.IsNullOrWhiteSpace(a?.ComputeScript)))
        {
            values = await EvaluateComputeScriptsWithPythonAsync(input, values, attrs);
        }

        // 3) Build output map (string values)
        foreach (var code in orderedCodes)
        {
            values.TryGetValue(code, out var v);
            map[code] = ConvertToString(v);
        }

        return new List<Dictionary<string, string>> { map };
    }

    /// <summary>
    /// Gets attribute required spec for form generation: from extract-attribute-parameters style input,
    /// parses DataPath to get parameter name, and checks ProProductAttribute.IsRequired for each attribute.
    /// </summary>
    public virtual async Task<GetAttributeRequiredSpecResultDto> GetAttributeRequiredSpecAsync(
        ExtractPolicyAttributeParametersInputDto input)
    {
        var result = new GetAttributeRequiredSpecResultDto();
        var product = input?.Product;
        var attrs = product?.Attributes ?? new List<ExtractPolicyAttributeDefinitionInputDto>();
        if (attrs.Count == 0)
        {
            return result;
        }

        Guid? productId = null;
        if (!string.IsNullOrWhiteSpace(product!.ProductId) && Guid.TryParse(product.ProductId.Trim(), out var pid))
        {
            productId = pid;
        }

        if (!productId.HasValue)
        {
            return result;
        }

        var attributeCodes = attrs
            .Where(a => !string.IsNullOrWhiteSpace(a?.Code))
            .Select(a => a!.Code!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (attributeCodes.Count == 0)
        {
            return result;
        }

        var attrQuery = await ProAttributeRepository.GetQueryableAsync();
        var attrsByCode = await AsyncExecuter.ToListAsync(
            attrQuery.Where(a => !a.IsDeleted && attributeCodes.Contains(a.Code)));
        var attributeIdByCode = attrsByCode.ToDictionary(a => a.Code, a => a.Id, StringComparer.OrdinalIgnoreCase);

        var paQuery = await ProProductAttributeRepository.GetQueryableAsync();
        var productAttrs = await AsyncExecuter.ToListAsync(
            paQuery.Where(pa => pa.ProductId == productId.Value && !pa.IsDeleted));

        foreach (var attr in attrs)
        {
            if (string.IsNullOrWhiteSpace(attr?.Code)) continue;
            var code = attr.Code.Trim();
            // Prefer request DataPath (caller-defined), fallback to ProAttribute.DataPath.
            var proAttr = attrsByCode.FirstOrDefault(a => string.Equals(a.Code, code, StringComparison.OrdinalIgnoreCase));
            var effectiveDataPath = !string.IsNullOrWhiteSpace(attr.DataPath) ? attr.DataPath : proAttr?.DataPath;
            var parameterName = GetParameterNameFromDataPath(effectiveDataPath);
            var name = attr.Name?.Trim() ?? string.Empty;
            var isRequired = false;
            if (attributeIdByCode.TryGetValue(code, out var attributeId))
            {
                var pa = productAttrs.FirstOrDefault(x => x.AttributeId == attributeId);
                isRequired = pa != null && "Y".Equals(pa.IsRequired?.Trim(), StringComparison.OrdinalIgnoreCase);
            }
            if (string.IsNullOrEmpty(name) && attributeIdByCode.TryGetValue(code, out _))
            {
                name = proAttr?.Name?.Trim() ?? code;
            }
            result.Items.Add(new AttributeRequiredSpecItemDto
            {
                Code = code,
                ParameterName = parameterName,
                Name = name,
                IsRequired = isRequired
            });
        }

        return result;
    }

    private async Task ValidateRequiredProductAttributesUsingRequiredSpecAsync(CreatePolicyDto input)
    {
        await ValidateRequiredProductAttributesUsingRequiredSpecCoreAsync(
            products: input.Products?.Select(p => (p.ProductId, p.Attributes?.Select(a => (a.Code, a.Value, a.AttributeName)).ToList())).ToList(),
            riskObject: input.RiskObject,
            riskMotor: input.RiskObject?.RiskObjectMotor,
            fullInput: input);
    }

    private async Task ValidateRequiredProductAttributesUsingRequiredSpecAsync(UpdatePolicyDetailDto input)
    {
        await ValidateRequiredProductAttributesUsingRequiredSpecCoreAsync(
            products: input.Products?.Select(p => (p.ProductId, p.Attributes?.Select(a => (a.Code, a.Value, a.AttributeName)).ToList())).ToList(),
            riskObject: input.RiskObject,
            riskMotor: input.RiskObject?.RiskObjectMotor,
            fullInput: input);
    }

    private async Task ValidateRequiredProductAttributesUsingRequiredSpecCoreAsync(
        List<(Guid ProductId, List<(string? Code, string? Value, string? Name)>? Attributes)>? products,
        object? riskObject,
        object? riskMotor,
        object fullInput)
    {
        if (products == null || products.Count == 0) return;

        foreach (var (productId, attrsInRequest) in products)
        {
            if (productId == Guid.Empty) continue;
            if (attrsInRequest == null || attrsInRequest.Count == 0) continue;

            var attributeCodes = attrsInRequest
                .Where(a => !string.IsNullOrWhiteSpace(a.Code))
                .Select(a => a.Code!.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (attributeCodes.Count == 0) continue;

            var attrQuery = await ProAttributeRepository.GetQueryableAsync();
            var proAttrs = await AsyncExecuter.ToListAsync(
                attrQuery.Where(a => !a.IsDeleted && attributeCodes.Contains(a.Code)));
            var proAttrByCode = proAttrs.ToDictionary(a => a.Code, a => a, StringComparer.OrdinalIgnoreCase);
            var attributeIdByCode = proAttrs.ToDictionary(a => a.Code, a => a.Id, StringComparer.OrdinalIgnoreCase);

            var paQuery = await ProProductAttributeRepository.GetQueryableAsync();
            var productAttrs = await AsyncExecuter.ToListAsync(
                paQuery.Where(pa => pa.ProductId == productId && !pa.IsDeleted));

            foreach (var code in attributeCodes)
            {
                if (!attributeIdByCode.TryGetValue(code, out var attributeId)) continue;
                var pa = productAttrs.FirstOrDefault(x => x.AttributeId == attributeId);
                var isRequired = pa != null && "Y".Equals(pa.IsRequired?.Trim(), StringComparison.OrdinalIgnoreCase);
                if (!isRequired) continue;

                proAttrByCode.TryGetValue(code, out var proAttr);
                var dataPath = proAttr?.DataPath?.Trim();

                object? raw = null;
                var found = false;

                // Primary: extract using ProAttribute.DataPath (same source as get-attribute-required-spec).
                if (!string.IsNullOrWhiteSpace(dataPath))
                {
                    found = TryGetValueByPath(fullInput, dataPath!, out raw);
                    if (!found && riskObject != null) found = TryGetValueByPath(riskObject, dataPath!, out raw);
                    if (!found && riskMotor != null) found = TryGetValueByPath(riskMotor, dataPath!, out raw);
                }

                // Fallback: legacy behavior - check request product attribute value.
                if (!found)
                {
                    var reqAttr = attrsInRequest.FirstOrDefault(a =>
                        string.Equals(a.Code?.Trim(), code, StringComparison.OrdinalIgnoreCase));
                    raw = reqAttr.Value;
                }

                var value = ConvertToString(raw);
                if (string.IsNullOrWhiteSpace(value))
                {
                    var display = (proAttr?.Name ?? "").Trim();
                    if (string.IsNullOrWhiteSpace(display))
                    {
                        display = (attrsInRequest.FirstOrDefault(a =>
                                string.Equals(a.Code?.Trim(), code, StringComparison.OrdinalIgnoreCase)).Name ?? "")
                            .Trim();
                    }
                    if (string.IsNullOrWhiteSpace(display)) display = code;

                    throw new UserFriendlyException(L["Policy:Policy:RequiredAttributeMissing", display].Value)
                        .WithData("Attribute", display)
                        .WithData("ProductId", productId);
                }
            }
        }
    }

    private static string GetParameterNameFromDataPath(string? dataPath)
    {
        if (string.IsNullOrWhiteSpace(dataPath)) return string.Empty;
        var p = dataPath.Trim();
        if (p.StartsWith("$.", StringComparison.Ordinal)) p = p.Substring(2);
        else if (p.StartsWith("$", StringComparison.Ordinal)) p = p.Substring(1);
        var segments = p.Split('.', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToArray();
        return segments.Length > 0 ? segments[segments.Length - 1] : string.Empty;
    }

    /// <summary>
    /// Same discount/markup aggregation as <see cref="PolicyAmount"/> on create/update (product lines vs policy/amount level).
    /// Used to persist <see cref="PolicyVersion.Discount"/> and <see cref="PolicyVersion.Markup"/> alongside policy_amount.
    /// </summary>
    private static (decimal TotalDiscount, decimal TotalMarkup) ComputeAggregatedDiscountAndMarkupForAmount(
        IEnumerable<(decimal? Discount, decimal? Markup)>? products,
        decimal? amountDiscount,
        decimal? inputDiscount,
        decimal? amountMarkup,
        decimal? versionMarkupFallback)
    {
        var rows = products?.ToList();
        var productDiscountSum = rows?.Sum(r => r.Discount ?? 0) ?? 0;
        var anyProductDiscountSpecified = rows?.Any(r => r.Discount.HasValue) == true;
        var policyLevelDiscount = amountDiscount ?? inputDiscount ?? 0;
        var totalDiscount = !anyProductDiscountSpecified
            ? policyLevelDiscount
            : (productDiscountSum != 0 ? productDiscountSum : policyLevelDiscount);

        var productMarkupSum = rows?.Sum(r => r.Markup ?? 0) ?? 0;
        var anyProductMarkupSpecified = rows?.Any(r => r.Markup.HasValue) == true;
        var policyLevelMarkup = amountMarkup ?? versionMarkupFallback ?? 0;
        var totalMarkup = !anyProductMarkupSpecified
            ? policyLevelMarkup
            : (productMarkupSum != 0 ? productMarkupSum : policyLevelMarkup);

        return (totalDiscount, totalMarkup);
    }

    public override async Task<PagedResultDto<PolicyDto>> GetListAsync(GetPoliciesInput input)
    {
        // List is per-version: one record per PolicyVersion (filtered by policy/version criteria)
        var query = await CreateFilteredVersionQueryAsync(input);

        query = query
            .Include(v => v.Policy)
            .ThenInclude(p => p.Contract)
            .ThenInclude(c => c.Customer)
            .Include(v => v.PolicyProducts)
            .Include(v => v.PolicyRiskObjects)
            .ThenInclude(ro => ro.PolicyRiskMotors)
            .Include(v => v.PolicyCertificates);

        var totalCount = await AsyncExecuter.CountAsync(query);

        query = ApplyVersionSorting(query, input);
        query = query.Skip(input.SkipCount).Take(input.MaxResultCount);

        var versionEntities = await AsyncExecuter.ToListAsync(query);

        // Preload implementer (hr_employee) names and creator (abp_users) usernames in batch (from policies)
        var implementerIds = versionEntities.Select(v => v.Policy.ImplementerId).Distinct().ToList();
        var implementerNameById = new Dictionary<Guid, string>();
        if (implementerIds.Count > 0)
        {
            var empQuery = await HrEmployeeRepository.GetQueryableAsync();
            var empPairs = await AsyncExecuter.ToListAsync(
                empQuery
                    .Where(e => implementerIds.Contains(e.Id))
                    .Select(e => new { e.Id, e.FullName })
            );
            implementerNameById = empPairs
                .GroupBy(x => x.Id)
                .ToDictionary(g => g.Key, g => g.First().FullName ?? string.Empty);
        }

        var creatorIds = versionEntities
            .Select(v => v.Policy.CreatorId)
            .Where(id => id.HasValue && id.Value != Guid.Empty)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        var creatorNameById = new Dictionary<Guid, string>();
        if (creatorIds.Count > 0)
        {
            var userQuery = await UserRepository.GetQueryableAsync();
            var users = await AsyncExecuter.ToListAsync(
                userQuery
                    .Where(u => creatorIds.Contains(u.Id))
                    .Select(u => new { u.Id, u.UserName, u.Name, u.Surname })
            );

            creatorNameById = users
                .GroupBy(x => x.Id)
                .ToDictionary(
                    g => g.Key,
                    g =>
                    {
                        var u = g.First();
                        var fullName = $"{u.Name} {u.Surname}".Trim();
                        return !string.IsNullOrWhiteSpace(fullName)
                            ? fullName
                            : (u.UserName ?? string.Empty);
                    }
                );
        }

        // Preload approver (hr_employee) names in batch (from policy versions)
        var approverIds = versionEntities
            .Select(v => v.ApproverId)
            .Where(id => id.HasValue && id.Value != Guid.Empty)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        var approverNameById = new Dictionary<Guid, string>();
        if (approverIds.Count > 0)
        {
            var approverEmpQuery = await HrEmployeeRepository.GetQueryableAsync();
            var approverPairs = await AsyncExecuter.ToListAsync(
                approverEmpQuery
                    .Where(e => approverIds.Contains(e.Id))
                    .Select(e => new { e.Id, e.FullName })
            );
            approverNameById = approverPairs
                .GroupBy(x => x.Id)
                .ToDictionary(g => g.Key, g => g.First().FullName ?? string.Empty);
        }

        // Preload channel names (res_channel) in batch
        var channelIds = versionEntities
            .Select(v => v.Policy.ChannelId)
            .Where(id => id.HasValue && id.Value != Guid.Empty)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        var channelNameById = new Dictionary<Guid, string>();
        if (channelIds.Count > 0)
        {
            var chQuery = await ResChannelRepository.GetQueryableAsync();
            var chPairs = await AsyncExecuter.ToListAsync(
                chQuery
                    .Where(c => channelIds.Contains(c.Id))
                    .Select(c => new { c.Id, c.Name })
            );
            channelNameById = chPairs
                .GroupBy(x => x.Id)
                .ToDictionary(g => g.Key, g => g.First().Name ?? string.Empty);
        }

        // Preload partner (root insurer) names (res_partner) in batch
        var partnerIds = versionEntities
            .Select(v => v.Policy.PartnerId)
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        var partnerNameById = new Dictionary<Guid, string>();
        if (partnerIds.Count > 0)
        {
            var partnerQuery = await ResPartnerRepository.GetQueryableAsync();
            var partnerPairs = await AsyncExecuter.ToListAsync(
                partnerQuery
                    .Where(p => partnerIds.Contains(p.Id))
                    .Select(p => new { p.Id, p.Name })
            );
            partnerNameById = partnerPairs
                .GroupBy(x => x.Id)
                .ToDictionary(g => g.Key, g => g.First().Name ?? string.Empty);
        }

        // Preload product names (ProProduct.Name) for all products in the loaded versions
        var allProductIds = versionEntities
            .SelectMany(v => v.PolicyProducts.Select(pp => pp.ProductId))
            .Distinct()
            .ToList();

        var productNameById = new Dictionary<Guid, string>();
        if (allProductIds.Count > 0)
        {
            var productQuery = await ProProductRepository.GetQueryableAsync();
            var productPairs = await AsyncExecuter.ToListAsync(
                productQuery
                    .Where(p => allProductIds.Contains(p.Id))
                    .Select(p => new { p.Id, p.Name })
            );

            productNameById = productPairs
                .GroupBy(x => x.Id)
                .ToDictionary(g => g.Key, g => g.First().Name ?? string.Empty);
        }

        // Preload PaymentStatus: gộp tất cả policy_amount.payment_status theo policy_version_id (một version có thể nhiều dòng phí).
        var policyVersionIds = versionEntities.Select(v => v.Id).Distinct().ToList();
        var paymentStatusByVersionId = new Dictionary<Guid, string>();
        if (policyVersionIds.Count > 0)
        {
            var amounts = await PolicyAmountRepository.GetListAsync(x => policyVersionIds.Contains(x.PolicyVersionId) && !x.IsDeleted);
            foreach (var g in amounts.GroupBy(a => a.PolicyVersionId))
            {
                paymentStatusByVersionId[g.Key] = AggregatePaymentStatusForDisplay(g.Select(a => a.PaymentStatus));
            }
        }

        // Preload latest draft payment request per policy (for payment inquiry action on list)
        var policyIds = versionEntities.Select(v => v.Policy.Id).Distinct().ToList();
        var latestPaymentByPolicyId = new Dictionary<Guid, AccountPaymentRequest>();
        if (policyIds.Count > 0)
        {
            var paymentRequests = await AccountPaymentRequestRepository.GetListAsync(x =>
                x.PolicyId.HasValue &&
                policyIds.Contains(x.PolicyId.Value) &&
                x.Status == AccountPaymentRequestStatus.Draft);

            latestPaymentByPolicyId = paymentRequests
                .GroupBy(x => x.PolicyId!.Value)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(x => x.CreationTime).First()
                );
        }

        // For policies in pending approval, get latest work_task (WaitApprove) assignee per policy
        const string policyVersionBusinessName = "policyVersion";
        var pendingVersions = versionEntities
            .Where(v => string.Equals(v.ApprovalStatus, "pending", StringComparison.OrdinalIgnoreCase))
            .Select(v => new { PolicyId = v.Policy.Id, VersionId = v.Id })
            .Distinct()
            .ToList();
        var versionIdToPolicyId = pendingVersions.ToDictionary(x => x.VersionId, x => x.PolicyId);
        var pendingVersionIds = pendingVersions.Select(x => x.VersionId).ToList();
        var assigneeIdByPolicyId = new Dictionary<Guid, Guid>();
        var workTaskIdByPolicyId = new Dictionary<Guid, Guid>();
        if (pendingVersionIds.Count > 0)
        {
            var wtQuery = await _workTaskRepository.GetQueryableAsync();
            var latestWaitApproveByVersion = await AsyncExecuter.ToListAsync(
                wtQuery
                    .Where(t => t.BusinessName == policyVersionBusinessName && pendingVersionIds.Contains(t.BusinessKey) && t.Status == WorkTaskStatus.WaitApprove)
                    .OrderByDescending(t => t.CreationTime)
            );
            foreach (var g in latestWaitApproveByVersion.GroupBy(t => t.BusinessKey))
            {
                var latest = g.First();
                if (!versionIdToPolicyId.TryGetValue(g.Key, out var policyId)) continue;
                workTaskIdByPolicyId[policyId] = latest.Id;
                if (latest.AssigneeId.HasValue && latest.AssigneeId.Value != Guid.Empty)
                    assigneeIdByPolicyId[policyId] = latest.AssigneeId.Value;
            }
        }

        // One PolicyDto per version (row = policy version)
        var items = versionEntities.Select(v =>
        {
            var policy = v.Policy;
            var dto = ObjectMapper.Map<iOne.Policies.Policy, PolicyDto>(policy);

            // This row represents this version - use policy_version.status and premium for display
            dto.LastVersionId = v.Id;
            dto.Version = (int)v.Version;
            dto.Status = ParseVersionStatusToPolicyStatus(v.Status);
            dto.IsRootPolicy = string.Equals(v.Type, "O", StringComparison.OrdinalIgnoreCase);
            dto.IsLatestVersion = v.Id == policy.LastVersionId;
            dto.PaymentStatus = paymentStatusByVersionId.TryGetValue(v.Id, out var ps) ? ps : "new";
            if (latestPaymentByPolicyId.TryGetValue(policy.Id, out var latestPayment))
            {
                dto.PaymentId = latestPayment.Id;
                dto.PaymentProvider = latestPayment.PaymentProvider;
            }
            // Net premium for list/detail: use persisted aggregates on policy_version (not per-product recompute)
            dto.PremiumTotal = PolicyNetPremiumForListDisplay.Compute(v.PremiumTotal, v.Discount, v.Markup);
            dto.OrgEffectDate = v.EffectDate;  // List shows version's effect/expire dates
            dto.OrgExpireDate = v.ExpireDate;
            dto.ApprovalDate = v.ApprovalDate;
            dto.ApproverId = v.ApproverId;
            dto.ApprovalStatus = v.ApprovalStatus; // Chỉ lấy từ policy_version, không fallback Policy (tránh hiển thị sai khi version.approval_status null)
            dto.TerminationStatus = v.TerminationStatus;
            if (string.Equals(v.ApprovalStatus, "pending", StringComparison.OrdinalIgnoreCase))
            {
                if (assigneeIdByPolicyId.TryGetValue(policy.Id, out var assigneeId))
                    dto.CurrentWorkTaskAssigneeId = assigneeId;
                if (workTaskIdByPolicyId.TryGetValue(policy.Id, out var workTaskId))
                    dto.CurrentWorkTaskId = workTaskId;
            }

            dto.ContractNo = policy.Contract?.Code;
            dto.CustomerId = policy.Contract?.CustomerId;
            dto.CustomerName = policy.Contract?.Customer?.Name;

            if (implementerNameById.TryGetValue(policy.ImplementerId, out var impName) && !string.IsNullOrWhiteSpace(impName))
            {
                dto.ImplementerName = impName;
                dto.PolicyIssuerName = impName;
                dto.PolicyIssuerId = policy.ImplementerId;
            }

            if (policy.CreatorId.HasValue &&
                creatorNameById.TryGetValue(policy.CreatorId.Value, out var creatorName) &&
                !string.IsNullOrWhiteSpace(creatorName))
            {
                dto.CreatorName = creatorName;
            }

            if (v.ApproverId.HasValue &&
                approverNameById.TryGetValue(v.ApproverId.Value, out var approverName) &&
                !string.IsNullOrWhiteSpace(approverName))
            {
                dto.ApproverName = approverName;
            }

            if (policy.ChannelId.HasValue &&
                channelNameById.TryGetValue(policy.ChannelId.Value, out var channelName) &&
                !string.IsNullOrWhiteSpace(channelName))
            {
                dto.ChannelName = channelName;
            }

            if (partnerNameById.TryGetValue(policy.PartnerId, out var partnerName) &&
                !string.IsNullOrWhiteSpace(partnerName))
            {
                dto.RootInsuranceName = partnerName;
            }

            var names = v.PolicyProducts
                .Where(pp => !pp.IsDeleted)
                .Select(pp => productNameById.TryGetValue(pp.ProductId, out var name) ? name : string.Empty)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct()
                .ToList();
            dto.ProductName = names.Count > 0 ? string.Join(", ", names) : null;

            var motor = v.PolicyRiskObjects
                .Where(ro => !ro.IsDeleted)
                .SelectMany(ro => ro.PolicyRiskMotors.Where(rm => !rm.IsDeleted))
                .FirstOrDefault();
            dto.VehiclePlate = motor?.CarPlate;
            dto.ChassisNumber = motor?.CarVin;
            dto.EngineNumber = motor?.CarEngineNumber;

            var cert = v.PolicyCertificates?.FirstOrDefault(c => !c.IsDeleted);
            dto.CertificateNo = cert?.CertificateNo;

            return dto;
        }).ToList();

        return new PagedResultDto<PolicyDto>(totalCount, items);
    }

    public virtual async Task<PolicyDto> GetAsync(Guid id, Guid? versionId = null)
    {
        if (CurrentUser.Id == null)
        {
            throw new EntityNotFoundException(typeof(iOne.Policies.Policy), id);
        }

        var viewer = await HrEmployeeRepository.FirstOrDefaultAsync(e => e.UserId == CurrentUser.Id.Value);
        if (viewer == null)
        {
            throw new EntityNotFoundException(typeof(iOne.Policies.Policy), id);
        }

        var accessQuery = await ReadOnlyRepository.GetQueryableAsync();
        accessQuery = accessQuery.Where(p => p.Id == id && !p.IsDeleted);
        var managedSubtree = viewer.PartnerId.HasValue
            ? null
            : await TryGetManagedSubtreeForViewerAsync(viewer);
        LogPolicyEmployeeUnitSearchScope("GetAsync", viewer, managedSubtree);
        accessQuery = PolicySearchQueryScope.ApplyEmployeeUnitSearchFilter(
            accessQuery,
            viewer.Id,
            viewer.PartnerId,
            viewer.DepartmentId,
            viewer.IsManager == true,
            managedSubtree);
        if (!await AsyncExecuter.AnyAsync(accessQuery))
        {
            throw new EntityNotFoundException(typeof(iOne.Policies.Policy), id);
        }

        // Load Policy with all required navigation properties for edit/view modal
        var query = await ReadOnlyRepository.GetQueryableAsync();

        query = query
            .Include(p => p.Contract)
            .ThenInclude(c => c.Customer)
            .Include(p => p.Contract)
            .ThenInclude(c => c.Documents)
            .Include(p => p.PolicyVersions)
            .ThenInclude(v => v.PolicyProducts)
            .ThenInclude(pp => pp.PolicyCoverages)
            .ThenInclude(pc => pc.PolicyCoverageLevels)
            .Include(p => p.PolicyVersions)
            .ThenInclude(v => v.PolicyRiskObjects)
            .ThenInclude(ro => ro.PolicyRiskMotors)
            .Include(p => p.PolicyCertificates)
            .Where(p => p.Id == id && !p.IsDeleted);

        var entity = await AsyncExecuter.FirstOrDefaultAsync(query);
        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(iOne.Policies.Policy), id);
        }

        // When versionId is provided, use that version; otherwise use LastVersionId (current version)
        var currentVersion = versionId.HasValue
            ? entity.PolicyVersions.FirstOrDefault(v => v.Id == versionId.Value && !v.IsDeleted)
            : entity.PolicyVersions.FirstOrDefault(v => v.Id == entity.LastVersionId && !v.IsDeleted);
        if (currentVersion == null && versionId.HasValue)
        {
            throw new EntityNotFoundException(typeof(PolicyVersion), versionId.Value);
        }
        if (currentVersion == null)
        {
            currentVersion = entity.PolicyVersions.OrderByDescending(v => v.Version).FirstOrDefault(v => !v.IsDeleted);
        }

        // Map base Policy fields
        var dto = ObjectMapper.Map<iOne.Policies.Policy, PolicyDto>(entity);
        dto.LastVersionId = currentVersion.Id;
        // Lấy Status và ApprovalStatus từ policy_version (phiên bản đang xem), không từ policy gốc
        dto.Status = ParseVersionStatusToPolicyStatus(currentVersion.Status);
        dto.ApprovalStatus = currentVersion.ApprovalStatus;
        dto.TerminationStatus = currentVersion.TerminationStatus;
        dto.IsLatestVersion = currentVersion.Id == entity.LastVersionId;
        // Net premium on root DTO: policy_version aggregates (same rule as GetList)
        dto.PremiumTotal = PolicyNetPremiumForListDisplay.Compute(
            currentVersion.PremiumTotal,
            currentVersion.Discount,
            currentVersion.Markup);

        // Map nested Contract
        if (entity.Contract != null)
        {
            dto.Contract = new PolicyContractDetailDto
            {
                Id = entity.Contract.Id,
                Type = entity.Contract.Type,
                CustomerId = entity.Contract.CustomerId,
                EffectDate = entity.Contract.EffectDate,
                ExpireDate = entity.Contract.ExpireDate,
                IsReciveInvoice = entity.Contract.IsReciveInvoice,
                LobId = entity.Contract.LobId,
                InsurerId = entity.Contract.InsurerId,
                InsurerContractCode = entity.Contract.InsurerContractCode,
                PayerName = entity.Contract.PayerName,
                PayerEmail = entity.Contract.PayerEmail,
                PayerPhone = entity.Contract.PayerPhone,
                PayerProvinceId = entity.Contract.PayerProvinceId,
                PayerWardId = entity.Contract.PayerWardId,
                PayerAddress = entity.Contract.PayerAddress,
                PayerFullAddress = entity.Contract.PayerFullAddress,
                PayerTaxCode = entity.Contract.PayerTin,
                Description = entity.Contract.Description,
                Quantity = entity.Contract.Quantity,
                CurrentQuantity = entity.Contract.CurrentQuantity,
                EmployeeId = entity.Contract.EmployeeId,
                Code = entity.Contract.Code,
                Name = entity.Contract.Name,
                Documents = entity.Contract.Documents
                    .Where(d => !d.IsDeleted)
                    .Select(d => new PolicyContractDocumentDetailDto
                    {
                        DocumentId = d.DocumentId
                    })
                    .ToList()
            };
        }

        if (currentVersion != null)
        {
            // Map nested Version
            dto.VersionDetail = new PolicyVersionDetailDto
            {
                Version = currentVersion.Version,
                EffectDate = currentVersion.EffectDate,
                ExpireDate = currentVersion.ExpireDate,
                OrgEffectDate = currentVersion.OrgEffectDate,
                OrgExpireDate = currentVersion.OrgExpireDate,
                InternalNote = currentVersion.InternalNote,
                CustomerNote = currentVersion.CustomerNote,
                PremiumTotal = currentVersion.PremiumTotal,
                Premium = currentVersion.Premium,
                Vat = currentVersion.Vat,
                Discount = currentVersion.Discount,
                DiscountRate = currentVersion.DiscountRate,
                Markup = currentVersion.Markup,
                EndorsementType = currentVersion.EndorsementType,
                EndorsementReasonId = currentVersion.EndorsementReasonId,
                EndorsementDescription = currentVersion.EndorsementDescription,
                RefundAmount = currentVersion.RefundAmount
            };

            // Map nested Amount (Option A: derive from version)
            dto.Amount = new PolicyAmountDetailDto
            {
                PremiumTotal = currentVersion.PremiumTotal,
                Premium = currentVersion.Premium,
                Vat = currentVersion.Vat,
                Discount = currentVersion.Discount,
                DiscountRate = currentVersion.DiscountRate,
                Markup = currentVersion.Markup
            };

            // Tổng tăng/giảm phí từ policy_amount (đơn SĐBS đã lưu)
            const string EndorsementAdjustmentFeeItemCode = "ENDORSEMENT_ADJUSTMENT_AMOUNT";
            try
            {
                var endorsementFeeItemId = await GetResFeeItemIdByCodeAsync(EndorsementAdjustmentFeeItemCode);
                var adjustmentRow = await PolicyAmountRepository.FirstOrDefaultAsync(x =>
                    x.PolicyVersionId == currentVersion.Id &&
                    x.FeeItemId == endorsementFeeItemId &&
                    !x.IsDeleted);
                if (adjustmentRow != null)
                {
                    // Signed điều chỉnh: Amount luôn cùng dấu với AmountTotal sau SĐBS; bản ghi cũ (amount_total dương) vẫn đọc Amount.
                    dto.Amount.EndorsementAdjustmentAmount = adjustmentRow.Amount;
                }
            }
            catch
            {
                // ResFeeItem or row may not exist; leave EndorsementAdjustmentAmount null
            }

            // For endorsement (type "A"), build (ProductId, CoverageId) -> previous Premium+Vat so we can show per-coverage tăng/giảm phí on view/update
            Dictionary<(Guid ProductId, Guid CoverageId), decimal>? previousPremiumVatByProductCoverage = null;
            var previousVersion = entity.PolicyVersions.FirstOrDefault(v => !v.IsDeleted && v.Version == currentVersion.Version - 1);
            if (string.Equals(currentVersion.Type, "A", StringComparison.OrdinalIgnoreCase) && previousVersion != null)
            {
                previousPremiumVatByProductCoverage = new Dictionary<(Guid, Guid), decimal>();
                foreach (var pProd in previousVersion.PolicyProducts.Where(x => !x.IsDeleted))
                {
                    foreach (var pCov in pProd.PolicyCoverages.Where(x => !x.IsDeleted))
                    {
                        previousPremiumVatByProductCoverage[(pProd.ProductId, pCov.CoverageId)] = pCov.Premium + pCov.Vat;
                    }
                }
            }

            // Map nested Products + Coverages + CoverageLevels
            dto.Products = currentVersion.PolicyProducts
                .Where(pp => !pp.IsDeleted)
                .Select(pp => new PolicyProductDetailDto
                {
                    Id = pp.Id,
                    ProductId = pp.ProductId,
                    PremiumTotal = pp.PremiumTotal,
                    Premium = pp.Premium,
                    Vat = pp.Vat,
                    Discount = pp.Discount,
                    DiscountRate = pp.DiscountRate,
                    Markup = pp.Markup,
                    AmountLiability = pp.AmountLiability,
                    InsurerProductCode = pp.InsurerProductCode,
                    Coverages = pp.PolicyCoverages
                        .Where(pc => !pc.IsDeleted)
                        .Select(pc => new PolicyCoverageDetailDto
                        {
                            Id = pc.Id,
                            CoverageId = pc.CoverageId,
                            CoverageParentId = pc.CoverageParentId,
                            UomId = pc.UomId,
                            TaxId = pc.TaxId,
                            Quantity = pc.Quantity,
                            PremiumRate = pc.PremiumRate,
                            PremiumTotal = pc.PremiumTotal,
                            Premium = pc.Premium,
                            Vat = pc.Vat,
                            InsurerCoverageCode = pc.InsurerCoverageCode,
                            TableRateLineId = pc.TableRateLineId,
                            AmountLiability = pc.AmountLiability,
                            NetRate = pc.NetRate,
                            BaseRate = pc.BaseRate,
                            FlatRate = pc.FlatRate,
                            Loading = pc.Loading,
                            Discount = pc.Discount,
                            DiscountRate = pc.DiscountRate,
                            PremiumChange = previousPremiumVatByProductCoverage != null
                                ? (decimal?)((pc.Premium + pc.Vat) - previousPremiumVatByProductCoverage.GetValueOrDefault((pp.ProductId, pc.CoverageId), 0m))
                                : null,
                            CoverageLevels = pc.PolicyCoverageLevels
                                .Where(pcl => !pcl.IsDeleted)
                                .Select(pcl => new PolicyCoverageLevelDetailDto
                                {
                                    Id = pcl.Id,
                                    CoverageLevelTypeId = pcl.CoverageLevelTypeId,
                                    CoverageLevelBasisId = pcl.CoverageLevelBasisId,
                                    AmountType = pcl.AmountType,
                                    FromAmount = pcl.FromAmount,
                                    ToAmount = pcl.ToAmount,
                                    ConditionScript = pcl.ConditionScript,
                                    ComputeScript = pcl.ComputeScript
                                })
                                .ToList()
                        })
                        .ToList()
                })
                .ToList();

            // Map nested RiskObject + RiskMotor (pick first non-deleted)
            var riskObject = currentVersion.PolicyRiskObjects
                .Where(ro => !ro.IsDeleted)
                .FirstOrDefault();

            if (riskObject != null)
            {
                var riskMotor = riskObject.PolicyRiskMotors
                    .Where(rm => !rm.IsDeleted)
                    .FirstOrDefault();

                dto.RiskObject = new PolicyRiskObjectDetailDto
                {
                    Id = riskObject.Id,
                    ObjectTypeId = riskObject.ObjectTypeId,
                    RepName = riskObject.RepName,
                    RepIdNo = riskObject.RepIdNo,
                    RepPassport = riskObject.RepPassport,
                    RepPhone = riskObject.RepPhone,
                    RepEmail = riskObject.RepEmail,
                    RepProvinceId = riskObject.RepProvinceId,
                    RepWardId = riskObject.RepWardId,
                    RepAddress = riskObject.RepAddress,
                    RepFullAddress = riskObject.RepFullAddress,
                    RiskObjectProvinceId = riskObject.RiskObjectProvinceId,
                    RiskObjectWardId = riskObject.RiskObjectWardId,
                    RiskObjectAddress = riskObject.RiskObjectAddress,
                    RiskObjectFullAddress = riskObject.RiskObjectFullAddress,
                    RiskObjectLat = riskObject.RiskObjectLat,
                    RiskObjectLong = riskObject.RiskObjectLong,
                    RiskObjectMotor = riskMotor != null ? await MapRiskMotorDetailDtoAsync(riskMotor) : null
                };

                // Map RiskObject documents (policy_risk_object_document)
                var roDocs =
                    await PolicyRiskObjectDocumentRepository.GetListAsync(x => x.PolicyRiskObjectId == riskObject.Id);
                if (roDocs.Count > 0)
                {
                    dto.RiskObject.Documents = roDocs
                        .Select(x => new PolicyRiskObjectDocumentDetailDto { DocumentId = x.DocumentId })
                        .ToList();
                }
            }
        }

        // Map Documents (policy_document table)
        var docQuery = await PolicyDocumentRepository.GetQueryableAsync();
        var policyDocuments = await AsyncExecuter.ToListAsync(
            docQuery.Where(pd => pd.PolicyId == entity.Id && !pd.IsDeleted)
        );

        if (policyDocuments.Count > 0)
        {
            dto.Documents = policyDocuments
                .Select(pd => new PolicyDocumentDetailDto
                {
                    DocumentId = pd.DocumentId
                })
                .ToList();
        }

        // Số Giấy chứng nhận from PolicyCertificate.certificate_no
        var certificateForVersion = entity.PolicyCertificates?
            .FirstOrDefault(c => !c.IsDeleted && currentVersion != null && c.PolicyVersionId == currentVersion.Id);
        if (certificateForVersion != null)
        {
            dto.CertificateNo = certificateForVersion.CertificateNo;
        }

        // PaymentStatus: gộp tất cả policy_amount.payment_status của phiên bản hiện tại (LastVersionId).
        var currentVersionAmounts = await PolicyAmountRepository.GetListAsync(x =>
            x.PolicyVersionId == entity.LastVersionId && !x.IsDeleted);
        dto.PaymentStatus = currentVersionAmounts.Count == 0
            ? "new"
            : AggregatePaymentStatusForDisplay(currentVersionAmounts.Select(a => a.PaymentStatus));

        return dto;
    }

    public override async Task<PolicyDto> CreateAsync(CreatePolicyDto input)
    {
        // Generate policy number on server when not provided (create mode from FE omits it)
        var normalizedPolicyNo = input.PolicyNo?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedPolicyNo))
        {
            normalizedPolicyNo = await GetNextPolicyNoForRenewalAsync();
        }

        // Validate policy number uniqueness
        if (await PolicyRepository.IsPolicyNoExistsAsync(normalizedPolicyNo))
        {
            throw new BusinessException("Policy:Policy:PolicyNoExists")
                .WithData("PolicyNo", normalizedPolicyNo);
        }

        // Pair (InsurerPolicyNo, CertificateNo) must be unique: only error when both match an existing non-cancelled policy
        if (await CheckInsurerPolicyNoAndCertificateNoPairExistsAsync(input.InsurerPolicyNo, input.CertificateNo, excludePolicyId: null))
        {
            throw new BusinessException("Policy:Policy:InsurerPolicyNoAndCertificateNoPairExists")
                .WithData("InsurerPolicyNo", input.InsurerPolicyNo!.Trim())
                .WithData("CertificateNo", input.CertificateNo!.Trim());
        }

        // Validate OrgExpireDate > OrgEffectDate
        if (input.OrgExpireDate < input.OrgEffectDate)
        {
            throw new BusinessException("Policy:Policy:OrgExpireDateMustBeAfterOrgEffectDate");
        }

        var effectivePremiumTotal = input.Amount?.PremiumTotal ?? input.PremiumTotal;
        if (effectivePremiumTotal <= 0)
        {
            throw new BusinessException("Policy:Policy:PremiumTotalMustBePositive");
        }

        // If nested objects are provided, Version is required (because children link to PolicyVersionId)
        var hasNestedChildren =
            (input.Products != null && input.Products.Count > 0) ||
            (input.Products != null && input.Products.Any(p => p.Coverages != null && p.Coverages.Count > 0)) ||
            input.RiskObject != null;

        if (hasNestedChildren && input.Version == null)
        {
            throw new BusinessException("Policy:Policy:PolicyVersionRequired");
        }

        // Validate required product attributes using the same logic as get-attribute-required-spec
        await ValidateRequiredProductAttributesUsingRequiredSpecAsync(input);

        // VCX: main coverage amountLiability must not exceed car value (RiskObjectValue)
        await ValidateVcxMainAmountLiabilityNotExceedingCarValueAsync(input.Products, input.RiskObject?.RiskObjectMotor?.RiskObjectValue);

        ValidateCreateRiskMotorPlateEngineVin(input.RiskObject?.RiskObjectMotor);

        // Create contract if provided as nested object
        var contractId = input.ContractId;
        if (!contractId.HasValue && input.Contract != null)
        {
            var contractDto = input.Contract;

            // If FE does not send contractId + contractCode, generate contractCode from ResSequence(CONTRACT_CODE_SEQ)
            var contractCode = contractDto.Code?.Trim();
            if (string.IsNullOrWhiteSpace(contractCode))
            {
                contractCode = await GetNextSequenceCodeAsync("CONTRACT_CODE_SEQ");
            }

            // Fallbacks when FE omits these fields (backend can refine generation later)
            var contractName = string.IsNullOrWhiteSpace(contractDto.Name) ? contractCode : contractDto.Name!.Trim();
            var contractQuantity = contractDto.Quantity ?? 1m;
            // Contract EmployeeId = Người khai thác (Seller) from policy; fallback to DTO if policy has no seller
            Guid? contractEmployeeId = input.SellerId != Guid.Empty ? input.SellerId : contractDto.EmployeeId;

            var contractEntity = new PolicyContract(
                GuidGenerator.Create(),
                contractCode!,
                contractName!,
                contractDto.Type,
                contractDto.CustomerId,
                contractDto.EffectDate,
                contractQuantity,
                // Default contract status on create (do not accept from client for policy create flow)
                PolicyContractStatus.Active,
                contractDto.InsurerId,
                contractDto.InsurerContractCode,
                contractDto.LobId ?? input.LobId,
                contractDto.PayerName,
                contractDto.PayerEmail,
                contractDto.PayerPhone,
                contractDto.PayerProvinceId,
                contractDto.PayerWardId,
                contractDto.PayerAddress,
                contractDto.PayerFullAddress,
                contractDto.PayerTaxCode?.Trim(),
                contractDto.Description,
                contractDto.ExpireDate,
                contractDto.CurrentQuantity,
                contractEmployeeId,
                null,
                null,
                null,
                null,
                null,
                null,
                contractDto.IsReciveInvoice
            );

            await ContractManager.CreateAsync(contractEntity);
            contractId = contractEntity.Id;
        }

        // Validate: hợp đồng lẻ (Individual) chỉ được gắn tối đa 1 policy
        // Chỉ validate khi user chọn contract có sẵn từ FE; khi tạo mới (input.Contract) thì bỏ qua
        if (input.ContractId.HasValue)
        {
            await ValidateIndividualContractSinglePolicyAsync(input.ContractId.Value, excludePolicyId: null);
        }

        // Handle contract documents (for both new and existing contracts)
        if (contractId.HasValue && input.Contract?.Documents != null && input.Contract.Documents.Count > 0)
        {
            foreach (var docDto in input.Contract.Documents)
            {
                // For existing contracts, check if already linked to avoid duplicates
                var exists = await PolicyContractDocumentRepository.AnyAsync(x =>
                    x.PolicyContractId == contractId.Value && x.DocumentId == docDto.DocumentId);

                if (!exists)
                {
                    var contractDoc = new PolicyContractDocument(
                        GuidGenerator.Create(),
                        contractId,
                        docDto.DocumentId
                    );
                    await PolicyContractDocumentManager.CreateAsync(contractDoc);
                }
            }
        }

        // Amount override (optional)
        var amount = input.Amount;
        var premiumTotal = amount?.PremiumTotal ?? input.PremiumTotal;
        var premium = amount?.Premium ?? input.Premium;
        var vat = amount?.Vat ?? input.Vat;
        var discount = amount?.Discount ?? input.Discount;
        var discountRate = amount?.DiscountRate ?? input.DiscountRate;

        // Determine policy version id (used as LastVersionId)
        // If caller already provided LastVersionId, we reuse it; otherwise generate.
        var policyVersionId = input.LastVersionId != Guid.Empty ? input.LastVersionId : GuidGenerator.Create();

        // Set issueDate to current system time
        var issueDate = Clock.Now;

        var entity = new iOne.Policies.Policy(
            GuidGenerator.Create(),
            input.LobId,
            normalizedPolicyNo!,
            policyVersionId,
            input.SellType,
            input.PolicyTypeId,
            input.PartnerId,
            input.SellerId,
            input.ImplementerId,
            input.CurrencyId,
            input.ExchangeRate,
            input.Status,
            input.OrgEffectDate,
            input.OrgExpireDate,
            premiumTotal,
            premium,
            vat,
            contractId,
            input.InsurerPolicyNo,
            issueDate,
            input.ApprovalStatus,
            input.CancellationDate,
            input.TerminationDate,
            input.CancellationReasonId,
            input.TerminationReasonId,
            input.IsRenewal,
            input.IsGift,
            discount,
            discountRate,
            input.IsBankLoan,
            input.ChannelId,
            input.LotImportCode,
            input.InsuredName,
            input.InsuredIdNo,
            input.InsuredTin,
            input.InsuredPassport,
            input.InsuredPhone,
            input.InsuredEmail,
            input.InsuredProvinceId,
            input.InsuredWardId,
            input.InsuredAddress,
            input.InsuredFullAddress,
            input.InsuredOrgType,
            input.BeneficiaryName,
            input.BeneficiaryIdNo,
            input.BeneficiaryTin,
            input.BeneficiaryPassport,
            input.BeneficiaryPhone,
            input.BeneficiaryEmail,
            input.BeneficiaryProvinceId,
            input.BeneficiaryWardId,
            input.BeneficiaryAddress,
            input.BeneficiaryFullAddress,
            input.BeneficiaryOrgType
        );

        await Manager.CreateAsync(entity);
        // Create policy documents (policy_document) from res_document ids (does NOT require version)
        if (input.Documents != null && input.Documents.Count > 0)
        {
            var docIds = input.Documents
                .Select(d => d.DocumentId)
                .Where(id => id.HasValue && id.Value != Guid.Empty)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();

            if (docIds.Count > 0)
            {
                var docQuery = await ResDocumentRepository.GetQueryableAsync();
                var existingDocIds = await AsyncExecuter.ToListAsync(
                    docQuery.Where(d => docIds.Contains(d.Id)).Select(d => d.Id)
                );

                var missing = docIds.Except(existingDocIds).ToList();
                if (missing.Count > 0)
                {
                    throw new BusinessException("Policy:Policy:ResDocumentNotFound")
                        .WithData("DocumentIds", string.Join(",", missing));
                }

                foreach (var docId in docIds)
                {
                    var policyDocument = new PolicyDocument(
                        GuidGenerator.Create(),
                        entity.Id,
                        docId
                    );
                    await PolicyDocumentManager.CreateAsync(policyDocument);
                }
            }
        }

        // Create policy version + children if provided
        if (input.Version != null)
        {
            var policyId = entity.Id;
            var versionDto = input.Version;

            // Create PolicyVersion
            var versionEntity = new PolicyVersion(
                policyVersionId,
                versionDto.Version,
                policyId,
                // Default values for initial create
                "O", // Origin (original)
                "draft", // Draft
                versionDto.EffectDate,
                versionDto.ExpireDate,
                versionDto.OrgEffectDate,
                versionDto.OrgExpireDate,
                versionDto.PremiumTotal,
                versionDto.Premium,
                versionDto.Vat,
                versionDto.InternalNote,
                versionDto.CustomerNote,
                versionDto.Discount,
                versionDto.DiscountRate,
                versionDto.Markup
            );
            await PolicyVersionManager.CreateAsync(versionEntity);
            // Số Giấy chứng nhận → PolicyCertificate.certificate_no
            if (!string.IsNullOrWhiteSpace(input.CertificateNo))
            {
                var certificate = new PolicyCertificate(
                    GuidGenerator.Create(),
                    entity.Id,
                    policyVersionId,
                    certificateNo: input.CertificateNo.Trim(),
                    url: null
                );
                await PolicyCertificateRepository.InsertAsync(certificate);
            }

            // Create PolicyProducts
            var productIdToPolicyProductId = new Dictionary<Guid, Guid>();
            if (input.Products != null)
            {
                foreach (var productDto in input.Products)
                {
                    var policyProductId = GuidGenerator.Create();
                    var policyProduct = new PolicyProduct(
                        policyProductId,
                        policyVersionId,
                        productDto.ProductId,
                        productDto.PremiumTotal,
                        productDto.Premium,
                        productDto.Vat,
                        productDto.InsurerProductCode,
                        productDto.AmountLiability,
                        productDto.Discount,
                        productDto.DiscountRate,
                        productDto.Markup
                    );
                    await PolicyProductManager.CreateAsync(policyProduct);
                    productIdToPolicyProductId[productDto.ProductId] = policyProductId;

                    // Create PolicyCoverages + CoverageLevels under this product
                    if (productDto.Coverages != null)
                    {
                        foreach (var coverageDto in productDto.Coverages)
                        {
                            var policyCoverageId = GuidGenerator.Create();
                            var policyCoverage = new PolicyCoverage(
                                policyCoverageId,
                                policyProductId,
                                coverageDto.CoverageId,
                                coverageDto.UomId,
                                coverageDto.Quantity,
                                coverageDto.TaxId,
                                coverageDto.PremiumRate,
                                coverageDto.PremiumTotal,
                                coverageDto.Premium,
                                coverageDto.Vat,
                                coverageDto.CoverageParentId,
                                coverageDto.InsurerCoverageCode,
                                coverageDto.TableRateLineId,
                                coverageDto.AmountLiability,
                                coverageDto.NetRate,
                                coverageDto.BaseRate,
                                coverageDto.FlatRate,
                                coverageDto.Loading,
                                coverageDto.Discount,
                                coverageDto.DiscountRate
                            );
                            await PolicyCoverageManager.CreateAsync(policyCoverage);
                            if (coverageDto.CoverageLevels != null)
                            {
                                foreach (var levelDto in coverageDto.CoverageLevels)
                                {
                                    var levelEntity = new PolicyCoverageLevel(
                                        GuidGenerator.Create(),
                                        policyCoverageId,
                                        levelDto.CoverageLevelTypeId,
                                        levelDto.CoverageLevelBasisId,
                                        levelDto.AmountType,
                                        levelDto.FromAmount,
                                        levelDto.ToAmount,
                                        levelDto.ConditionScript,
                                        levelDto.ComputeScript
                                    );
                                    await PolicyCoverageLevelManager.CreateAsync(levelEntity);
                                }
                            }
                        }
                    }
                }
            }

            // Create PolicyRiskObject + RiskMotor (optional)
            if (input.RiskObject != null)
            {
                var roDto = input.RiskObject;
                var policyRiskObjectId = GuidGenerator.Create();
                var riskObjectEntity = new PolicyRiskObject(
                    policyRiskObjectId,
                    policyId,
                    policyVersionId,
                    roDto.ObjectTypeId,
                    roDto.RepName,
                    roDto.RepIdNo,
                    roDto.RepPassport,
                    roDto.RepPhone,
                    roDto.RepEmail,
                    roDto.RepProvinceId,
                    roDto.RepWardId,
                    roDto.RepAddress,
                    roDto.RepFullAddress,
                    roDto.RiskObjectProvinceId,
                    roDto.RiskObjectWardId,
                    roDto.RiskObjectAddress,
                    roDto.RiskObjectFullAddress,
                    roDto.RiskObjectLat,
                    roDto.RiskObjectLong
                );
                await PolicyRiskObjectManager.CreateAsync(riskObjectEntity);
                // Create PolicyRiskObjectDocument links (policy_risk_object_document)
                if (roDto.Documents != null)
                {
                    var docIds = roDto.Documents
                        .Select(d => d.DocumentId)
                        .Where(id => id.HasValue && id.Value != Guid.Empty)
                        .Select(id => id!.Value)
                        .Distinct()
                        .ToList();

                    if (docIds.Count > 0)
                    {
                        var docQuery = await ResDocumentRepository.GetQueryableAsync();
                        var existingDocIds = await AsyncExecuter.ToListAsync(
                            docQuery.Where(d => docIds.Contains(d.Id)).Select(d => d.Id)
                        );

                        foreach (var docId in existingDocIds.Distinct())
                        {
                            await PolicyRiskObjectDocumentRepository.InsertAsync(
                                new PolicyRiskObjectDocument(policyRiskObjectId, docId)
                            );
                        }
                    }
                }

                if (roDto.RiskObjectMotor != null)
                {
                    var m = roDto.RiskObjectMotor;
                    ValidateCarProductionYear(m.CarProductionYear);
                    var motorEntity = new PolicyRiskMotor(
                        GuidGenerator.Create(),
                        policyRiskObjectId,
                        m.RiskObjectValue,
                        m.MotorClassCode,
                        m.CarLineCode,
                        m.CarGroupCode,
                        m.CarTypeCode,
                        m.CarBrandCode,
                        m.CarModelCode,
                        m.CarCategoryCode,
                        m.CarUsage,
                        m.CarOld,
                        m.CarProductionYear,
                        m.CarPlate,
                        m.CarPlateType,
                        m.CarPlateClear,
                        m.CarSeatNumber,
                        m.CarVin,
                        m.CarEngineNumber,
                        m.CarPayloadCapacity,
                        m.CarColor,
                        m.CarOrigin,
                        m.CarNew
                    );
                    await PolicyRiskMotorManager.CreateAsync(motorEntity);
                }
            }

            // PolicyAmount (policy_amount): cùng một lần SaveChanges với policy_version — tránh flush tách đoạn khiến bản ghi amount không commit (import/FE dùng chung CreateAsync).
            // feeItemId từ ResFeeItem.Code = 'POLICY_AMOUNT'.
            var policyAmountFeeItemId = await GetResFeeItemIdByCodeAsync(PolicyAmountFeeItemCode);

            // PolicyAmount: totalAmount = totalPremium - totalDiscount + totalMarkup; Amount (excl. VAT) = totalAmount - totalVat
            var amountDto = input.Amount;
            var totalPremium = amountDto?.PremiumTotal ?? premiumTotal;
            var totalVat = amountDto?.Vat ?? vat;
            var (totalDiscount, totalMarkup) = ComputeAggregatedDiscountAndMarkupForAmount(
                input.Products?.Select(p => (p.Discount, p.Markup)),
                amountDto?.Discount,
                input.Discount,
                amountDto?.Markup,
                input.Version?.Markup);
            versionEntity.UpdateDiscount(totalDiscount);
            versionEntity.UpdateMarkup(totalMarkup);
            var amountTotalToSave = Math.Max(0, totalPremium - totalDiscount + totalMarkup);
            var amountToSave = Math.Max(0, amountTotalToSave - totalVat);
            var vatToSave = totalVat;

            var policyAmount = new PolicyAmount(
                GuidGenerator.Create(),
                entity.Id,
                policyVersionId,
                policyAmountFeeItemId,
                issueDate,
                amountTotalToSave,
                amountToSave,
                vatToSave // Default payment status
            );
            await PolicyAmountManager.CreateAsync(policyAmount);

            await CurrentUnitOfWork.SaveChangesAsync();
        }

        return ObjectMapper.Map<iOne.Policies.Policy, PolicyDto>(entity);
    }

    public override async Task<PolicyDto> UpdateAsync(Guid id, UpdatePolicyDetailDto input)
    {
        // Load Policy with current version + nested graph for upsert/soft-delete
        var query = await Repository.GetQueryableAsync();
        query = query
            .Include(p => p.Contract)
            .Include(p => p.PolicyVersions)
            .ThenInclude(v => v.PolicyProducts)
            .ThenInclude(pp => pp.PolicyCoverages)
            .ThenInclude(pc => pc.PolicyCoverageLevels)
            .Include(p => p.PolicyVersions)
            .ThenInclude(v => v.PolicyRiskObjects)
            .ThenInclude(ro => ro.PolicyRiskMotors)
            .Where(p => p.Id == id && !p.IsDeleted);

        var entity = await AsyncExecuter.FirstOrDefaultAsync(query);
        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(iOne.Policies.Policy), id);
        }

        // Enforce immutability: PolicyNo / LastVersionId / Version number (ContractId is now allowed on update)
        if (input.OrgExpireDate < input.OrgEffectDate)
        {
            throw new BusinessException("Policy:Policy:OrgExpireDateMustBeAfterOrgEffectDate");
        }

        var updateEffectivePremiumTotal = input.Amount?.PremiumTotal ?? input.PremiumTotal;
        if (updateEffectivePremiumTotal <= 0)
        {
            throw new BusinessException("Policy:Policy:PremiumTotalMustBePositive");
        }

        var currentVersion = entity.PolicyVersions
            .FirstOrDefault(v => v.Id == entity.LastVersionId && !v.IsDeleted);

        if (currentVersion == null)
        {
            throw new BusinessException("Policy:Policy:CurrentVersionNotFound")
                .WithData("PolicyId", entity.Id)
                .WithData("LastVersionId", entity.LastVersionId);
        }

        var isNewVersion = false;

        // Pair (InsurerPolicyNo, CertificateNo) must be unique: only error when both match another non-cancelled policy
        if (await CheckInsurerPolicyNoAndCertificateNoPairExistsAsync(input.InsurerPolicyNo, input.CertificateNo, excludePolicyId: id))
        {
            throw new BusinessException("Policy:Policy:InsurerPolicyNoAndCertificateNoPairExists")
                .WithData("InsurerPolicyNo", input.InsurerPolicyNo!.Trim())
                .WithData("CertificateNo", input.CertificateNo!.Trim());
        }

        // Allow changing the policy's contract when user selects another contract on edit
        var contractSwitched = input.ContractId.HasValue && input.ContractId.Value != entity.ContractId;
        if (contractSwitched)
        {
            // Validate: hợp đồng lẻ (Individual) chỉ được gắn tối đa 1 policy
            await ValidateIndividualContractSinglePolicyAsync(input.ContractId!.Value, excludePolicyId: id);
            entity.UpdateContractId(input.ContractId!.Value);
        }

        // Validate required product attributes using the same logic as get-attribute-required-spec
        await ValidateRequiredProductAttributesUsingRequiredSpecAsync(input);

        // VCX: main coverage amountLiability must not exceed car value (RiskObjectValue)
        await ValidateVcxMainAmountLiabilityNotExceedingCarValueAsync(input.Products, input.RiskObject?.RiskObjectMotor?.RiskObjectValue);

        // Update Contract details only when not switching (otherwise we would update the old contract with form data)
        if (input.Contract != null && entity.Contract != null && !contractSwitched)
        {
            var c = input.Contract;

            // Allow editing contractNo (Code) and contractType (Type) in policy edit mode.
            // WARNING: This updates the underlying PolicyContract entity, which may be shared by multiple policies.

            if (c.Code != null)
            {
                entity.Contract.UpdateCode(c.Code);
            }

            if (c.Type.HasValue)
            {
                entity.Contract.UpdateType(c.Type.Value);
            }

            if (c.Name != null)
            {
                entity.Contract.UpdateName(c.Name);
            }

            if (c.InsurerId.HasValue)
            {
                entity.Contract.UpdateInsurerId(c.InsurerId);
            }

            if (c.InsurerContractCode != null)
            {
                entity.Contract.UpdateInsurerContractCode(c.InsurerContractCode);
            }

            if (c.LobId.HasValue)
            {
                entity.Contract.UpdateLobId(c.LobId);
            }

            if (c.CustomerId.HasValue)
            {
                entity.Contract.UpdateCustomerId(c.CustomerId.Value);
            }

            entity.Contract.UpdatePayerInfo(
                c.PayerName,
                c.PayerEmail,
                c.PayerPhone,
                c.PayerProvinceId,
                c.PayerWardId,
                c.PayerAddress,
                c.PayerFullAddress
            );

            if (c.PayerTaxCode != null)
            {
                entity.Contract.UpdatePayerTin(c.PayerTaxCode);
            }

            if (c.Description != null)
            {
                entity.Contract.UpdateDescription(c.Description);
            }

            if (c.EffectDate.HasValue)
            {
                entity.Contract.UpdateEffectDate(c.EffectDate.Value);
            }

            if (c.ExpireDate.HasValue)
            {
                entity.Contract.UpdateExpireDate(c.ExpireDate);
            }

            if (c.Quantity.HasValue)
            {
                entity.Contract.UpdateQuantity(c.Quantity.Value);
            }

            if (c.CurrentQuantity.HasValue)
            {
                entity.Contract.UpdateCurrentQuantity(c.CurrentQuantity);
            }

            // Contract EmployeeId = Người khai thác (Seller) from policy; prefer input.SellerId, else DTO
            Guid? contractEmployeeId = input.SellerId != Guid.Empty ? input.SellerId : c.EmployeeId;
            if (contractEmployeeId.HasValue)
            {
                entity.Contract.UpdateEmployeeId(contractEmployeeId.Value);
            }

            if (c.IsReciveInvoice != null)
            {
                entity.Contract.UpdateIsReciveInvoice(c.IsReciveInvoice);
            }

            await ContractManager.UpdateAsync(entity.Contract);

            // Sync contract documents
            if (c.Documents != null)
            {
                var desiredDocIds = c.Documents
                    .Select(d => d.DocumentId)
                    .Distinct()
                    .ToHashSet();

                var existingDocs =
                    await PolicyContractDocumentRepository.GetListAsync(x => x.PolicyContractId == entity.Contract.Id);
                var existingDocIds = existingDocs
                    .Where(d => d.DocumentId.HasValue)
                    .Select(d => d.DocumentId!.Value)
                    .ToHashSet();

                // Add new
                foreach (var docId in desiredDocIds)
                {
                    if (!existingDocIds.Contains(docId))
                    {
                        var contractDoc = new PolicyContractDocument(
                            GuidGenerator.Create(),
                            entity.Contract.Id,
                            docId
                        );
                        await PolicyContractDocumentManager.CreateAsync(contractDoc);
                    }
                }

                // Delete removed
                foreach (var doc in existingDocs)
                {
                    if (doc.DocumentId.HasValue && !desiredDocIds.Contains(doc.DocumentId.Value))
                    {
                        await PolicyContractDocumentRepository.DeleteAsync(doc);
                    }
                }
            }
        }

        ApplyPolicyBaseFieldsForUpdate(entity, input);

        // Sync latest policy_version.status when policy status changes (excl. endorsement)
        currentVersion.UpdateStatus(PolicyStatusToVersionStatusString(entity.Status));
        await PolicyVersionRepository.UpdateAsync(currentVersion);

        // Số Giấy chứng nhận → PolicyCertificate.certificate_no
        var certificatesForVersion = await PolicyCertificateRepository.GetListAsync(x =>
            x.PolicyId == id && x.PolicyVersionId == currentVersion.Id && !x.IsDeleted);
        var existingCert = certificatesForVersion.FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(input.CertificateNo))
        {
            var certNo = input.CertificateNo.Trim();
            if (existingCert != null)
            {
                existingCert.UpdateCertificateNo(certNo);
                await PolicyCertificateRepository.UpdateAsync(existingCert);
            }
            else
            {
                var newCert = new PolicyCertificate(
                    GuidGenerator.Create(),
                    id,
                    currentVersion.Id,
                    certificateNo: certNo,
                    url: null
                );
                await PolicyCertificateRepository.InsertAsync(newCert);
            }
        }
        else if (existingCert != null)
        {
            existingCert.UpdateCertificateNo(null);
            await PolicyCertificateRepository.UpdateAsync(existingCert);
        }

        entity.UpdateInsuredName(input.InsuredName);
        entity.UpdateInsuredIdNo(input.InsuredIdNo);
        entity.UpdateInsuredTin(input.InsuredTin);
        entity.UpdateInsuredPassport(input.InsuredPassport);
        entity.UpdateInsuredPhone(input.InsuredPhone);
        entity.UpdateInsuredEmail(input.InsuredEmail);
        entity.UpdateInsuredProvinceId(input.InsuredProvinceId);
        entity.UpdateInsuredWardId(input.InsuredWardId);
        entity.UpdateInsuredAddress(input.InsuredAddress);
        entity.UpdateInsuredFullAddress(input.InsuredFullAddress);
        entity.UpdateInsuredOrgType(input.InsuredOrgType);

        entity.UpdateBeneficiaryName(input.BeneficiaryName);
        entity.UpdateBeneficiaryIdNo(input.BeneficiaryIdNo);
        entity.UpdateBeneficiaryTin(input.BeneficiaryTin);
        entity.UpdateBeneficiaryPassport(input.BeneficiaryPassport);
        entity.UpdateBeneficiaryPhone(input.BeneficiaryPhone);
        entity.UpdateBeneficiaryEmail(input.BeneficiaryEmail);
        entity.UpdateBeneficiaryProvinceId(input.BeneficiaryProvinceId);
        entity.UpdateBeneficiaryWardId(input.BeneficiaryWardId);
        entity.UpdateBeneficiaryAddress(input.BeneficiaryAddress);
        entity.UpdateBeneficiaryFullAddress(input.BeneficiaryFullAddress);
        entity.UpdateBeneficiaryOrgType(input.BeneficiaryOrgType);

        // Update current version fields (do NOT change Version/Type/Status)
        if (input.Version != null)
        {
            var v = input.Version;
            if (v.EffectDate.HasValue)
            {
                currentVersion.UpdateEffectDate(v.EffectDate.Value);
            }

            if (v.ExpireDate.HasValue)
            {
                currentVersion.UpdateExpireDate(v.ExpireDate.Value);
            }

            if (v.OrgEffectDate.HasValue)
            {
                currentVersion.UpdateOrgEffectDate(v.OrgEffectDate.Value);
            }

            if (v.OrgExpireDate.HasValue)
            {
                currentVersion.UpdateOrgExpireDate(v.OrgExpireDate.Value);
            }

            if (v.InternalNote != null)
            {
                currentVersion.UpdateInternalNote(v.InternalNote);
            }

            if (v.CustomerNote != null)
            {
                currentVersion.UpdateCustomerNote(v.CustomerNote);
            }

            if (v.PremiumTotal.HasValue)
            {
                currentVersion.UpdatePremiumTotal(v.PremiumTotal.Value);
            }

            if (v.Premium.HasValue)
            {
                currentVersion.UpdatePremium(v.Premium.Value);
            }

            if (v.Vat.HasValue)
            {
                currentVersion.UpdateVat(v.Vat.Value);
            }

            if (v.Discount.HasValue)
            {
                currentVersion.UpdateDiscount(v.Discount);
            }

            if (v.DiscountRate.HasValue)
            {
                currentVersion.UpdateDiscountRate(v.DiscountRate);
            }

            if (v.Markup.HasValue)
            {
                currentVersion.UpdateMarkup(v.Markup);
            }

            if (v.EndorsementType.HasValue)
            {
                currentVersion.UpdateEndorsementType(v.EndorsementType);
            }

            if (v.EndorsementReasonId.HasValue)
            {
                currentVersion.UpdateEndorsementReasonId(v.EndorsementReasonId);
            }

            if (v.EndorsementDescription != null)
            {
                currentVersion.UpdateEndorsementDescription(v.EndorsementDescription);
            }
        }
        else
        {
            // Keep version dates in sync with policy org dates by default
            currentVersion.UpdateOrgEffectDate(input.OrgEffectDate);
            currentVersion.UpdateOrgExpireDate(input.OrgExpireDate);
        }

        // Upsert Products/Coverages/CoverageLevels (if provided)
        if (input.Products != null)
        {
            // ID-based upsert: match by policy_product.id
            var desiredProducts = input.Products
                .GroupBy(p => p.Id ?? Guid.Empty)
                .Select(g => g.First())
                .ToList();

            var existingProducts = currentVersion.PolicyProducts.Where(x => !x.IsDeleted).ToList();
            var existingById = existingProducts.ToDictionary(x => x.Id, x => x);
            var desiredExistingIds = desiredProducts.Where(p => p.Id.HasValue).Select(p => p.Id!.Value).ToHashSet();

            foreach (var p in desiredProducts)
            {
                PolicyProduct? productEntity = null;
                if (p.Id.HasValue)
                {
                    existingById.TryGetValue(p.Id.Value, out productEntity);
                    if (productEntity == null && !isNewVersion)
                    {
                        throw new BusinessException("Policy:PolicyProduct:NotFound")
                            .WithData("PolicyProductId", p.Id.Value)
                            .WithData("PolicyId", entity.Id);
                    }
                }

                if (productEntity == null)
                {
                    productEntity = new PolicyProduct(
                        GuidGenerator.Create(),
                        currentVersion.Id,
                        p.ProductId,
                        p.PremiumTotal,
                        p.Premium,
                        p.Vat,
                        p.InsurerProductCode,
                        p.AmountLiability,
                        p.Discount,
                        p.DiscountRate,
                        p.Markup
                    );
                    await PolicyProductManager.CreateAsync(productEntity);
                    currentVersion.PolicyProducts.Add(productEntity);
                    existingById[productEntity.Id] = productEntity;
                }
                else
                {
                    await PolicyProductManager.UpdateAsync(
                        productEntity,
                        p.InsurerProductCode,
                        p.AmountLiability,
                        p.PremiumTotal,
                        p.Premium,
                        p.Vat,
                        p.Discount,
                        p.DiscountRate,
                        p.Markup
                    );
                }

                // Coverages for this product
                if (p.Coverages != null)
                {
                    // ID-based upsert: match by policy_coverage.id
                    var desiredCoverages = p.Coverages
                        // If client doesn't send Id, dedupe by CoverageId (otherwise everything collapses to Guid.Empty)
                        .GroupBy(c => c.Id ?? c.CoverageId)
                        .Select(g => g.First())
                        .ToList();

                    var existingCoverages = productEntity.PolicyCoverages.Where(x => !x.IsDeleted).ToList();
                    var existingCoverageById = existingCoverages.ToDictionary(x => x.Id, x => x);
                    var existingCoverageByCoverageId = existingCoverages
                        .GroupBy(x => x.CoverageId)
                        .ToDictionary(g => g.Key, g => g.First());
                    var desiredExistingCoverageIds = desiredCoverages
                        .Where(c => c.Id.HasValue)
                        .Select(c => c.Id!.Value)
                        .ToHashSet();

                    foreach (var c in desiredCoverages)
                    {
                        PolicyCoverage? coverageEntity = null;
                        if (c.Id.HasValue)
                        {
                            existingCoverageById.TryGetValue(c.Id.Value, out coverageEntity);
                            if (coverageEntity == null && !isNewVersion)
                            {
                                throw new BusinessException("Policy:PolicyCoverage:NotFound")
                                    .WithData("PolicyCoverageId", c.Id.Value)
                                    .WithData("PolicyId", entity.Id);
                            }
                        }
                        else
                        {
                            // Fallback: match existing row by CoverageId when Id isn't provided
                            existingCoverageByCoverageId.TryGetValue(c.CoverageId, out coverageEntity);
                        }

                        if (coverageEntity == null)
                        {
                            coverageEntity = new PolicyCoverage(
                                GuidGenerator.Create(),
                                productEntity.Id,
                                c.CoverageId,
                                c.UomId,
                                c.Quantity,
                                c.TaxId,
                                c.PremiumRate,
                                c.PremiumTotal,
                                c.Premium,
                                c.Vat,
                                c.CoverageParentId,
                                c.InsurerCoverageCode,
                                c.TableRateLineId,
                                c.AmountLiability,
                                c.NetRate,
                                c.BaseRate,
                                c.FlatRate,
                                c.Loading,
                                c.Discount,
                                c.DiscountRate
                            );
                            await PolicyCoverageManager.CreateAsync(coverageEntity);
                            productEntity.PolicyCoverages.Add(coverageEntity);
                            existingCoverageById[coverageEntity.Id] = coverageEntity;
                            existingCoverageByCoverageId[coverageEntity.CoverageId] = coverageEntity;
                        }
                        else
                        {
                            await PolicyCoverageManager.UpdateAsync(
                                coverageEntity,
                                c.CoverageParentId,
                                c.InsurerCoverageCode,
                                c.TableRateLineId,
                                c.AmountLiability,
                                c.Quantity,
                                c.NetRate,
                                c.BaseRate,
                                c.FlatRate,
                                c.Loading,
                                c.PremiumRate,
                                c.PremiumTotal,
                                c.Premium,
                                c.Vat,
                                c.Discount,
                                c.DiscountRate
                            );
                        }

                        // Track this coverage as "kept" even if client didn't send Id
                        desiredExistingCoverageIds.Add(coverageEntity.Id);

                        // Coverage Levels (match by TypeId + BasisId)
                        if (c.CoverageLevels != null)
                        {
                            // ID-based upsert: match by policy_coverage_level.id
                            var desiredLevels = c.CoverageLevels
                                // If client doesn't send Id, dedupe by TypeId+BasisId (deductible selection)
                                .GroupBy(l => l.Id?.ToString() ?? $"{l.CoverageLevelTypeId:N}:{l.CoverageLevelBasisId:N}")
                                .Select(g => g.First())
                                .ToList();

                            var existingLevels = coverageEntity.PolicyCoverageLevels.Where(x => !x.IsDeleted).ToList();
                            var existingLevelById = existingLevels.ToDictionary(x => x.Id, x => x);
                            var existingLevelByTypeBasis = existingLevels
                                .GroupBy(x => $"{x.CoverageLevelTypeId:N}:{x.CoverageLevelBasisId:N}")
                                .ToDictionary(g => g.Key, g => g.First());
                            var desiredExistingLevelIds = desiredLevels
                                .Where(l => l.Id.HasValue)
                                .Select(l => l.Id!.Value)
                                .ToHashSet();

                            foreach (var l in desiredLevels)
                            {
                                PolicyCoverageLevel? levelEntity = null;
                                if (l.Id.HasValue)
                                {
                                    existingLevelById.TryGetValue(l.Id.Value, out levelEntity);
                                    if (levelEntity == null && !isNewVersion)
                                    {
                                        throw new BusinessException("Policy:PolicyCoverageLevel:NotFound")
                                            .WithData("PolicyCoverageLevelId", l.Id.Value)
                                            .WithData("PolicyId", entity.Id);
                                    }
                                }
                                else
                                {
                                    // Fallback: match by TypeId+BasisId when Id isn't provided
                                    var k = $"{l.CoverageLevelTypeId:N}:{l.CoverageLevelBasisId:N}";
                                    existingLevelByTypeBasis.TryGetValue(k, out levelEntity);
                                }

                                if (levelEntity == null)
                                {
                                    levelEntity = new PolicyCoverageLevel(
                                        GuidGenerator.Create(),
                                        coverageEntity.Id,
                                        l.CoverageLevelTypeId,
                                        l.CoverageLevelBasisId,
                                        l.AmountType,
                                        l.FromAmount,
                                        l.ToAmount,
                                        l.ConditionScript,
                                        l.ComputeScript
                                    );
                                    await PolicyCoverageLevelManager.CreateAsync(levelEntity);
                                    coverageEntity.PolicyCoverageLevels.Add(levelEntity);
                                    existingLevelById[levelEntity.Id] = levelEntity;
                                    existingLevelByTypeBasis[$"{levelEntity.CoverageLevelTypeId:N}:{levelEntity.CoverageLevelBasisId:N}"] = levelEntity;
                                }
                                else
                                {
                                    await PolicyCoverageLevelManager.UpdateAsync(
                                        levelEntity,
                                        l.CoverageLevelTypeId,
                                        l.CoverageLevelBasisId,
                                        l.ConditionScript,
                                        l.ComputeScript,
                                        l.AmountType,
                                        l.FromAmount,
                                        l.ToAmount
                                    );
                                }

                                // Track this level as "kept" even if client didn't send Id
                                desiredExistingLevelIds.Add(levelEntity.Id);
                            }

                            // Soft-delete removed levels
                            foreach (var existing in existingLevels)
                            {
                                if (!desiredExistingLevelIds.Contains(existing.Id))
                                {
                                    await PolicyCoverageLevelRepository.DeleteAsync(existing);
                                }
                            }
                        }
                    }

                    // Soft-delete removed coverages
                    foreach (var existing in existingCoverages)
                    {
                        if (!desiredExistingCoverageIds.Contains(existing.Id))
                        {
                            await PolicyCoverageRepository.DeleteAsync(existing);
                        }
                    }
                }
            }

            // Soft-delete removed products
            foreach (var existing in existingProducts)
            {
                if (!desiredExistingIds.Contains(existing.Id))
                {
                    await PolicyProductRepository.DeleteAsync(existing);
                }
            }
        }

        // Upsert RiskObject + RiskMotor (single object model)
        if (input.RiskObject != null)
        {
            PolicyRiskObject? riskObjectEntity = null;
            var existingRiskObjects = currentVersion.PolicyRiskObjects.Where(x => !x.IsDeleted).ToList();
            var existingRiskObjectById = existingRiskObjects.ToDictionary(x => x.Id, x => x);

            if (input.RiskObject.Id.HasValue)
            {
                existingRiskObjectById.TryGetValue(input.RiskObject.Id.Value, out riskObjectEntity);
                if (riskObjectEntity == null && !isNewVersion)
                {
                    throw new BusinessException("Policy:PolicyRiskObject:NotFound")
                        .WithData("PolicyRiskObjectId", input.RiskObject.Id.Value)
                        .WithData("PolicyId", entity.Id);
                }
            }
            else
            {
                // Backward compatibility: if no id sent, pick the first
                riskObjectEntity = existingRiskObjects.FirstOrDefault();
            }

            if (riskObjectEntity == null)
            {
                riskObjectEntity = new PolicyRiskObject(
                    GuidGenerator.Create(),
                    entity.Id,
                    currentVersion.Id,
                    input.RiskObject.ObjectTypeId,
                    input.RiskObject.RepName,
                    input.RiskObject.RepIdNo,
                    input.RiskObject.RepPassport,
                    input.RiskObject.RepPhone,
                    input.RiskObject.RepEmail,
                    input.RiskObject.RepProvinceId,
                    input.RiskObject.RepWardId,
                    input.RiskObject.RepAddress,
                    input.RiskObject.RepFullAddress,
                    input.RiskObject.RiskObjectProvinceId,
                    input.RiskObject.RiskObjectWardId,
                    input.RiskObject.RiskObjectAddress,
                    input.RiskObject.RiskObjectFullAddress,
                    input.RiskObject.RiskObjectLat,
                    input.RiskObject.RiskObjectLong
                );
                await PolicyRiskObjectManager.CreateAsync(riskObjectEntity);
                currentVersion.PolicyRiskObjects.Add(riskObjectEntity);
            }
            else
            {
                // ObjectTypeId is immutable in domain entity
                await PolicyRiskObjectManager.UpdateAsync(
                    riskObjectEntity,
                    input.RiskObject.RepName,
                    input.RiskObject.RepIdNo,
                    input.RiskObject.RepPassport,
                    input.RiskObject.RepPhone,
                    input.RiskObject.RepEmail,
                    input.RiskObject.RepProvinceId,
                    input.RiskObject.RepWardId,
                    input.RiskObject.RepAddress,
                    input.RiskObject.RepFullAddress,
                    input.RiskObject.RiskObjectProvinceId,
                    input.RiskObject.RiskObjectWardId,
                    input.RiskObject.RiskObjectAddress,
                    input.RiskObject.RiskObjectFullAddress,
                    input.RiskObject.RiskObjectLat,
                    input.RiskObject.RiskObjectLong
                );
            }

            // Sync PolicyRiskObjectDocument links (policy_risk_object_document)
            if (input.RiskObject.Documents != null)
            {
                var desiredDocIds = input.RiskObject.Documents
                    .Select(d => d.DocumentId)
                    .Where(id => id != Guid.Empty)
                    .Distinct()
                    .ToList();

                // Filter to existing ResDocument ids to avoid FK errors
                if (desiredDocIds.Count > 0)
                {
                    var docQuery = await ResDocumentRepository.GetQueryableAsync();
                    desiredDocIds = await AsyncExecuter.ToListAsync(
                        docQuery.Where(d => desiredDocIds.Contains(d.Id)).Select(d => d.Id)
                    );
                }

                var desiredSet = desiredDocIds.ToHashSet();

                var existingLinks =
                    await PolicyRiskObjectDocumentRepository.GetListAsync(x =>
                        x.PolicyRiskObjectId == riskObjectEntity.Id
                    );

                var existingDocIdSet = existingLinks.Select(x => x.DocumentId).ToHashSet();

                // Add new links
                foreach (var docId in desiredSet)
                {
                    if (!existingDocIdSet.Contains(docId))
                    {
                        await PolicyRiskObjectDocumentRepository.InsertAsync(
                            new PolicyRiskObjectDocument(riskObjectEntity.Id, docId)
                        );
                    }
                }

                // Remove missing links
                foreach (var link in existingLinks)
                {
                    if (!desiredSet.Contains(link.DocumentId))
                    {
                        await PolicyRiskObjectDocumentRepository.DeleteAsync(link);
                    }
                }
            }

            // Risk motor (single record)
            if (input.RiskObject.RiskObjectMotor != null)
            {
                var m = input.RiskObject.RiskObjectMotor;
                ValidateCarProductionYear(m.CarProductionYear);

                PolicyRiskMotor? motorEntity = null;
                var existingMotors = riskObjectEntity.PolicyRiskMotors.Where(x => !x.IsDeleted).ToList();
                var existingMotorById = existingMotors.ToDictionary(x => x.Id, x => x);

                if (m.Id.HasValue)
                {
                    existingMotorById.TryGetValue(m.Id.Value, out motorEntity);
                    if (motorEntity == null && !isNewVersion)
                    {
                        throw new BusinessException("Policy:PolicyRiskMotor:NotFound")
                            .WithData("PolicyRiskMotorId", m.Id.Value)
                            .WithData("PolicyId", entity.Id);
                    }
                }
                else
                {
                    motorEntity = existingMotors.FirstOrDefault();
                }

                if (motorEntity == null)
                {
                    motorEntity = new PolicyRiskMotor(
                        GuidGenerator.Create(),
                        riskObjectEntity.Id,
                        m.RiskObjectValue,
                        m.MotorClassCode,
                        m.CarLineCode,
                        m.CarGroupCode,
                        m.CarTypeCode,
                        m.CarBrandCode,
                        m.CarModelCode,
                        m.CarCategoryCode,
                        m.CarUsage,
                        m.CarOld,
                        m.CarProductionYear,
                        m.CarPlate,
                        m.CarPlateType,
                        m.CarPlateClear,
                        m.CarSeatNumber,
                        m.CarVin,
                        m.CarEngineNumber,
                        m.CarPayloadCapacity,
                        m.CarColor,
                        m.CarOrigin,
                        m.CarNew
                    );
                    await PolicyRiskMotorManager.CreateAsync(motorEntity);
                    riskObjectEntity.PolicyRiskMotors.Add(motorEntity);
                }
                else
                {
                    await PolicyRiskMotorManager.UpdateAsync(
                        motorEntity,
                        riskObjectEntity.Id,
                        m.RiskObjectValue,
                        m.MotorClassCode,
                        m.CarLineCode,
                        m.CarGroupCode,
                        m.CarTypeCode,
                        m.CarBrandCode,
                        m.CarModelCode,
                        m.CarCategoryCode,
                        m.CarUsage,
                        m.CarOld,
                        m.CarProductionYear,
                        m.CarPlate,
                        m.CarPlateType,
                        m.CarPlateClear,
                        m.CarSeatNumber,
                        m.CarVin,
                        m.CarEngineNumber,
                        m.CarPayloadCapacity,
                        m.CarColor,
                        m.CarOrigin,
                        m.CarNew
                    );
                }
            }
        }

        // Upsert Documents (policy_document)
        if (input.Documents != null)
        {
            var desiredDocumentIds = input.Documents
                .Select(d => d.DocumentId)
                .Distinct()
                .ToHashSet();

            var existingDocs = await PolicyDocumentRepository.GetListAsync(x => x.PolicyId == entity.Id);
            var existingDocIds = existingDocs
                .Where(d => d.DocumentId.HasValue)
                .Select(d => d.DocumentId!.Value)
                .ToHashSet();

            // Add new
            foreach (var docId in desiredDocumentIds)
            {
                if (!existingDocIds.Contains(docId))
                {
                    var docEntity = new PolicyDocument(GuidGenerator.Create(), entity.Id, docId);
                    await PolicyDocumentManager.CreateAsync(docEntity);
                }
            }

            // Soft-delete removed
            foreach (var doc in existingDocs)
            {
                if (doc.DocumentId.HasValue && !desiredDocumentIds.Contains(doc.DocumentId.Value))
                {
                    await PolicyDocumentRepository.DeleteAsync(doc);
                }
            }
        }

        // Upsert PolicyAmount (policy_amount) for current version.
        // feeItemId is resolved from ResFeeItem.Code = 'POLICY_AMOUNT' (do not trust FE for this field).
        var policyAmountFeeItemIdForUpdate = await GetResFeeItemIdByCodeAsync(PolicyAmountFeeItemCode);

        // PolicyAmount: totalAmount = totalPremium - totalDiscount + totalMarkup, totalVat = totalVat, totalPremium (Amount) = totalAmount - totalVat
        var amountInput = input.Amount;
        var totalPremiumUpdate = amountInput?.PremiumTotal ?? input.PremiumTotal;
        var totalVatUpdate = amountInput?.Vat ?? input.Vat;
        var (totalDiscountUpdate, totalMarkupUpdate) = ComputeAggregatedDiscountAndMarkupForAmount(
            input.Products?.Select(p => (p.Discount, p.Markup)),
            amountInput?.Discount,
            input.Discount,
            amountInput?.Markup,
            input.Version?.Markup);
        currentVersion.UpdateDiscount(totalDiscountUpdate);
        currentVersion.UpdateMarkup(totalMarkupUpdate);
        var amountTotalUpdate = Math.Max(0, totalPremiumUpdate - totalDiscountUpdate + totalMarkupUpdate);
        var amountUpdate = Math.Max(0, amountTotalUpdate - totalVatUpdate);
        var vatUpdate = totalVatUpdate;

        var existingAmountsForFeeItem = await PolicyAmountRepository.GetListAsync(x =>
            x.PolicyId == entity.Id &&
            x.PolicyVersionId == currentVersion.Id &&
            x.FeeItemId == policyAmountFeeItemIdForUpdate);
        var existingAmountForFeeItem = existingAmountsForFeeItem.FirstOrDefault();

        var amountIssueDate = input.IssueDate ?? Clock.Now;
        if (existingAmountForFeeItem == null)
        {
            var amountEntity = new PolicyAmount(
                GuidGenerator.Create(),
                entity.Id,
                currentVersion.Id,
                policyAmountFeeItemIdForUpdate,
                amountIssueDate,
                amountTotalUpdate,
                amountUpdate,
                vatUpdate
            );
            await PolicyAmountManager.CreateAsync(amountEntity);
        }
        else
        {
            await PolicyAmountManager.UpdateAsync(
                existingAmountForFeeItem,
                amountIssueDate,
                amountTotalUpdate,
                amountUpdate,
                vatUpdate,
                existingAmountForFeeItem.PaymentStatus,
                existingAmountForFeeItem.PaymentMethodId,
                existingAmountForFeeItem.PaymentDate
            );
        }

        await Manager.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();

        // Return detail after update
        return await GetAsync(entity.Id);
    }

    public virtual async Task<PolicyDto> EndorsementAsync(Guid id, UpdatePolicyDetailDto input)
    {
        // Load Policy with current version + nested graph for upsert/soft-delete
        var query = await Repository.GetQueryableAsync();
        query = query
            .Include(p => p.Contract)
            .Include(p => p.PolicyVersions)
            .ThenInclude(v => v.PolicyProducts)
            .ThenInclude(pp => pp.PolicyCoverages)
            .ThenInclude(pc => pc.PolicyCoverageLevels)
            .Include(p => p.PolicyVersions)
            .ThenInclude(v => v.PolicyRiskObjects)
            .ThenInclude(ro => ro.PolicyRiskMotors)
            .Where(p => p.Id == id && !p.IsDeleted);

        var entity = await AsyncExecuter.FirstOrDefaultAsync(query);
        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(iOne.Policies.Policy), id);
        }

        // Enforce immutability: PolicyNo / LastVersionId / Version number (ContractId is now allowed on update)
        if (input.OrgExpireDate < input.OrgEffectDate)
        {
            throw new BusinessException("Policy:Policy:OrgExpireDateMustBeAfterOrgEffectDate");
        }

        var endorsementEffectivePremiumTotal = input.Version?.PremiumTotal ?? input.PremiumTotal;
        if (endorsementEffectivePremiumTotal <= 0)
        {
            throw new BusinessException("Policy:Policy:PremiumTotalMustBePositive");
        }

        var targetVersionId = input.VersionId ?? entity.LastVersionId;
        var currentVersion = entity.PolicyVersions
            .FirstOrDefault(v => v.Id == targetVersionId && !v.IsDeleted);

        if (currentVersion == null)
        {
            throw new BusinessException("Policy:Policy:CurrentVersionNotFound")
                .WithData("PolicyId", entity.Id)
                .WithData("LastVersionId", targetVersionId);
        }

        // Endorsement type branching:
        // - POLICY_COMMON_INFOR_ENDORSEMENT = luôn update in place, không sinh mới (dù đơn gốc hay SĐBS đã duyệt);
        // - Đơn SĐBS chưa duyệt (type=A, status!=active) = update in place;
        // - Đơn SĐBS đã duyệt hoặc đơn gốc + type khác common info = tạo policy_version mới.
        var endorsementTypeCode = input.Version?.EndorsementTypeCode?.Trim();
        var isCommonInforEndorsement = string.Equals(endorsementTypeCode, "POLICY_COMMON_INFOR_ENDORSEMENT", StringComparison.OrdinalIgnoreCase);
        var isEffectDateEndorsement = string.Equals(endorsementTypeCode, "POLICY_EFFECT_DATE_ENDORSEMENT", StringComparison.OrdinalIgnoreCase);
        var isAllInforEndorsement = string.Equals(endorsementTypeCode, "POLICY_ALL_INFOR_ENDORSEMENT", StringComparison.OrdinalIgnoreCase);
        var shouldUpdatePreviousVersionExpiry = isEffectDateEndorsement || isAllInforEndorsement;
        var isNewVersion = false;
        var currentVersionIsUnapprovedEndorsement = string.Equals(currentVersion.Type, "A", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(currentVersion.Status, "active", StringComparison.OrdinalIgnoreCase);
        var shouldUpdateInPlace = isCommonInforEndorsement || currentVersionIsUnapprovedEndorsement;

        if (!shouldUpdateInPlace)
        {
            var previousVersion = currentVersion;
            var v = input.Version;
            var newVersionNumber = currentVersion.Version + 1;
            var newVersionId = GuidGenerator.Create();
            var effectDate = v?.EffectDate ?? currentVersion.EffectDate;
            var expireDate = v?.ExpireDate ?? currentVersion.ExpireDate;
            // Org dates stick to policy origin: always clone from previous version, never from input
            var orgEffectDate = previousVersion.OrgEffectDate;
            var orgExpireDate = previousVersion.OrgExpireDate;
            var premiumTotal = v?.PremiumTotal ?? input.PremiumTotal;
            var premium = v?.Premium ?? input.Premium;
            var vat = v?.Vat ?? input.Vat;
            var endorsementApprovalStatus = input.SubmitForApproval ? "pending" : null;
            var newVersion = new PolicyVersion(
                newVersionId,
                newVersionNumber,
                entity.Id,
                "A",
                "draft", // New version from endorsement starts as Draft
                effectDate,
                expireDate,
                orgEffectDate,
                orgExpireDate,
                premiumTotal,
                premium,
                vat,
                v?.InternalNote ?? currentVersion.InternalNote,
                v?.CustomerNote ?? currentVersion.CustomerNote,
                v?.Discount ?? currentVersion.Discount,
                v?.DiscountRate ?? currentVersion.DiscountRate,
                v?.Markup ?? currentVersion.Markup,
                null, // ApprovalDate - new version not yet approved
                null, // ApproverId - new version not yet approved
                endorsementApprovalStatus, // null = draft (save draft), "pending" = submitted for approval
                currentVersion.InsurerIntegrationStatus,
                currentVersion.InsurerIntegrationDescription,
                v?.EndorsementType ?? currentVersion.EndorsementType,
                v?.EndorsementReasonId ?? currentVersion.EndorsementReasonId,
                v?.EndorsementDescription ?? currentVersion.EndorsementDescription
            );
            await PolicyVersionManager.CreateAsync(newVersion);
            entity.UpdateLastVersionId(newVersionId);
            entity.PolicyVersions.Add(newVersion);

            currentVersion = newVersion;
            isNewVersion = true;
        }

        // Pair (InsurerPolicyNo, CertificateNo) must be unique: only error when both match another non-cancelled policy
        if (await CheckInsurerPolicyNoAndCertificateNoPairExistsAsync(input.InsurerPolicyNo, input.CertificateNo, excludePolicyId: id))
        {
            throw new BusinessException("Policy:Policy:InsurerPolicyNoAndCertificateNoPairExists")
                .WithData("InsurerPolicyNo", input.InsurerPolicyNo!.Trim())
                .WithData("CertificateNo", input.CertificateNo!.Trim());
        }

        // Allow changing the policy's contract when user selects another contract on edit
        var contractSwitched = input.ContractId.HasValue && input.ContractId.Value != entity.ContractId;
        if (contractSwitched)
        {
            // Validate: hợp đồng lẻ (Individual) chỉ được gắn tối đa 1 policy
            await ValidateIndividualContractSinglePolicyAsync(input.ContractId!.Value, excludePolicyId: id);
            entity.UpdateContractId(input.ContractId!.Value);
        }

        // Validate required product attributes using the same logic as get-attribute-required-spec
        await ValidateRequiredProductAttributesUsingRequiredSpecAsync(input);

        // VCX: main coverage amountLiability must not exceed car value (RiskObjectValue)
        await ValidateVcxMainAmountLiabilityNotExceedingCarValueAsync(input.Products, input.RiskObject?.RiskObjectMotor?.RiskObjectValue);

        // Update Contract details only when not switching (otherwise we would update the old contract with form data)
        if (input.Contract != null && entity.Contract != null && !contractSwitched)
        {
            var c = input.Contract;

            // Allow editing contractNo (Code) and contractType (Type) in policy edit mode.
            // WARNING: This updates the underlying PolicyContract entity, which may be shared by multiple policies.

            if (c.Code != null)
            {
                entity.Contract.UpdateCode(c.Code);
            }

            if (c.Type.HasValue)
            {
                entity.Contract.UpdateType(c.Type.Value);
            }

            if (c.Name != null)
            {
                entity.Contract.UpdateName(c.Name);
            }

            if (c.InsurerId.HasValue)
            {
                entity.Contract.UpdateInsurerId(c.InsurerId);
            }

            if (c.InsurerContractCode != null)
            {
                entity.Contract.UpdateInsurerContractCode(c.InsurerContractCode);
            }

            if (c.LobId.HasValue)
            {
                entity.Contract.UpdateLobId(c.LobId);
            }

            if (c.CustomerId.HasValue)
            {
                entity.Contract.UpdateCustomerId(c.CustomerId.Value);
            }

            entity.Contract.UpdatePayerInfo(
                c.PayerName,
                c.PayerEmail,
                c.PayerPhone,
                c.PayerProvinceId,
                c.PayerWardId,
                c.PayerAddress,
                c.PayerFullAddress
            );

            if (c.PayerTaxCode != null)
            {
                entity.Contract.UpdatePayerTin(c.PayerTaxCode);
            }

            if (c.Description != null)
            {
                entity.Contract.UpdateDescription(c.Description);
            }

            if (c.EffectDate.HasValue)
            {
                entity.Contract.UpdateEffectDate(c.EffectDate.Value);
            }

            if (c.ExpireDate.HasValue)
            {
                entity.Contract.UpdateExpireDate(c.ExpireDate);
            }

            if (c.Quantity.HasValue)
            {
                entity.Contract.UpdateQuantity(c.Quantity.Value);
            }

            if (c.CurrentQuantity.HasValue)
            {
                entity.Contract.UpdateCurrentQuantity(c.CurrentQuantity);
            }

            // Contract EmployeeId = Người khai thác (Seller) from policy; prefer input.SellerId, else DTO
            Guid? contractEmployeeId = input.SellerId != Guid.Empty ? input.SellerId : c.EmployeeId;
            if (contractEmployeeId.HasValue)
            {
                entity.Contract.UpdateEmployeeId(contractEmployeeId.Value);
            }

            if (c.IsReciveInvoice != null)
            {
                entity.Contract.UpdateIsReciveInvoice(c.IsReciveInvoice);
            }

            await ContractManager.UpdateAsync(entity.Contract);

            // Sync contract documents
            if (c.Documents != null)
            {
                var desiredDocIds = c.Documents
                    .Select(d => d.DocumentId)
                    .Distinct()
                    .ToHashSet();

                var existingDocs =
                    await PolicyContractDocumentRepository.GetListAsync(x => x.PolicyContractId == entity.Contract.Id);
                var existingDocIds = existingDocs
                    .Where(d => d.DocumentId.HasValue)
                    .Select(d => d.DocumentId!.Value)
                    .ToHashSet();

                // Add new
                foreach (var docId in desiredDocIds)
                {
                    if (!existingDocIds.Contains(docId))
                    {
                        var contractDoc = new PolicyContractDocument(
                            GuidGenerator.Create(),
                            entity.Contract.Id,
                            docId
                        );
                        await PolicyContractDocumentManager.CreateAsync(contractDoc);
                    }
                }

                // Delete removed
                foreach (var doc in existingDocs)
                {
                    if (doc.DocumentId.HasValue && !desiredDocIds.Contains(doc.DocumentId.Value))
                    {
                        await PolicyContractDocumentRepository.DeleteAsync(doc);
                    }
                }
            }
        }

        ApplyPolicyBaseFieldsForEndorsement(entity, input);

        // Số Giấy chứng nhận → PolicyCertificate.certificate_no
        var certificatesForVersion = await PolicyCertificateRepository.GetListAsync(x =>
            x.PolicyId == id && x.PolicyVersionId == currentVersion.Id && !x.IsDeleted);
        var existingCert = certificatesForVersion.FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(input.CertificateNo))
        {
            var certNo = input.CertificateNo.Trim();
            if (existingCert != null)
            {
                existingCert.UpdateCertificateNo(certNo);
                await PolicyCertificateRepository.UpdateAsync(existingCert);
            }
            else
            {
                var newCert = new PolicyCertificate(
                    GuidGenerator.Create(),
                    id,
                    currentVersion.Id,
                    certificateNo: certNo,
                    url: null
                );
                await PolicyCertificateRepository.InsertAsync(newCert);
            }
        }
        else if (existingCert != null)
        {
            existingCert.UpdateCertificateNo(null);
            await PolicyCertificateRepository.UpdateAsync(existingCert);
        }

        entity.UpdateInsuredName(input.InsuredName);
        entity.UpdateInsuredIdNo(input.InsuredIdNo);
        entity.UpdateInsuredTin(input.InsuredTin);
        entity.UpdateInsuredPassport(input.InsuredPassport);
        entity.UpdateInsuredPhone(input.InsuredPhone);
        entity.UpdateInsuredEmail(input.InsuredEmail);
        entity.UpdateInsuredProvinceId(input.InsuredProvinceId);
        entity.UpdateInsuredWardId(input.InsuredWardId);
        entity.UpdateInsuredAddress(input.InsuredAddress);
        entity.UpdateInsuredFullAddress(input.InsuredFullAddress);
        entity.UpdateInsuredOrgType(input.InsuredOrgType);

        entity.UpdateBeneficiaryName(input.BeneficiaryName);
        entity.UpdateBeneficiaryIdNo(input.BeneficiaryIdNo);
        entity.UpdateBeneficiaryTin(input.BeneficiaryTin);
        entity.UpdateBeneficiaryPassport(input.BeneficiaryPassport);
        entity.UpdateBeneficiaryPhone(input.BeneficiaryPhone);
        entity.UpdateBeneficiaryEmail(input.BeneficiaryEmail);
        entity.UpdateBeneficiaryProvinceId(input.BeneficiaryProvinceId);
        entity.UpdateBeneficiaryWardId(input.BeneficiaryWardId);
        entity.UpdateBeneficiaryAddress(input.BeneficiaryAddress);
        entity.UpdateBeneficiaryFullAddress(input.BeneficiaryFullAddress);
        entity.UpdateBeneficiaryOrgType(input.BeneficiaryOrgType);

        // Update current version fields (do NOT change Version/Type/Status)
        if (input.Version != null)
        {
            var v = input.Version;
            if (v.EffectDate.HasValue)
            {
                currentVersion.UpdateEffectDate(v.EffectDate.Value);
            }

            if (v.ExpireDate.HasValue)
            {
                currentVersion.UpdateExpireDate(v.ExpireDate.Value);
            }

            if (v.OrgEffectDate.HasValue)
            {
                currentVersion.UpdateOrgEffectDate(v.OrgEffectDate.Value);
            }

            if (v.OrgExpireDate.HasValue)
            {
                currentVersion.UpdateOrgExpireDate(v.OrgExpireDate.Value);
            }

            if (v.InternalNote != null)
            {
                currentVersion.UpdateInternalNote(v.InternalNote);
            }

            if (v.CustomerNote != null)
            {
                currentVersion.UpdateCustomerNote(v.CustomerNote);
            }

            if (v.PremiumTotal.HasValue)
            {
                currentVersion.UpdatePremiumTotal(v.PremiumTotal.Value);
            }

            if (v.Premium.HasValue)
            {
                currentVersion.UpdatePremium(v.Premium.Value);
            }

            if (v.Vat.HasValue)
            {
                currentVersion.UpdateVat(v.Vat.Value);
            }

            if (v.Discount.HasValue)
            {
                currentVersion.UpdateDiscount(v.Discount);
            }

            if (v.DiscountRate.HasValue)
            {
                currentVersion.UpdateDiscountRate(v.DiscountRate);
            }

            if (v.Markup.HasValue)
            {
                currentVersion.UpdateMarkup(v.Markup);
            }

            if (v.EndorsementType.HasValue)
            {
                currentVersion.UpdateEndorsementType(v.EndorsementType);
            }

            if (v.EndorsementReasonId.HasValue)
            {
                currentVersion.UpdateEndorsementReasonId(v.EndorsementReasonId);
            }

            if (v.EndorsementDescription != null)
            {
                currentVersion.UpdateEndorsementDescription(v.EndorsementDescription);
            }

            if (shouldUpdateInPlace)
            {
                currentVersion.UpdateApprovalStatus(input.SubmitForApproval ? "pending" : null);
            }
        }
        else
        {
            // Keep version dates in sync with policy org dates by default
            currentVersion.UpdateOrgEffectDate(input.OrgEffectDate);
            currentVersion.UpdateOrgExpireDate(input.OrgExpireDate);
        }

        // Upsert Products/Coverages/CoverageLevels (if provided)
        if (input.Products != null)
        {
            // ID-based upsert: match by policy_product.id
            var desiredProducts = input.Products
                .GroupBy(p => p.Id ?? Guid.Empty)
                .Select(g => g.First())
                .ToList();

            var existingProducts = currentVersion.PolicyProducts.Where(x => !x.IsDeleted).ToList();
            var existingById = existingProducts.ToDictionary(x => x.Id, x => x);
            var desiredExistingIds = desiredProducts.Where(p => p.Id.HasValue).Select(p => p.Id!.Value).ToHashSet();

            foreach (var p in desiredProducts)
            {
                PolicyProduct? productEntity = null;
                if (p.Id.HasValue)
                {
                    existingById.TryGetValue(p.Id.Value, out productEntity);
                    if (productEntity == null && !isNewVersion)
                    {
                        throw new BusinessException("Policy:PolicyProduct:NotFound")
                            .WithData("PolicyProductId", p.Id.Value)
                            .WithData("PolicyId", entity.Id);
                    }
                }

                if (productEntity == null)
                {
                    productEntity = new PolicyProduct(
                        GuidGenerator.Create(),
                        currentVersion.Id,
                        p.ProductId,
                        p.PremiumTotal,
                        p.Premium,
                        p.Vat,
                        p.InsurerProductCode,
                        p.AmountLiability,
                        p.Discount,
                        p.DiscountRate,
                        p.Markup
                    );
                    await PolicyProductManager.CreateAsync(productEntity);
                    currentVersion.PolicyProducts.Add(productEntity);
                    existingById[productEntity.Id] = productEntity;
                }
                else
                {
                    await PolicyProductManager.UpdateAsync(
                        productEntity,
                        p.InsurerProductCode,
                        p.AmountLiability,
                        p.PremiumTotal,
                        p.Premium,
                        p.Vat,
                        p.Discount,
                        p.DiscountRate,
                        p.Markup
                    );
                }

                // Coverages for this product
                if (p.Coverages != null)
                {
                    // ID-based upsert: match by policy_coverage.id
                    var desiredCoverages = p.Coverages
                        // If client doesn't send Id, dedupe by CoverageId (otherwise everything collapses to Guid.Empty)
                        .GroupBy(c => c.Id ?? c.CoverageId)
                        .Select(g => g.First())
                        .ToList();

                    var existingCoverages = productEntity.PolicyCoverages.Where(x => !x.IsDeleted).ToList();
                    var existingCoverageById = existingCoverages.ToDictionary(x => x.Id, x => x);
                    var existingCoverageByCoverageId = existingCoverages
                        .GroupBy(x => x.CoverageId)
                        .ToDictionary(g => g.Key, g => g.First());
                    var desiredExistingCoverageIds = desiredCoverages
                        .Where(c => c.Id.HasValue)
                        .Select(c => c.Id!.Value)
                        .ToHashSet();

                    foreach (var c in desiredCoverages)
                    {
                        PolicyCoverage? coverageEntity = null;
                        if (c.Id.HasValue)
                        {
                            existingCoverageById.TryGetValue(c.Id.Value, out coverageEntity);
                            if (coverageEntity == null && !isNewVersion)
                            {
                                throw new BusinessException("Policy:PolicyCoverage:NotFound")
                                    .WithData("PolicyCoverageId", c.Id.Value)
                                    .WithData("PolicyId", entity.Id);
                            }
                        }
                        else
                        {
                            // Fallback: match existing row by CoverageId when Id isn't provided
                            existingCoverageByCoverageId.TryGetValue(c.CoverageId, out coverageEntity);
                        }

                        if (coverageEntity == null)
                        {
                            coverageEntity = new PolicyCoverage(
                                GuidGenerator.Create(),
                                productEntity.Id,
                                c.CoverageId,
                                c.UomId,
                                c.Quantity,
                                c.TaxId,
                                c.PremiumRate,
                                c.PremiumTotal,
                                c.Premium,
                                c.Vat,
                                c.CoverageParentId,
                                c.InsurerCoverageCode,
                                c.TableRateLineId,
                                c.AmountLiability,
                                c.NetRate,
                                c.BaseRate,
                                c.FlatRate,
                                c.Loading,
                                c.Discount,
                                c.DiscountRate
                            );
                            await PolicyCoverageManager.CreateAsync(coverageEntity);
                            productEntity.PolicyCoverages.Add(coverageEntity);
                            existingCoverageById[coverageEntity.Id] = coverageEntity;
                            existingCoverageByCoverageId[coverageEntity.CoverageId] = coverageEntity;
                        }
                        else
                        {
                            await PolicyCoverageManager.UpdateAsync(
                                coverageEntity,
                                c.CoverageParentId,
                                c.InsurerCoverageCode,
                                c.TableRateLineId,
                                c.AmountLiability,
                                c.Quantity,
                                c.NetRate,
                                c.BaseRate,
                                c.FlatRate,
                                c.Loading,
                                c.PremiumRate,
                                c.PremiumTotal,
                                c.Premium,
                                c.Vat,
                                c.Discount,
                                c.DiscountRate
                            );
                        }

                        // Track this coverage as "kept" even if client didn't send Id
                        desiredExistingCoverageIds.Add(coverageEntity.Id);

                        // Coverage Levels (match by TypeId + BasisId)
                        if (c.CoverageLevels != null)
                        {
                            // ID-based upsert: match by policy_coverage_level.id
                            var desiredLevels = c.CoverageLevels
                                // If client doesn't send Id, dedupe by TypeId+BasisId (deductible selection)
                                .GroupBy(l => l.Id?.ToString() ?? $"{l.CoverageLevelTypeId:N}:{l.CoverageLevelBasisId:N}")
                                .Select(g => g.First())
                                .ToList();

                            var existingLevels = coverageEntity.PolicyCoverageLevels.Where(x => !x.IsDeleted).ToList();
                            var existingLevelById = existingLevels.ToDictionary(x => x.Id, x => x);
                            var existingLevelByTypeBasis = existingLevels
                                .GroupBy(x => $"{x.CoverageLevelTypeId:N}:{x.CoverageLevelBasisId:N}")
                                .ToDictionary(g => g.Key, g => g.First());
                            var desiredExistingLevelIds = desiredLevels
                                .Where(l => l.Id.HasValue)
                                .Select(l => l.Id!.Value)
                                .ToHashSet();

                            foreach (var l in desiredLevels)
                            {
                                PolicyCoverageLevel? levelEntity = null;
                                if (l.Id.HasValue)
                                {
                                    existingLevelById.TryGetValue(l.Id.Value, out levelEntity);
                                    if (levelEntity == null && !isNewVersion)
                                    {
                                        throw new BusinessException("Policy:PolicyCoverageLevel:NotFound")
                                            .WithData("PolicyCoverageLevelId", l.Id.Value)
                                            .WithData("PolicyId", entity.Id);
                                    }
                                }
                                else
                                {
                                    // Fallback: match by TypeId+BasisId when Id isn't provided
                                    var k = $"{l.CoverageLevelTypeId:N}:{l.CoverageLevelBasisId:N}";
                                    existingLevelByTypeBasis.TryGetValue(k, out levelEntity);
                                }

                                if (levelEntity == null)
                                {
                                    levelEntity = new PolicyCoverageLevel(
                                        GuidGenerator.Create(),
                                        coverageEntity.Id,
                                        l.CoverageLevelTypeId,
                                        l.CoverageLevelBasisId,
                                        l.AmountType,
                                        l.FromAmount,
                                        l.ToAmount,
                                        l.ConditionScript,
                                        l.ComputeScript
                                    );
                                    await PolicyCoverageLevelManager.CreateAsync(levelEntity);
                                    coverageEntity.PolicyCoverageLevels.Add(levelEntity);
                                    existingLevelById[levelEntity.Id] = levelEntity;
                                    existingLevelByTypeBasis[$"{levelEntity.CoverageLevelTypeId:N}:{levelEntity.CoverageLevelBasisId:N}"] = levelEntity;
                                }
                                else
                                {
                                    await PolicyCoverageLevelManager.UpdateAsync(
                                        levelEntity,
                                        l.CoverageLevelTypeId,
                                        l.CoverageLevelBasisId,
                                        l.ConditionScript,
                                        l.ComputeScript,
                                        l.AmountType,
                                        l.FromAmount,
                                        l.ToAmount
                                    );
                                }

                                // Track this level as "kept" even if client didn't send Id
                                desiredExistingLevelIds.Add(levelEntity.Id);
                            }

                            // Soft-delete removed levels
                            foreach (var existing in existingLevels)
                            {
                                if (!desiredExistingLevelIds.Contains(existing.Id))
                                {
                                    await PolicyCoverageLevelRepository.DeleteAsync(existing);
                                }
                            }
                        }
                    }

                    // Soft-delete removed coverages
                    foreach (var existing in existingCoverages)
                    {
                        if (!desiredExistingCoverageIds.Contains(existing.Id))
                        {
                            await PolicyCoverageRepository.DeleteAsync(existing);
                        }
                    }
                }
            }

            // Soft-delete removed products
            foreach (var existing in existingProducts)
            {
                if (!desiredExistingIds.Contains(existing.Id))
                {
                    await PolicyProductRepository.DeleteAsync(existing);
                }
            }
        }

        // Upsert RiskObject + RiskMotor (single object model)
        if (input.RiskObject != null)
        {
            PolicyRiskObject? riskObjectEntity = null;
            var existingRiskObjects = currentVersion.PolicyRiskObjects.Where(x => !x.IsDeleted).ToList();
            var existingRiskObjectById = existingRiskObjects.ToDictionary(x => x.Id, x => x);

            if (input.RiskObject.Id.HasValue)
            {
                existingRiskObjectById.TryGetValue(input.RiskObject.Id.Value, out riskObjectEntity);
                if (riskObjectEntity == null && !isNewVersion)
                {
                    throw new BusinessException("Policy:PolicyRiskObject:NotFound")
                        .WithData("PolicyRiskObjectId", input.RiskObject.Id.Value)
                        .WithData("PolicyId", entity.Id);
                }
            }
            else
            {
                // Backward compatibility: if no id sent, pick the first
                riskObjectEntity = existingRiskObjects.FirstOrDefault();
            }

            if (riskObjectEntity == null)
            {
                riskObjectEntity = new PolicyRiskObject(
                    GuidGenerator.Create(),
                    entity.Id,
                    currentVersion.Id,
                    input.RiskObject.ObjectTypeId,
                    input.RiskObject.RepName,
                    input.RiskObject.RepIdNo,
                    input.RiskObject.RepPassport,
                    input.RiskObject.RepPhone,
                    input.RiskObject.RepEmail,
                    input.RiskObject.RepProvinceId,
                    input.RiskObject.RepWardId,
                    input.RiskObject.RepAddress,
                    input.RiskObject.RepFullAddress,
                    input.RiskObject.RiskObjectProvinceId,
                    input.RiskObject.RiskObjectWardId,
                    input.RiskObject.RiskObjectAddress,
                    input.RiskObject.RiskObjectFullAddress,
                    input.RiskObject.RiskObjectLat,
                    input.RiskObject.RiskObjectLong
                );
                await PolicyRiskObjectManager.CreateAsync(riskObjectEntity);
                currentVersion.PolicyRiskObjects.Add(riskObjectEntity);
            }
            else
            {
                // ObjectTypeId is immutable in domain entity
                await PolicyRiskObjectManager.UpdateAsync(
                    riskObjectEntity,
                    input.RiskObject.RepName,
                    input.RiskObject.RepIdNo,
                    input.RiskObject.RepPassport,
                    input.RiskObject.RepPhone,
                    input.RiskObject.RepEmail,
                    input.RiskObject.RepProvinceId,
                    input.RiskObject.RepWardId,
                    input.RiskObject.RepAddress,
                    input.RiskObject.RepFullAddress,
                    input.RiskObject.RiskObjectProvinceId,
                    input.RiskObject.RiskObjectWardId,
                    input.RiskObject.RiskObjectAddress,
                    input.RiskObject.RiskObjectFullAddress,
                    input.RiskObject.RiskObjectLat,
                    input.RiskObject.RiskObjectLong
                );
            }

            // Sync PolicyRiskObjectDocument links (policy_risk_object_document)
            if (input.RiskObject.Documents != null)
            {
                var desiredDocIds = input.RiskObject.Documents
                    .Select(d => d.DocumentId)
                    .Where(id => id != Guid.Empty)
                    .Distinct()
                    .ToList();

                // Filter to existing ResDocument ids to avoid FK errors
                if (desiredDocIds.Count > 0)
                {
                    var docQuery = await ResDocumentRepository.GetQueryableAsync();
                    desiredDocIds = await AsyncExecuter.ToListAsync(
                        docQuery.Where(d => desiredDocIds.Contains(d.Id)).Select(d => d.Id)
                    );
                }

                var desiredSet = desiredDocIds.ToHashSet();

                var existingLinks =
                    await PolicyRiskObjectDocumentRepository.GetListAsync(x =>
                        x.PolicyRiskObjectId == riskObjectEntity.Id
                    );

                var existingDocIdSet = existingLinks.Select(x => x.DocumentId).ToHashSet();

                // Add new links
                foreach (var docId in desiredSet)
                {
                    if (!existingDocIdSet.Contains(docId))
                    {
                        await PolicyRiskObjectDocumentRepository.InsertAsync(
                            new PolicyRiskObjectDocument(riskObjectEntity.Id, docId)
                        );
                    }
                }

                // Remove missing links
                foreach (var link in existingLinks)
                {
                    if (!desiredSet.Contains(link.DocumentId))
                    {
                        await PolicyRiskObjectDocumentRepository.DeleteAsync(link);
                    }
                }
            }

            // Risk motor (single record)
            if (input.RiskObject.RiskObjectMotor != null)
            {
                var m = input.RiskObject.RiskObjectMotor;
                ValidateCarProductionYear(m.CarProductionYear);

                PolicyRiskMotor? motorEntity = null;
                var existingMotors = riskObjectEntity.PolicyRiskMotors.Where(x => !x.IsDeleted).ToList();
                var existingMotorById = existingMotors.ToDictionary(x => x.Id, x => x);

                if (m.Id.HasValue)
                {
                    existingMotorById.TryGetValue(m.Id.Value, out motorEntity);
                    if (motorEntity == null && !isNewVersion)
                    {
                        throw new BusinessException("Policy:PolicyRiskMotor:NotFound")
                            .WithData("PolicyRiskMotorId", m.Id.Value)
                            .WithData("PolicyId", entity.Id);
                    }
                }
                else
                {
                    motorEntity = existingMotors.FirstOrDefault();
                }

                if (motorEntity == null)
                {
                    motorEntity = new PolicyRiskMotor(
                        GuidGenerator.Create(),
                        riskObjectEntity.Id,
                        m.RiskObjectValue,
                        m.MotorClassCode,
                        m.CarLineCode,
                        m.CarGroupCode,
                        m.CarTypeCode,
                        m.CarBrandCode,
                        m.CarModelCode,
                        m.CarCategoryCode,
                        m.CarUsage,
                        m.CarOld,
                        m.CarProductionYear,
                        m.CarPlate,
                        m.CarPlateType,
                        m.CarPlateClear,
                        m.CarSeatNumber,
                        m.CarVin,
                        m.CarEngineNumber,
                        m.CarPayloadCapacity,
                        m.CarColor,
                        m.CarOrigin,
                        m.CarNew
                    );
                    await PolicyRiskMotorManager.CreateAsync(motorEntity);
                    riskObjectEntity.PolicyRiskMotors.Add(motorEntity);
                }
                else
                {
                    await PolicyRiskMotorManager.UpdateAsync(
                        motorEntity,
                        riskObjectEntity.Id,
                        m.RiskObjectValue,
                        m.MotorClassCode,
                        m.CarLineCode,
                        m.CarGroupCode,
                        m.CarTypeCode,
                        m.CarBrandCode,
                        m.CarModelCode,
                        m.CarCategoryCode,
                        m.CarUsage,
                        m.CarOld,
                        m.CarProductionYear,
                        m.CarPlate,
                        m.CarPlateType,
                        m.CarPlateClear,
                        m.CarSeatNumber,
                        m.CarVin,
                        m.CarEngineNumber,
                        m.CarPayloadCapacity,
                        m.CarColor,
                        m.CarOrigin,
                        m.CarNew
                    );
                }
            }
        }

        // Upsert Documents (policy_document)
        if (input.Documents != null)
        {
            var desiredDocumentIds = input.Documents
                .Select(d => d.DocumentId)
                .Distinct()
                .ToHashSet();

            var existingDocs = await PolicyDocumentRepository.GetListAsync(x => x.PolicyId == entity.Id);
            var existingDocIds = existingDocs
                .Where(d => d.DocumentId.HasValue)
                .Select(d => d.DocumentId!.Value)
                .ToHashSet();

            // Add new
            foreach (var docId in desiredDocumentIds)
            {
                if (!existingDocIds.Contains(docId))
                {
                    var docEntity = new PolicyDocument(GuidGenerator.Create(), entity.Id, docId);
                    await PolicyDocumentManager.CreateAsync(docEntity);
                }
            }

            // Soft-delete removed
            foreach (var doc in existingDocs)
            {
                if (doc.DocumentId.HasValue && !desiredDocumentIds.Contains(doc.DocumentId.Value))
                {
                    await PolicyDocumentRepository.DeleteAsync(doc);
                }
            }
        }

        // Persist aggregated discount/markup on policy_version (same rules as main policy update / PolicyAmount)
        var (endorsementVersionDiscount, endorsementVersionMarkup) = ComputeAggregatedDiscountAndMarkupForAmount(
            input.Products?.Select(p => (p.Discount, p.Markup)),
            input.Amount?.Discount,
            input.Discount,
            input.Amount?.Markup,
            input.Version?.Markup);
        currentVersion.UpdateDiscount(endorsementVersionDiscount);
        currentVersion.UpdateMarkup(endorsementVersionMarkup);

        // Endorsement: do NOT apply POLICY_AMOUNT (total premium) — that is for Create/Update only.
        // Tổng tăng/giảm phí chỉ từ FE: EndorsementAdjustmentAmount (ưu tiên) hoặc tổng EndorsementCoverageChanges — không tính chênh trên BE.
        const string EndorsementAdjustmentFeeItemCode = "ENDORSEMENT_ADJUSTMENT_AMOUNT";
        var endorsementAdjustmentFeeItemId = await GetResFeeItemIdByCodeAsync(EndorsementAdjustmentFeeItemCode);

        var amountIssueDate = input.IssueDate ?? Clock.Now;
        var totalChangeAmount = input.Amount?.EndorsementAdjustmentAmount
            ?? (input.Amount?.EndorsementCoverageChanges?.Count > 0
                ? input.Amount.EndorsementCoverageChanges.Sum(x => x.ChangeAmount)
                : 0m);

        // Một bản ghi điều chỉnh SĐBS / version: upsert (không append mỗi lần save).
        var existingAdjustments = (await PolicyAmountRepository.GetListAsync(x =>
            x.PolicyId == entity.Id &&
            x.PolicyVersionId == currentVersion.Id &&
            x.FeeItemId == endorsementAdjustmentFeeItemId &&
            !x.IsDeleted))
            .OrderByDescending(x => x.LastModificationTime)
            .ThenByDescending(x => x.CreationTime)
            .ToList();
        var existingAdjustment = existingAdjustments.FirstOrDefault();
        if (existingAdjustments.Count > 1)
        {
            foreach (var extra in existingAdjustments.Skip(1))
                await PolicyAmountManager.DeleteAsync(extra);
        }

        if (totalChangeAmount != 0)
        {
            if (existingAdjustment == null)
            {
                var amountEntity = new PolicyAmount(
                    GuidGenerator.Create(),
                    entity.Id,
                    currentVersion.Id,
                    endorsementAdjustmentFeeItemId,
                    amountIssueDate,
                    totalChangeAmount,
                    totalChangeAmount,
                    0
                );
                await PolicyAmountManager.CreateAsync(amountEntity);
            }
            else
            {
                await PolicyAmountManager.UpdateAsync(
                    existingAdjustment,
                    amountIssueDate,
                    totalChangeAmount,
                    totalChangeAmount,
                    0,
                    existingAdjustment.PaymentStatus,
                    existingAdjustment.PaymentMethodId,
                    existingAdjustment.PaymentDate);
            }
        }
        else if (existingAdjustment != null)
        {
            await PolicyAmountManager.UpdateAsync(
                existingAdjustment,
                amountIssueDate,
                0,
                0,
                0,
                existingAdjustment.PaymentStatus,
                existingAdjustment.PaymentMethodId,
                existingAdjustment.PaymentDate);
        }

        // Keep policy and policy version status unchanged (only new policy_version gets status = draft)

        await Manager.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();

        if (input.SubmitForApproval)
        {
            await _elsaWorkflowService.InitEndorsementWorkflowAsync(currentVersion.Id);
        }

        // Return detail after endorsement
        return await GetAsync(entity.Id);
    }

    public virtual async Task<UpdatePrevEffectDateResultDto> UpdatePrevEffectDateAsync(UpdatePrevEffectDateInputDto input)
    {
        if (input == null) throw new ArgumentNullException(nameof(input));
        if (input.PolicyVersionId == Guid.Empty) throw new BusinessException("Policy:PolicyVersion:PolicyVersionIdRequired");

        var currentVersion = await PolicyVersionRepository.GetAsync(input.PolicyVersionId);

        if (!currentVersion.EndorsementType.HasValue)
        {
            return new UpdatePrevEffectDateResultDto { Updated = false, PreviousPolicyVersionId = null };
        }

        // "Previous version" = highest version number < current version (same PolicyId).
        var prevVersionQuery = (await PolicyVersionRepository.GetQueryableAsync())
            .Where(v => !v.IsDeleted && v.PolicyId == currentVersion.PolicyId && v.Version < currentVersion.Version)
            .OrderByDescending(v => v.Version);
        var prevVersion = await AsyncExecuter.FirstOrDefaultAsync(prevVersionQuery);

        if (prevVersion == null)
        {
            return new UpdatePrevEffectDateResultDto { Updated = false, PreviousPolicyVersionId = null };
        }

        // Resolve endorsement type code via AdminConfig (code = ENDORSEMENT_POLICY_TYPE).
        // Some systems store the decision code in either SubCode or Value, so we match both.
        const string AdminConfigEndorsementPolicyTypeCode = "ENDORSEMENT_POLICY_TYPE";
        const string EffectDateEndorsementCode = "POLICY_EFFECT_DATE_ENDORSEMENT";
        const string EffectDateEndorsementCodeTruncated = "POLICY_EFFECT_DATE_ENDORS";
        const string AllInforEndorsementCode = "POLICY_ALL_INFOR_ENDORSEMENT";
        const string AllInforEndorsementCodeTruncated = "POLICY_ALL_INFOR_ENDORSEM";

        string? Normalize(string? s) =>
            string.IsNullOrWhiteSpace(s) ? null : s.Trim().ToUpperInvariant();

        bool MatchesExpected(string? candidate, params string[] expected)
        {
            var n = Normalize(candidate);
            if (n == null) return false;

            foreach (var e in expected)
            {
                if (n == Normalize(e))
                {
                    return true;
                }
            }

            return false;
        }

        var adminConfigQuery = await _adminConfigRepository.GetQueryableAsync();
        var adminConfig = await AsyncExecuter.FirstOrDefaultAsync(adminConfigQuery.Where(c =>
            c.Id == currentVersion.EndorsementType.Value &&
            c.Code == AdminConfigEndorsementPolicyTypeCode &&
            c.Status == AdminConfigStatus.Active));

        var endorsementCodeValue = adminConfig?.Value;
        var endorsementCodeSubCode = adminConfig?.SubCode;

        var isEffectDateEndorsement =
            MatchesExpected(endorsementCodeValue, EffectDateEndorsementCode, EffectDateEndorsementCodeTruncated) ||
            MatchesExpected(endorsementCodeSubCode, EffectDateEndorsementCode, EffectDateEndorsementCodeTruncated);

        var isAllInforEndorsement =
            MatchesExpected(endorsementCodeValue, AllInforEndorsementCode, AllInforEndorsementCodeTruncated) ||
            MatchesExpected(endorsementCodeSubCode, AllInforEndorsementCode, AllInforEndorsementCodeTruncated);

        // Spec condition:
        // - effect-date endorsement => continue
        // - all-infor endorsement => continue only when current effect date != prev effect date
        var shouldUpdate =
            isEffectDateEndorsement ||
            (isAllInforEndorsement && currentVersion.EffectDate != prevVersion.EffectDate);

        if (shouldUpdate)
        {
            // Spec update rules:
            // + If prevVersion.EffectDate < cur.EffectDate (and also prev == cur) => set prevVersion.ExpireDate = prevVersion.EffectDate
            // + If prevVersion.EffectDate > cur.EffectDate                     => set both prevVersion.EffectDate and prevVersion.ExpireDate to cur.EffectDate
            if (prevVersion.EffectDate <= currentVersion.EffectDate)
            {
                prevVersion.UpdateExpireDate(prevVersion.EffectDate);
            }
            else
            {
                prevVersion.UpdateEffectDate(currentVersion.EffectDate);
                prevVersion.UpdateExpireDate(currentVersion.EffectDate);
            }
        }
        else
        {
            prevVersion.UpdateExpireDate(prevVersion.EffectDate);
        }

        await PolicyVersionRepository.UpdateAsync(prevVersion);
        await CurrentUnitOfWork.SaveChangesAsync();

        return new UpdatePrevEffectDateResultDto
        {
            Updated = true,
            PreviousPolicyVersionId = prevVersion.Id
        };
    }

    /// <inheritdoc />
    [Authorize(PolicyPermissions.View)]
    public virtual async Task<List<MotorbikeInsurerVehicleTypeOptionDto>> GetMotorbikeVehicleTypeOptionsAsync(Guid insurerId)
    {
        if (insurerId == Guid.Empty)
        {
            return new List<MotorbikeInsurerVehicleTypeOptionDto>();
        }

        // Theo spec nghiệp vụ: tên bảng mapping (đúng chính tả dữ liệu master).
        const string motorTypeBusinessName = "RES_MORTOR_TYPE";
        var now = Clock.Now;

        var q = await InsurerDictionaryRepository.GetQueryableAsync();
        var query = q
            .Where(x => x.BusinessName == motorTypeBusinessName)
            .Where(x => x.InsurerId == insurerId)
            .Where(x => x.Status == InsurerDictionaryStatus.Active)
            .Where(x => x.EffectDate <= now)
            .Where(x => x.ExpireDate == null || x.ExpireDate >= now)
            .OrderBy(x => x.OwnCode);

        var items = await AsyncExecuter.ToListAsync(query);
        var result = new List<MotorbikeInsurerVehicleTypeOptionDto>(items.Count);

        foreach (var x in items)
        {
            var ownCode = x.OwnCode;
            var label = BuildMotorbikeVehicleTypeLabelFromExtraData(x.ExtraData, ownCode);

            result.Add(new MotorbikeInsurerVehicleTypeOptionDto
            {
                SelectValue = ownCode,
                CarTypeCode = ownCode,
                Label = label
            });
        }

        return result;
    }

    /// <summary>
    /// Label hiển thị: nội dung ExtraData (nếu JSON có name/displayName thì lấy; không thì dùng nguyên chuỗi; rỗng thì OwnCode).
    /// </summary>
    private static string BuildMotorbikeVehicleTypeLabelFromExtraData(string? extraData, string ownCode)
    {
        if (string.IsNullOrWhiteSpace(extraData))
        {
            return ownCode;
        }

        var trimmed = extraData.Trim();
        var fromJson = TryGetLabelFromInsurerDictionaryExtraData(trimmed);
        if (!string.IsNullOrWhiteSpace(fromJson))
        {
            return fromJson!;
        }

        return trimmed;
    }

    private static string? TryGetLabelFromInsurerDictionaryExtraData(string? extraData)
    {
        if (string.IsNullOrWhiteSpace(extraData))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(extraData);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            if (doc.RootElement.TryGetProperty("name", out var n) && n.ValueKind == JsonValueKind.String)
            {
                return n.GetString();
            }

            if (doc.RootElement.TryGetProperty("displayName", out var d) && d.ValueKind == JsonValueKind.String)
            {
                return d.GetString();
            }
        }
        catch (JsonException)
        {
            // ignore invalid JSON
        }

        return null;
    }

    [Authorize(PolicyPermissions.View)]
    public virtual async Task<List<PolicyWorkTaskHistoryItemDto>> GetWorkTaskHistoryAsync(Guid policyVersionId)
    {
        const string businessName = "policyVersion";
        var query = await _workTaskRepository.GetQueryableAsync();
        var entities = await AsyncExecuter.ToListAsync(
            query
                .Where(t => t.BusinessName == businessName && t.BusinessKey == policyVersionId)
                .OrderByDescending(t => t.CreationTime));
        var assigneeIds = entities.Where(t => t.AssigneeId.HasValue && t.AssigneeId.Value != Guid.Empty).Select(t => t.AssigneeId!.Value).Distinct().ToList();
        var assigneeNameById = new Dictionary<Guid, string>();
        if (assigneeIds.Count > 0)
        {
            var empQuery = await HrEmployeeRepository.GetQueryableAsync();
            var pairs = await AsyncExecuter.ToListAsync(empQuery.Where(e => assigneeIds.Contains(e.Id)).Select(e => new { e.Id, e.FullName }));
            assigneeNameById = pairs.ToDictionary(x => x.Id, x => x.FullName ?? string.Empty);
        }

        // Build BusinessCode -> localized display name map from admin_config
        var businessCodes = entities
            .Where(t => !string.IsNullOrWhiteSpace(t.BusinessCode))
            .Select(t => t.BusinessCode!.ToUpperInvariant())
            .Distinct()
            .ToList();
        var businessCodeDisplayNameById = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (businessCodes.Count > 0)
        {
            var configQuery = await _adminConfigRepository.GetQueryableAsync();
            var configs = await AsyncExecuter.ToListAsync(
                configQuery.Where(c => c.Code == "BUSINESS_CODE" && businessCodes.Contains(c.SubCode)));
            foreach (var config in configs)
            {
                businessCodeDisplayNameById[config.SubCode] = config.Value;
            }
        }

        return entities.Select(t => new PolicyWorkTaskHistoryItemDto
        {
            Id = t.Id,
            Name = t.Name,
            AssigneeName = t.AssigneeId.HasValue && assigneeNameById.TryGetValue(t.AssigneeId.Value, out var assigneeName) ? assigneeName : null,
            Code = t.Code,
            EventName = t.EventName,
            Status = (int)t.Status,
            StatusText = t.Status.ToString(),
            CreationTime = t.CreationTime,
            Description = t.Description,
            BusinessCode = t.BusinessCode,
            BusinessCodeDisplayName = !string.IsNullOrWhiteSpace(t.BusinessCode) && businessCodeDisplayNameById.TryGetValue(t.BusinessCode, out var displayName) ? displayName : null,
            StartDate = t.StartDate,
            EndDate = t.EndDate
        }).ToList();
    }

    [Authorize(PolicyPermissions.Delete)]
    public override async Task DeleteAsync(Guid id)
    {
        var query = await Repository.WithDetailsAsync(
            p => p.PolicyVersions,
            p => p.PolicyCertificates,
            p => p.PolicyRiskObjects
        );

        var entity = await query.Where(p => p.Id == id).FirstOrDefaultAsync();

        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(iOne.Policies.Policy), id);
        }

        if (entity.Status != PolicyStatus.Draft)
        {
            throw new BusinessException("Policy:Policy:DeleteOnlyDraftAllowed");
        }

        var policyId = entity.Id;

        // 1. Delete PolicyDocument (Root level)
        await PolicyDocumentRepository.DeleteAsync(x => x.PolicyId == policyId);

        // 2. Process Versions and their children
        var versions = await PolicyVersionRepository.GetListAsync(x => x.PolicyId == policyId);
        foreach (var version in versions)
        {
            var versionId = version.Id;

            // PolicyAmount (linked to version)
            await PolicyAmountRepository.DeleteAsync(x => x.PolicyVersionId == versionId);

            // PolicyProduct -> PolicyCoverage -> PolicyCoverageLevel
            var products = await PolicyProductRepository.GetListAsync(x => x.PolicyVersionId == versionId);
            foreach (var product in products)
            {
                var coverages = await PolicyCoverageRepository.GetListAsync(x => x.PolicyProductId == product.Id);
                foreach (var coverage in coverages)
                {
                    // Delete CoverageLevels
                    await PolicyCoverageLevelRepository.DeleteAsync(x => x.PolicyCoverageId == coverage.Id);
                    // Delete Coverage
                    await PolicyCoverageRepository.DeleteAsync(coverage);
                }

                // Delete Product
                await PolicyProductRepository.DeleteAsync(product);
            }

            // PolicyRiskObject -> PolicyRiskMotor
            var riskObjects = await PolicyRiskObjectRepository.GetListAsync(x => x.PolicyVersionId == versionId);
            foreach (var riskObject in riskObjects)
            {
                // Delete Motor
                await PolicyRiskMotorRepository.DeleteAsync(x => x.PolicyRiskObjectId == riskObject.Id);
                // Delete RiskObject
                await PolicyRiskObjectRepository.DeleteAsync(riskObject);
            }

            // Delete Version
            await PolicyVersionRepository.DeleteAsync(version);
        }

        // 3. PolicyCertificate
        var certificates = await PolicyCertificateRepository.GetListAsync(x => x.PolicyId == policyId);
        foreach (var cert in certificates)
        {
            await PolicyCertificateRepository.DeleteAsync(cert);
        }

        // 4. Capture contract info and delete the policy itself first
        var contractId = entity.ContractId;

        // Use ID-based delete to avoid ambiguity
        await Repository.DeleteAsync(policyId);

        // 5. Cleanup PolicyContract and PolicyContractDocument if orphaned
        if (contractId.HasValue)
        {
            // Check if other active/available policies still use this contract.
            // Since we just deleted the policy above, it should no longer be in the count.
            var otherPoliciesCount = await PolicyRepository.CountAsync(x => x.ContractId == contractId.Value);

            if (otherPoliciesCount == 0)
            {
                // Delete linked documents first
                await PolicyContractDocumentRepository.DeleteAsync(x => x.PolicyContractId == contractId.Value);
                // Delete Contract using expression to avoid ambiguity
                await PolicyContractRepository.DeleteAsync(x => x.Id == contractId.Value);
            }
        }

        await CurrentUnitOfWork.SaveChangesAsync();
    }

    /// <summary>
    /// Validates that CarProductionYear (Năm sản xuất) is not in the future; only allow dates less than or equal to today.
    /// </summary>
    private static void ValidateCarProductionYear(DateTime? carProductionYear)
    {
        if (!carProductionYear.HasValue)
        {
            return;
        }

        var dateOnly = carProductionYear.Value.Date;
        if (dateOnly > DateTime.Today)
        {
            throw new BusinessException("Policy:Policy:ProductionYearMustNotBeFuture");
        }
    }

    /// <summary>
    /// Checks uniqueness of the pair (InsurerPolicyNo, CertificateNo) among policies with Status != Cancelled.
    /// Only reports duplicate when BOTH insurerPolicyNo and certificateNo match the same existing policy.
    /// </summary>
    /// <param name="insurerPolicyNo">Insurer policy number (required for check).</param>
    /// <param name="certificateNo">Certificate number (required for check).</param>
    /// <param name="excludePolicyId">When updating, pass the current policy Id so it is excluded. When creating, pass null.</param>
    /// <returns>True if the pair already exists on another non-cancelled policy.</returns>
    private async Task<bool> CheckInsurerPolicyNoAndCertificateNoPairExistsAsync(
        string? insurerPolicyNo,
        string? certificateNo,
        Guid? excludePolicyId)
    {
        var trimmedInsurer = insurerPolicyNo?.Trim();
        var trimmedCert = certificateNo?.Trim();
        if (string.IsNullOrWhiteSpace(trimmedInsurer) || string.IsNullOrWhiteSpace(trimmedCert))
        {
            return false;
        }

        var policyQuery = await Repository.GetQueryableAsync();
        var certQuery = await PolicyCertificateRepository.GetQueryableAsync();

        var exists = await AsyncExecuter.AnyAsync(
            from p in policyQuery
            join c in certQuery on p.Id equals c.PolicyId
            where p.Status != PolicyStatus.Cancelled
                  && (!excludePolicyId.HasValue || p.Id != excludePolicyId.Value)
                  && p.InsurerPolicyNo != null
                  && p.InsurerPolicyNo.Trim() == trimmedInsurer
                  && c.CertificateNo != null
                  && c.CertificateNo.Trim() == trimmedCert
            select p.Id);

        return exists;
    }

    /// <summary>
    /// Applies policy base fields from input. When <paramref name="updateOrgDates"/> is false (endorsement),
    /// OrgEffectDate and OrgExpireDate are not updated so they stick to the policy origin.
    /// </summary>
    private static void ApplyPolicyBaseFieldsFromInput(iOne.Policies.Policy entity, UpdatePolicyDetailDto input, bool updateOrgDates, bool updateStatus)
    {
        ApplyPolicyBaseFieldsFromInputImpl(entity, input, updateOrgDates, updateStatus);
    }

    private static void ApplyPolicyBaseFieldsForUpdate(iOne.Policies.Policy entity, UpdatePolicyDetailDto input) =>
        ApplyPolicyBaseFieldsFromInput(entity, input, updateOrgDates: true, updateStatus: true);

    private static void ApplyPolicyBaseFieldsForEndorsement(iOne.Policies.Policy entity, UpdatePolicyDetailDto input) =>
        ApplyPolicyBaseFieldsFromInput(entity, input, updateOrgDates: false, updateStatus: false);

    /// <summary>
    /// Converts PolicyStatus enum to policy_version.status string (lowercase).
    /// </summary>
    private static string PolicyStatusToVersionStatusString(PolicyStatus status)
    {
        return status.ToString().ToLowerInvariant();
    }

    // NOTE:
    // Product attribute "required" validation is handled by ValidateRequiredProductAttributesUsingRequiredSpecAsync(...)
    // to keep logic consistent with get-attribute-required-spec API (FE + BE).

    /// <summary>
    /// When motor detail is sent on create: if plate is blank, engine number and VIN are both required;
    /// if plate is set, at least one of engine number or VIN is required.
    /// </summary>
    private void ValidateCreateRiskMotorPlateEngineVin(CreatePolicyRiskMotorInputDto? motor)
    {
        if (motor == null)
        {
            return;
        }

        var plateBlank = string.IsNullOrWhiteSpace(motor.CarPlate);
        var hasEngine = !string.IsNullOrWhiteSpace(motor.CarEngineNumber);
        var hasVin = !string.IsNullOrWhiteSpace(motor.CarVin);

        if (plateBlank)
        {
            if (!hasEngine || !hasVin)
            {
                throw new UserFriendlyException(L["Policy:Policy:RiskMotorRequiresEngineAndVinWhenNoPlate"].Value);
            }
        }
        else if (!hasEngine && !hasVin)
        {
            throw new UserFriendlyException(L["Policy:Policy:RiskMotorRequiresEngineOrVinWhenPlatePresent"].Value);
        }
    }

    /// <summary>
    /// Validates that for any VCX product (ProductType.Code = "VCX"), the main coverage's AmountLiability does not exceed the car value (RiskObjectValue).
    /// </summary>
    private async Task ValidateVcxMainAmountLiabilityNotExceedingCarValueAsync(
        List<CreatePolicyProductInputDto>? products,
        decimal? carValue)
    {
        if (products == null || products.Count == 0) return;
        var productCoverages = products
            .Select(p => (p.ProductId, Coverages: p.Coverages?.Select(c => (c.CoverageId, c.AmountLiability)).ToList() ?? new List<(Guid CoverageId, decimal? AmountLiability)>()))
            .ToList();
        await ValidateVcxMainAmountLiabilityCoreAsync(productCoverages, carValue);
    }

    /// <summary>
    /// Validates that for any VCX product (ProductType.Code = "VCX"), the main coverage's AmountLiability does not exceed the car value (RiskObjectValue).
    /// </summary>
    private async Task ValidateVcxMainAmountLiabilityNotExceedingCarValueAsync(
        List<UpdatePolicyProductInputDto>? products,
        decimal? carValue)
    {
        if (products == null || products.Count == 0) return;
        var productCoverages = products
            .Select(p => (p.ProductId, Coverages: p.Coverages?.Select(c => (c.CoverageId, c.AmountLiability)).ToList() ?? new List<(Guid CoverageId, decimal? AmountLiability)>()))
            .ToList();
        await ValidateVcxMainAmountLiabilityCoreAsync(productCoverages, carValue);
    }

    private async Task ValidateVcxMainAmountLiabilityCoreAsync(
        List<(Guid ProductId, List<(Guid CoverageId, decimal? AmountLiability)> Coverages)> productCoverages,
        decimal? carValue)
    {
        if (productCoverages.Count == 0) return;
        var productIds = productCoverages.Select(x => x.ProductId).Distinct().ToList();
        var productQuery = await ProProductRepository.GetQueryableAsync();
        var productsWithTypeAndCoverages = await AsyncExecuter.ToListAsync(
            productQuery
                .Where(p => productIds.Contains(p.Id))
                .Include(p => p.ProductType)
                .Include(p => p.ProductCoverages)
                    .ThenInclude(pc => pc.Coverage));

        foreach (var product in productsWithTypeAndCoverages)
        {
            var productTypeCode = product.ProductType?.Code?.Trim();
            if (string.IsNullOrEmpty(productTypeCode) || !string.Equals(productTypeCode, "VCX", StringComparison.OrdinalIgnoreCase))
                continue;

            if (product.ProductCoverages == null) continue;
            var mainCoverageId = product.ProductCoverages
                .Where(pc => pc.Coverage != null && pc.Coverage.Type == ProCoverageTermType.Main)
                .Select(pc => pc.CoverageId)
                .FirstOrDefault();
            if (mainCoverageId == Guid.Empty) continue;

            var inputCoverages = productCoverages.FirstOrDefault(x => x.ProductId == product.Id).Coverages;
            var mainInput = inputCoverages?.FirstOrDefault(c => c.CoverageId == mainCoverageId);
            var amountLiability = mainInput?.AmountLiability ?? 0m;

            if (!carValue.HasValue || carValue.Value <= 0)
            {
                throw new UserFriendlyException(L["Policy:Policy:VcxRequiresCarValue"].Value)
                    .WithData("ProductId", product.Id);
            }

            if (amountLiability > carValue.Value)
            {
                var message = L["Policy:Policy:VcxMainAmountLiabilityExceedsCarValue", amountLiability.ToString("N0"), carValue.Value.ToString("N0")].Value;
                throw new UserFriendlyException(message)
                    .WithData("AmountLiability", amountLiability)
                    .WithData("CarValue", carValue.Value)
                    .WithData("ProductId", product.Id);
            }
        }
    }

    /// <summary>
    /// Hợp đồng lẻ (Individual) chỉ được gắn tối đa 1 policy. Nếu đã có policy khác thì throw.
    /// </summary>
    /// <param name="contractId">Contract cần kiểm tra.</param>
    /// <param name="excludePolicyId">PolicyId loại trừ (dùng khi update – policy đang chuyển sang contract này).</param>
    private async Task ValidateIndividualContractSinglePolicyAsync(Guid contractId, Guid? excludePolicyId)
    {
        var contract = await PolicyContractRepository.FindAsync(contractId);
        if (contract == null)
            throw new UserFriendlyException(L["Policy:Policy:ContractNotFound"].Value);
        if (contract.Type != PolicyContractType.Individual)
            return;

        var policyQuery = await Repository.GetQueryableAsync();
        var count = await AsyncExecuter.CountAsync(
            policyQuery.Where(p => p.ContractId == contractId && !p.IsDeleted && (excludePolicyId == null || p.Id != excludePolicyId.Value))
        );
        if (count >= 1)
        {
            throw new BusinessException("Policy:Policy:IndividualContractMaxOnePolicy");
        }
    }

    /// <summary>
    /// Parses policy_version.status (string) to PolicyStatus enum for list display.
    /// </summary>
    private static PolicyStatus ParseVersionStatusToPolicyStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return PolicyStatus.Draft;
        var lower = status.Trim().ToLowerInvariant();
        return lower switch
        {
            "quotation" => PolicyStatus.Quotation,
            "draft" => PolicyStatus.Draft,
            "active" => PolicyStatus.Active,
            "expired" => PolicyStatus.Expired,
            "terminated" => PolicyStatus.Terminated,
            "cancelled" => PolicyStatus.Cancelled,
            _ => Enum.TryParse<PolicyStatus>(status, true, out var parsed) ? parsed : PolicyStatus.Draft
        };
    }

    private static void ApplyPolicyBaseFieldsFromInputImpl(iOne.Policies.Policy entity, UpdatePolicyDetailDto input, bool updateOrgDates, bool updateStatus)
    {
        entity.UpdateLobId(input.LobId);
        entity.UpdateSellType(input.SellType);
        entity.UpdateInsurerPolicyNo(input.InsurerPolicyNo);
        entity.UpdatePolicyTypeId(input.PolicyTypeId);
        entity.UpdatePartnerId(input.PartnerId);
        entity.UpdateSellerId(input.SellerId);
        entity.UpdateImplementerId(input.ImplementerId);
        entity.UpdateCurrencyId(input.CurrencyId);
        entity.UpdateExchangeRate(input.ExchangeRate);
        if (updateStatus)
            entity.UpdateStatus(input.Status);
        entity.UpdateIssueDate(input.IssueDate);
        entity.UpdateApprovalStatus(input.ApprovalStatus);
        if (updateOrgDates)
        {
            entity.UpdateOrgEffectDate(input.OrgEffectDate);
            entity.UpdateOrgExpireDate(input.OrgExpireDate);
        }
        entity.UpdateIsRenewal(input.IsRenewal);
        entity.UpdateIsGift(input.IsGift);
        entity.UpdatePremiumTotal(input.PremiumTotal);
        entity.UpdatePremium(input.Premium);
        entity.UpdateVat(input.Vat);
        entity.UpdateDiscount(input.Discount);
        entity.UpdateDiscountRate(input.DiscountRate);
        entity.UpdateIsBankLoan(input.IsBankLoan);
        entity.UpdateChannelId(input.ChannelId);
        entity.UpdateLotImportCode(input.LotImportCode);
    }

    private async Task<Guid> GetResFeeItemIdByCodeAsync(string code)
    {
        var normalized = (code ?? string.Empty).Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new UserFriendlyException("FeeItem code is required.");
        }

        var query = await ResFeeItemRepository.GetQueryableAsync();
        var id = await AsyncExecuter.FirstOrDefaultAsync(
            query.Where(x => x.Code != null && x.Code.ToUpper() == normalized).Select(x => x.Id)
        );

        if (id == Guid.Empty)
        {
            throw new UserFriendlyException($"ResFeeItem not found for code '{normalized}'.");
        }

        return id;
    }

    private static object? ConvertToPythonValue(object? value)
    {
        if (value == null)
        {
            return null;
        }

        if (value is string s)
        {
            var t = s.Trim();
            if (t.Equals("y", StringComparison.OrdinalIgnoreCase))
            {
                return "Y";
            }

            if (t.Equals("n", StringComparison.OrdinalIgnoreCase))
            {
                return "N";
            }

            return t;
        }

        if (value is DateTime dt)
        {
            return dt.ToString("O", CultureInfo.InvariantCulture);
        }

        if (value is Guid g)
        {
            return g.ToString();
        }

        if (value is bool b)
        {
            return b;
        }

        if (value is Enum e)
        {
            return e.ToString();
        }

        if (value is byte
            || value is sbyte
            || value is short
            || value is ushort
            || value is int
            || value is uint
            || value is long
            || value is ulong
            || value is float
            || value is double
            || value is decimal)
        {
            return value;
        }

        return value.ToString();
    }

    private static object? JsonElementToObject(JsonElement el)
    {
        switch (el.ValueKind)
        {
            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                return null;
            case JsonValueKind.String:
                return el.GetString();
            case JsonValueKind.Number:
                if (el.TryGetInt64(out var l))
                {
                    return l;
                }

                if (el.TryGetDecimal(out var d))
                {
                    return d;
                }

                return el.GetDouble();
            case JsonValueKind.True:
            case JsonValueKind.False:
                return el.GetBoolean();
            default:
                // object/array -> raw json text
                return el.GetRawText();
        }
    }

    private static async Task<Dictionary<string, object?>> EvaluateComputeScriptsWithPythonAsync(
        ExtractPolicyAttributeParametersInputDto input,
        Dictionary<string, object?> values,
        List<ExtractPolicyAttributeDefinitionInputDto> attrs)
    {
        // Build a minimal JSON payload (camelCase) for python evaluation.
        // IMPORTANT: computeScript is treated as a pure transformer of the extracted value:
        // - input: `value` (already extracted from dataPath)
        // - output: `result` (or expression return)
        var payload = new
        {
            values,
            attributes = (attrs ?? new List<ExtractPolicyAttributeDefinitionInputDto>())
                .Where(a => a != null && !string.IsNullOrWhiteSpace(a.Code))
                .Select(a => new
                {
                    code = a!.Code!.Trim(),
                    name = a.Name,
                    dataPath = a.DataPath,
                    computeScript = a.ComputeScript
                })
                .ToList()
        };

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });

        var b64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

        const string pythonProgram = @"
import base64
import json
import sys

payload = json.loads(base64.b64decode(sys.argv[1]).decode('utf-8'))
values = payload.get('values') or {}
attrs = payload.get('attributes') or []

for attr in attrs:
    code = (attr.get('code') or '').strip()
    if not code:
        continue

    script = attr.get('computeScript')
    if script is None:
        continue
    script = str(script)
    if not script.strip():
        continue

    # Allow simple ""return <expr>"" scripts by translating to ""result = <expr>""
    s = script.strip()
    if s.startswith('return '):
        script = 'result = ' + s[len('return '):]

    # computeScript is evaluated ONLY against the extracted value.
    # Users should set:
    # - `result = <expr using value>` for multi-line scripts
    # - or provide a pure expression using `value`
    extracted_value = values.get(code)
    ctx = {
        '__builtins__': __builtins__,
        'value': extracted_value,
    }

    try:
        # Support both:
        # - a pure expression (eval)
        # - a multi-line script that assigns to `result` (exec)
        #
        # IMPORTANT: don't exec() then eval() for multi-line scripts
        # (eval can't parse statements like `def`, `import`, etc.)
        try:
            values[code] = eval(script, ctx, ctx)
        except SyntaxError:
            exec(script, ctx, ctx)
            if 'result' in ctx:
                values[code] = ctx.get('result')
            else:
                raise Exception('computeScript must be a Python expression or assign to variable `result`')
    except Exception as e:
        sys.stderr.write(f'computeScript error for {code}: {e}\\n')
        sys.stderr.write(f'value: {repr(extracted_value)}\\n')
        sys.stderr.write('computeScript:\\n' + script + '\\n')
        sys.exit(2)

print(json.dumps(values, ensure_ascii=False))
";

        var stdout = await RunPythonAsync(b64, pythonProgram);
        var updated = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        using (var doc = JsonDocument.Parse(stdout))
        {
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                return values;
            }

            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                updated[prop.Name] = JsonElementToObject(prop.Value);
            }
        }

        // Keep original keys even if python omitted something
        foreach (var kv in values)
        {
            if (!updated.ContainsKey(kv.Key))
            {
                updated[kv.Key] = kv.Value;
            }
        }

        return updated;
    }

    private static async Task<string> RunPythonAsync(string payloadB64, string pythonProgram)
    {
        // Prefer python3, fallback to python.
        // Allow overriding the executable path via env var (useful in containers/servers):
        // - IONE_PYTHON (e.g. "/usr/bin/python3" or "python3")
        var overrideExe = (Environment.GetEnvironmentVariable("IONE_PYTHON") ?? string.Empty).Trim();

        var candidates = new List<string>();
        if (!string.IsNullOrWhiteSpace(overrideExe))
        {
            candidates.Add(overrideExe);
        }
        else
        {
            // Common names
            candidates.Add("python3");
            candidates.Add("python");

            // Common absolute paths (Linux/macOS)
            candidates.Add("/usr/bin/python3");
            candidates.Add("/usr/local/bin/python3");
            candidates.Add("/opt/homebrew/bin/python3");
        }

        // Use a safe working directory (current directory might not exist in some deployments)
        var workingDir = AppContext.BaseDirectory;
        if (string.IsNullOrWhiteSpace(workingDir) || !Directory.Exists(workingDir))
        {
            workingDir = Directory.GetCurrentDirectory();
        }

        if (string.IsNullOrWhiteSpace(workingDir) || !Directory.Exists(workingDir))
        {
            workingDir = "/";
        }

        Exception? lastError = null;
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

                if (!p.Start())
                {
                    continue;
                }

                var stdoutTask = p.StandardOutput.ReadToEndAsync();
                var stderrTask = p.StandardError.ReadToEndAsync();

                await p.WaitForExitAsync();

                var stdout = await stdoutTask ?? string.Empty;
                var stderr = await stderrTask ?? string.Empty;

                if (p.ExitCode != 0)
                {
                    // Python exists and ran, but the script failed -> don't wrap as "runtime not available".
                    throw new UserFriendlyException(
                        $"Failed to evaluate computeScript using Python '{exe}' (exit {p.ExitCode}). {stderr}".Trim());
                }

                return stdout;
            }
            catch (UserFriendlyException)
            {
                // Script/runtime ran but failed -> bubble up as-is for clearer errors.
                throw;
            }
            catch (Exception ex)
            {
                // Start/process issues -> try next candidate
                lastError = ex;
            }
        }

        var tried = string.Join(", ", candidates.Distinct(StringComparer.OrdinalIgnoreCase));
        var overrideInfo = string.IsNullOrWhiteSpace(overrideExe) ? "not set" : overrideExe;
        throw new UserFriendlyException(
            ("Python runtime not available for computeScript evaluation. " +
             $"IONE_PYTHON: {overrideInfo}. " +
             $"WorkingDirectory: {workingDir}. " +
             $"Tried: {tried}. " +
             "Please install python3 and ensure it is in PATH, or set env var IONE_PYTHON to the python executable path. " +
             $"{lastError?.Message}").Trim());
    }

    private static bool TryGetValueByPath(object root, string path, out object? value)
    {
        value = null;
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        // Support paths like "$.riskObject.riskObjectMotor.carVin"
        var p = path.Trim();
        if (p.StartsWith("$.", StringComparison.Ordinal))
        {
            p = p.Substring(2);
        }
        else if (p.StartsWith("$", StringComparison.Ordinal))
        {
            p = p.Substring(1);
        }

        var segments = p
            .Split('.', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrEmpty(s))
            .ToArray();

        if (segments.Length == 0)
        {
            return false;
        }

        var current = root;
        foreach (var seg in segments)
        {
            if (current == null)
            {
                return false;
            }

            var type = current.GetType();

            // Allow callers to include wrappers even when root is already nested.
            if (IsWrapperSegment(seg))
            {
                var maybeWrapper = type.GetProperty(seg,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                if (maybeWrapper == null)
                {
                    continue;
                }
            }

            var prop = type.GetProperty(seg, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
            if (prop == null)
            {
                // "car_vin" -> "CarVin"
                var pascal = ToPascalCase(seg);
                prop = type.GetProperty(pascal, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
            }

            if (prop == null)
            {
                return false;
            }

            current = prop.GetValue(current);
        }

        value = current;
        return true;
    }

    private static bool IsWrapperSegment(string seg)
    {
        return seg.Equals("riskObject", StringComparison.OrdinalIgnoreCase)
               || seg.Equals("riskObjectMotor", StringComparison.OrdinalIgnoreCase)
               || seg.Equals("riskMotor", StringComparison.OrdinalIgnoreCase)
               || seg.Equals("motor", StringComparison.OrdinalIgnoreCase);
    }

    private static string ConvertToString(object? value)
    {
        if (value == null)
        {
            return string.Empty;
        }

        if (value is string s)
        {
            var t = s.Trim();
            if (t.Equals("y", StringComparison.OrdinalIgnoreCase))
            {
                return "Y";
            }

            if (t.Equals("n", StringComparison.OrdinalIgnoreCase))
            {
                return "N";
            }

            return t;
        }

        if (value is DateTime dt)
        {
            return dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        if (value is DateTimeOffset dto)
        {
            return dto.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        if (value is decimal dec)
        {
            return dec.ToString(CultureInfo.InvariantCulture);
        }

        if (value is double d)
        {
            return d.ToString(CultureInfo.InvariantCulture);
        }

        if (value is float f)
        {
            return f.ToString(CultureInfo.InvariantCulture);
        }

        if (value is Guid g)
        {
            return g.ToString();
        }

        return value.ToString() ?? string.Empty;
    }

    private static string ToPascalCase(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        var parts = input
            .Split(new[] { '_', '-', ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrEmpty(s))
            .ToArray();

        if (parts.Length == 1)
        {
            // e.g. "carVin" -> "CarVin"
            var p = parts[0];
            return p.Length == 1
                ? char.ToUpperInvariant(p[0]).ToString()
                : char.ToUpperInvariant(p[0]) + p.Substring(1);
        }

        var sb = new StringBuilder();
        foreach (var part in parts)
        {
            if (part.Length == 0)
            {
                continue;
            }

            sb.Append(char.ToUpperInvariant(part[0]));
            if (part.Length > 1)
            {
                sb.Append(part.Substring(1));
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Escapes % and _ for use in ILike pattern so they are matched literally (PostgreSQL / EF.Functions.ILike).
    /// </summary>
    private static string EscapeForLike(string pattern)
    {
        if (string.IsNullOrEmpty(pattern))
            return pattern;
        return pattern
            .Replace("\\", "\\\\")
            .Replace("%", "\\%")
            .Replace("_", "\\_");
    }

    /// <summary>
    ///     Maps PolicyRiskMotor to PolicyRiskMotorDetailDto, resolving IDs from codes
    /// </summary>
    private async Task<PolicyRiskMotorDetailDto> MapRiskMotorDetailDtoAsync(PolicyRiskMotor riskMotor)
    {
        var dto = new PolicyRiskMotorDetailDto
        {
            Id = riskMotor.Id,
            RiskObjectValue = riskMotor.RiskObjectValue,
            MotorClassCode = riskMotor.MotorClassCode,
            CarLineCode = riskMotor.CarLineCode,
            CarGroupCode = riskMotor.CarGroupCode,
            CarTypeCode = riskMotor.CarTypeCode,
            CarBrandCode = riskMotor.CarBrandCode,
            CarModelCode = riskMotor.CarModelCode,
            CarCategoryCode = riskMotor.CarCategoryCode,
            CarUsage = riskMotor.CarUsage,
            CarOld = riskMotor.CarOld,
            CarProductionYear = riskMotor.CarProductionYear,
            CarPlate = riskMotor.CarPlate,
            CarPlateType = riskMotor.CarPlateType,
            CarPlateClear = riskMotor.CarPlateClear,
            CarSeatNumber = riskMotor.CarSeatNumber,
            CarVin = riskMotor.CarVin,
            CarEngineNumber = riskMotor.CarEngineNumber,
            CarPayloadCapacity = riskMotor.CarPayloadCapacity,
            CarColor = riskMotor.CarColor,
            CarOrigin = riskMotor.CarOrigin,
            CarNew = riskMotor.CarNew
        };

        // Resolve IDs from codes (for FE dropdown mapping)
        if (!string.IsNullOrWhiteSpace(riskMotor.MotorClassCode))
        {
            var motorClassQuery = await ResMotorClassRepository.GetQueryableAsync();
            var motorClass = await AsyncExecuter.FirstOrDefaultAsync(
                motorClassQuery.Where(x => x.Code == riskMotor.MotorClassCode)
            );
            dto.MotorClassId = motorClass?.Id;
        }

        if (!string.IsNullOrWhiteSpace(riskMotor.CarLineCode))
        {
            var carLineQuery = await ResCarLineRepository.GetQueryableAsync();
            var carLine = await AsyncExecuter.FirstOrDefaultAsync(
                carLineQuery.Where(x => x.Code == riskMotor.CarLineCode)
            );
            dto.CarLineId = carLine?.Id;
        }

        if (!string.IsNullOrWhiteSpace(riskMotor.CarGroupCode))
        {
            var carGroupQuery = await ResCarGroupRepository.GetQueryableAsync();
            var carGroup = await AsyncExecuter.FirstOrDefaultAsync(
                carGroupQuery.Where(x => x.Code == riskMotor.CarGroupCode)
            );
            dto.CarGroupId = carGroup?.Id;
        }

        if (!string.IsNullOrWhiteSpace(riskMotor.CarTypeCode))
        {
            var carTypeQuery = await ResCarTypeRepository.GetQueryableAsync();
            var carType = await AsyncExecuter.FirstOrDefaultAsync(
                carTypeQuery.Where(x => x.Code == riskMotor.CarTypeCode)
            );
            dto.CarTypeId = carType?.Id;
        }

        if (!string.IsNullOrWhiteSpace(riskMotor.CarBrandCode))
        {
            var carBrandQuery = await ResCarBrandRepository.GetQueryableAsync();
            var carBrand = await AsyncExecuter.FirstOrDefaultAsync(
                carBrandQuery.Where(x => x.Code == riskMotor.CarBrandCode)
            );
            dto.CarBrandId = carBrand?.Id;
        }

        if (!string.IsNullOrWhiteSpace(riskMotor.CarModelCode))
        {
            var carModelQuery = await ResCarModelRepository.GetQueryableAsync();
            var carModel = await AsyncExecuter.FirstOrDefaultAsync(
                carModelQuery.Where(x => x.Code == riskMotor.CarModelCode)
            );
            dto.CarModelId = carModel?.Id;
        }

        if (!string.IsNullOrWhiteSpace(riskMotor.CarCategoryCode))
        {
            var carCategoryQuery = await ResCarCategoryRepository.GetQueryableAsync();
            var carCategory = await AsyncExecuter.FirstOrDefaultAsync(
                carCategoryQuery.Where(x => x.Code == riskMotor.CarCategoryCode)
            );
            dto.CarCategoryId = carCategory?.Id;
        }

        return dto;
    }

    /// <summary>
    /// Apply sorting for policy list. Only allows: contractNo (Contract.Code), policyNo, orgEffectDate, orgExpireDate, creationTime.
    /// When no sort is specified, default is creation date descending (newest first).
    /// </summary>
    protected override IQueryable<iOne.Policies.Policy> ApplySorting(IQueryable<iOne.Policies.Policy> query, GetPoliciesInput input)
    {
        var sorting = input.Sorting?.Trim();
        if (string.IsNullOrWhiteSpace(sorting))
        {
            return query.OrderByDescending(x => x.CreationTime);
        }

        // Support "FieldName asc/desc" (from PrimeNG) or "-FieldName" (ABP convention)
        var isDesc = sorting.StartsWith("-");
        var parts = sorting.TrimStart('-').Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var property = parts.Length > 0 ? parts[0] : "policyNo";
        if (parts.Length > 1 && string.Equals(parts[1], "desc", StringComparison.OrdinalIgnoreCase))
        {
            isDesc = true;
        }

        return string.Equals(property, "contractNo", StringComparison.OrdinalIgnoreCase)
            ? (isDesc
                ? query.OrderByDescending(x => x.Contract != null ? (x.Contract.Code ?? "") : "")
                : query.OrderBy(x => x.Contract != null ? (x.Contract.Code ?? "") : ""))
            : string.Equals(property, "policyNo", StringComparison.OrdinalIgnoreCase)
                ? (isDesc ? query.OrderByDescending(x => x.PolicyNo) : query.OrderBy(x => x.PolicyNo))
                : string.Equals(property, "orgEffectDate", StringComparison.OrdinalIgnoreCase)
                    ? (isDesc ? query.OrderByDescending(x => x.OrgEffectDate) : query.OrderBy(x => x.OrgEffectDate))
                    : string.Equals(property, "orgExpireDate", StringComparison.OrdinalIgnoreCase)
                        ? (isDesc ? query.OrderByDescending(x => x.OrgExpireDate) : query.OrderBy(x => x.OrgExpireDate))
                        : string.Equals(property, "creationTime", StringComparison.OrdinalIgnoreCase)
                            ? (isDesc ? query.OrderByDescending(x => x.CreationTime) : query.OrderBy(x => x.CreationTime))
                            : query.OrderByDescending(x => x.CreationTime);
    }

    /// <summary>
    /// Gắn thêm vào <b>cuối</b> pipeline query (sau tất cả filter từ input): đơn mà Seller/Implementer là user,
    /// Nếu nhân viên thuộc đối tác (<c>PartnerId</c>): lọc theo <c>PartnerId</c> của <see cref="iOne.Policies.Policy.Implementer"/> (cùng đối tác kênh với viewer),
    /// không theo <see cref="iOne.Policies.Policy.PartnerId"/> trên đơn (BH gốc/insurer). Chi tiết: <see cref="PolicySearchQueryScope.ApplyEmployeeUnitSearchFilter"/>.
    /// Ngược lại: đồng phòng ban / cây phòng ban như <see cref="PolicySearchQueryScope"/>.
    /// </summary>
    protected virtual async Task<IQueryable<iOne.Policies.Policy>> ApplyPolicySearchScopeAsync(
        IQueryable<iOne.Policies.Policy> query)
    {
        if (CurrentUser.Id == null)
        {
            return query.Where(_ => false);
        }

        var employee = await HrEmployeeRepository.FirstOrDefaultAsync(e => e.UserId == CurrentUser.Id.Value);
        if (employee == null)
        {
            return query.Where(_ => false);
        }

        var managedSubtree = employee.PartnerId.HasValue
            ? null
            : await TryGetManagedSubtreeForViewerAsync(employee);
        LogPolicyEmployeeUnitSearchScope("ApplyPolicySearchScope(Policy)", employee, managedSubtree);
        return PolicySearchQueryScope.ApplyEmployeeUnitSearchFilter(
            query,
            employee.Id,
            employee.PartnerId,
            employee.DepartmentId,
            employee.IsManager == true,
            managedSubtree);
    }

    /// <summary>
    /// Cùng lọc phạm vi tìm kiếm cho query list theo <see cref="PolicyVersion"/>.
    /// </summary>
    protected virtual async Task<IQueryable<PolicyVersion>> ApplyPolicySearchScopeAsync(
        IQueryable<PolicyVersion> query)
    {
        if (CurrentUser.Id == null)
        {
            return query.Where(_ => false);
        }

        var employee = await HrEmployeeRepository.FirstOrDefaultAsync(e => e.UserId == CurrentUser.Id.Value);
        if (employee == null)
        {
            return query.Where(_ => false);
        }

        var managedSubtree = employee.PartnerId.HasValue
            ? null
            : await TryGetManagedSubtreeForViewerAsync(employee);
        LogPolicyEmployeeUnitSearchScope("ApplyPolicySearchScope(PolicyVersion)", employee, managedSubtree);
        return PolicySearchQueryScope.ApplyEmployeeUnitSearchFilter(
            query,
            employee.Id,
            employee.PartnerId,
            employee.DepartmentId,
            employee.IsManager == true,
            managedSubtree);
    }

    /// <summary>
    /// Ghi log để trace nhánh <see cref="PolicySearchQueryScope.ApplyEmployeeUnitSearchFilter"/> (Partner vs nội bộ, IsManager, cây phòng ban).
    /// </summary>
    private void LogPolicyEmployeeUnitSearchScope(string source, HrEmployee employee, IReadOnlyList<Guid>? managedSubtree)
    {
        var isManager = employee.IsManager == true;
        var subtreeRoot = employee.OrgId ?? employee.DepartmentId;
        var subtreeCount = managedSubtree?.Count ?? 0;

        string branch;
        if (employee.PartnerId.HasValue)
        {
            branch = isManager
                ? "partner_manager_all_policies_of_partner"
                : "partner_employee_self_seller_implementer";
        }
        else if (isManager && subtreeCount > 0)
        {
            branch = "internal_manager_subtree_departments";
        }
        else if (isManager)
        {
            branch = "internal_manager_but_subtree_empty_fallback_same_department";
        }
        else
        {
            branch = "internal_non_manager_same_department";
        }

        var subtreePreview = subtreeCount == 0
            ? "(none)"
            : string.Join(",", managedSubtree!.Take(12).Select(g => g.ToString("N")));

        Logger.LogInformation(
            "PolicyEmployeeUnitSearchScope Source={Source} Branch={Branch} UserId={UserId} EmployeeId={EmployeeId} PartnerId={PartnerId} DepartmentId={DepartmentId} OrgId={OrgId} IsManager={IsManager} SubtreeRootDepartmentId={SubtreeRoot} ManagedSubtreeCount={ManagedSubtreeCount} SubtreeDepartmentIdsPreview={SubtreePreview}",
            source,
            branch,
            CurrentUser.Id,
            employee.Id,
            employee.PartnerId,
            employee.DepartmentId,
            employee.OrgId,
            isManager,
            subtreeRoot,
            subtreeCount,
            subtreePreview);
    }

    /// <summary>
    /// Gốc cây: <see cref="HrEmployee.OrgId"/> nếu có, không thì <see cref="HrEmployee.DepartmentId"/>; BFS theo <see cref="HrDepartment.ParentId"/>.
    /// </summary>
    protected virtual async Task<List<Guid>> BuildManagedDepartmentSubtreeAsync(Guid rootDepartmentId)
    {
        if (rootDepartmentId == Guid.Empty)
        {
            Logger.LogInformation(
                "PolicyEmployeeUnitSearchScope BuildManagedDepartmentSubtree: RootId is empty — subtree will be empty.");
            return new List<Guid>();
        }

        var q = await HrDepartmentRepository.GetQueryableAsync();
        var rows = await AsyncExecuter.ToListAsync(
            q.Where(d => !d.IsDeleted).Select(d => new { d.Id, d.ParentId }));

        if (!rows.Any(r => r.Id == rootDepartmentId))
        {
            Logger.LogInformation(
                "PolicyEmployeeUnitSearchScope BuildManagedDepartmentSubtree: Root department not found in hr_department (missing or deleted). RootId={RootId}",
                rootDepartmentId);
            return new List<Guid>();
        }

        var childrenByParent = rows
            .Where(r => r.ParentId.HasValue)
            .GroupBy(r => r.ParentId!.Value)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Id).ToList());

        var result = new List<Guid>();
        var queue = new Queue<Guid>();
        queue.Enqueue(rootDepartmentId);
        while (queue.Count > 0)
        {
            var id = queue.Dequeue();
            result.Add(id);
            if (childrenByParent.TryGetValue(id, out var children))
            {
                foreach (var c in children)
                {
                    queue.Enqueue(c);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Khi <see cref="HrEmployee.IsManager"/> là true, trả về id đơn vị gốc + mọi đơn vị con trong <c>hr_department</c>,
    /// và luôn gồm <see cref="HrEmployee.DepartmentId"/> nếu chưa có trong cây (tránh lệch với nhánh same-department của nhân viên).
    /// </summary>
    protected virtual async Task<IReadOnlyList<Guid>?> TryGetManagedSubtreeForViewerAsync(HrEmployee employee)
    {
        if (employee.IsManager != true)
        {
            return null;
        }

        var root = employee.OrgId ?? employee.DepartmentId;
        var subtree = await BuildManagedDepartmentSubtreeAsync(root);
        if (subtree.Count == 0)
        {
            return null;
        }

        // DepartmentId có thể không nằm dưới OrgId trong cây ParentId (dữ liệu org vs phòng ban tách);
        // khi đó nhân viên vẫn thấy đơn theo DepartmentId còn quản lý chỉ match subtree → thiếu đơn đồng phòng.
        if (employee.DepartmentId != Guid.Empty && !subtree.Contains(employee.DepartmentId))
        {
            subtree = new List<Guid>(subtree) { employee.DepartmentId };
        }

        return subtree;
    }

    protected override async Task<IQueryable<iOne.Policies.Policy>> CreateFilteredQueryAsync(GetPoliciesInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();

        // Basic filters - Số đơn BH: match PolicyNo or InsurerPolicyNo (BH gốc)
        if (!string.IsNullOrWhiteSpace(input.PolicyNo))
        {
            var policyNoFilter = input.PolicyNo.Trim();
            // Use simple contains pattern: no escaping so that e.g. PO_000410 matches ( _ in ILike matches one char, so matches literal _ )
            var policyNoPattern = "%" + policyNoFilter + "%";
            query = query.Where(policy =>
                EF.Functions.ILike(policy.PolicyNo ?? string.Empty, policyNoPattern)
                || EF.Functions.ILike(policy.InsurerPolicyNo ?? string.Empty, policyNoPattern));
        }

        if (input.ContractId.HasValue)
        {
            query = query.Where(x => x.ContractId == input.ContractId.Value);
        }

        if (input.LobId.HasValue)
        {
            query = query.Where(x => x.LobId == input.LobId.Value);
        }

        if (input.PolicyTypeId.HasValue)
        {
            query = query.Where(x => x.PolicyTypeId == input.PolicyTypeId.Value);
        }

        if (input.PartnerId.HasValue)
        {
            query = query.Where(x => x.PartnerId == input.PartnerId.Value);
        }

        if (input.SellerId.HasValue)
        {
            query = query.Where(x => x.SellerId == input.SellerId.Value);
        }

        if (input.CurrencyId.HasValue)
        {
            query = query.Where(x => x.CurrencyId == input.CurrencyId.Value);
        }

        if (input.SellType.HasValue)
        {
            query = query.Where(x => x.SellType == input.SellType.Value);
        }

        if (input.Status.HasValue)
        {
            query = query.Where(x => x.Status == input.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(input.ApprovalStatus))
        {
            query = query.Where(x => x.ApprovalStatus == input.ApprovalStatus);
        }

        // New filters for wireframe implementation
        if (input.ChannelId.HasValue)
        {
            query = query.Where(x => x.ChannelId == input.ChannelId.Value);
        }

        // CustomerId filter - Query through PolicyContract -> ResCustomer relationship
        if (input.CustomerId.HasValue)
        {
            query = query.Where(x => x.ContractId != null &&
                                     x.Contract.CustomerId == input.CustomerId.Value);
        }

        // Contract Type filter - Query through PolicyContract (Type is enum PolicyContractType)
        if (!string.IsNullOrWhiteSpace(input.ContractType))
        {
            if (Enum.TryParse<PolicyContractType>(input.ContractType, out var contractType))
            {
                query = query.Where(x => x.ContractId != null &&
                                         x.Contract.Type == contractType);
            }
        }

        // Contract Status filter - Query through PolicyContract (Status is enum)
        if (!string.IsNullOrWhiteSpace(input.ContractStatus))
        {
            if (Enum.TryParse<PolicyContractStatus>(input.ContractStatus, out var contractStatus))
            {
                query = query.Where(x => x.ContractId != null &&
                                         x.Contract.Status == contractStatus);
            }
        }

        // Certificate Number filter - Query through PolicyVersion -> PolicyCertificate
        if (!string.IsNullOrWhiteSpace(input.CertificateNo))
        {
            query = query.Where(x => x.PolicyVersions.Any(v =>
                v.PolicyCertificates.Any(c =>
                    EF.Functions.ILike(c.CertificateNo ?? string.Empty, $"%{input.CertificateNo}%")
                )
            ));
        }

        // ImplementerId filter - Can be provided directly or via PolicyIssuerId (both map to ImplementerId)
        if (input.ImplementerId.HasValue || input.PolicyIssuerId.HasValue)
        {
            var implementerId = input.PolicyIssuerId ?? input.ImplementerId;
            query = query.Where(x => x.ImplementerId == implementerId.Value);
        }

        // Date range filters
        if (input.EffectiveDateFrom.HasValue)
        {
            query = query.Where(x => x.OrgEffectDate >= input.EffectiveDateFrom.Value);
        }

        if (input.EffectiveDateTo.HasValue)
        {
            // Use next day and < so that the selected "To" date is inclusive (whole day)
            var effectiveToExclusive = input.EffectiveDateTo.Value.Date.AddDays(1);
            query = query.Where(x => x.OrgEffectDate < effectiveToExclusive);
        }

        if (input.ExpiryDateFrom.HasValue)
        {
            query = query.Where(x => x.OrgExpireDate >= input.ExpiryDateFrom.Value);
        }

        if (input.ExpiryDateTo.HasValue)
        {
            // Use next day and < so that the selected "To" date is inclusive (whole day)
            var expiryToExclusive = input.ExpiryDateTo.Value.Date.AddDays(1);
            query = query.Where(x => x.OrgExpireDate < expiryToExclusive);
        }

        // Car Plate filter - Query via PolicyVersion->PolicyRiskObject or Policy->PolicyRiskObject (both paths to PolicyRiskMotors)
        if (!string.IsNullOrWhiteSpace(input.CarPlate))
        {
            var carPlateFilter = input.CarPlate.Trim();
            var carPlatePattern = "%" + EscapeForLike(carPlateFilter) + "%";
            query = query.Where(x =>
                x.PolicyVersions.Any(v =>
                    v.PolicyRiskObjects.Any(ro =>
                        ro.PolicyRiskMotors.Any(m =>
                            EF.Functions.ILike(m.CarPlate ?? string.Empty, carPlatePattern))))
                || x.PolicyRiskObjects.Any(ro =>
                    ro.PolicyRiskMotors.Any(m =>
                        EF.Functions.ILike(m.CarPlate ?? string.Empty, carPlatePattern))));
        }

        // Car VIN filter - Query through PolicyVersion -> PolicyRiskObject -> PolicyRiskMotors
        if (!string.IsNullOrWhiteSpace(input.CarVin))
        {
            query = query.Where(x => x.PolicyVersions.Any(v =>
                v.PolicyRiskObjects.Any(ro =>
                    ro.PolicyRiskMotors.Any(m =>
                        EF.Functions.ILike(m.CarVin ?? string.Empty, $"%{input.CarVin}%")
                    )
                )
            ));
        }

        // Car Engine Number filter - Query via PolicyVersion->PolicyRiskObject or Policy->PolicyRiskObject (both paths to PolicyRiskMotors)
        if (!string.IsNullOrWhiteSpace(input.CarEngineNumber))
        {
            var engineFilter = input.CarEngineNumber.Trim();
            query = query.Where(x =>
                x.PolicyVersions.Any(v =>
                    v.PolicyRiskObjects.Any(ro =>
                        ro.PolicyRiskMotors.Any(m =>
                            EF.Functions.ILike(m.CarEngineNumber ?? string.Empty, $"%{engineFilter}%")))
                ) ||
                x.PolicyRiskObjects.Any(ro =>
                    ro.PolicyRiskMotors.Any(m =>
                        EF.Functions.ILike(m.CarEngineNumber ?? string.Empty, $"%{engineFilter}%")))
            );
        }

        // Primary Insurance Partner filter - Query through PolicyContract.InsurerId
        // Where ResPartner has ResPartnerType.code = 'INSURER'
        // Note: The filtering by ResPartnerType.code = 'INSURER' should be done when loading the dropdown options
        if (input.PrimaryInsurancePartnerId.HasValue)
        {
            query = query.Where(x => x.ContractId != null &&
                                     x.Contract.InsurerId == input.PrimaryInsurancePartnerId.Value);
        }

        // Import Lot Number filter - Direct field on Policy
        if (!string.IsNullOrWhiteSpace(input.ImportLotNumber))
        {
            query = query.Where(x => EF.Functions.ILike(x.LotImportCode ?? string.Empty, $"%{input.ImportLotNumber}%"));
        }

        if (!string.IsNullOrWhiteSpace(input.PaymentStatus))
        {
            var amounts = await PolicyAmountRepository.GetQueryableAsync();
            var matchingAmounts = ApplyPaymentStatusFilterToAmountsQuery(amounts, input.PaymentStatus);
            query = query.Join(matchingAmounts, p => p.Id, a => a.PolicyId, (p, _) => p).Distinct();
        }

        // AND thêm: nhân viên đăng nhập = Seller hoặc Implementer (các rule tìm kiếm phía trên giữ nguyên)
        return await ApplyPolicySearchScopeAsync(query);
    }

    /// <summary>
    /// Lọc <see cref="PolicyAmount"/> theo <c>payment_status</c>: <c>new</c>, <c>paid</c>, <c>partial</c>
    /// (cộng thêm khớp <c>done</c>/<c>inprogress</c> nếu còn bản ghi thời kỳ đặt tên cũ).
    /// </summary>
    private static IQueryable<PolicyAmount> ApplyPaymentStatusFilterToAmountsQuery(
        IQueryable<PolicyAmount> amounts,
        string paymentStatusInput)
    {
        var p = paymentStatusInput.Trim().ToLowerInvariant();
        var baseQ = amounts.Where(a => !a.IsDeleted);
        return p switch
        {
            "new" => baseQ.Where(a => a.PaymentStatus.ToLower() == "new"),
            "paid" => baseQ.Where(a =>
                a.PaymentStatus.ToLower() == "paid" ||
                a.PaymentStatus.ToLower() == "done"),
            "partial" => baseQ.Where(a =>
                a.PaymentStatus.ToLower() == "partial" ||
                a.PaymentStatus.ToLower() == "inprogress"),
            "cancelled" => baseQ.Where(a => a.PaymentStatus.ToLower() == "cancelled"),
            _ => baseQ.Where(a => a.PaymentStatus.ToLower() == p)
        };
    }

    /// <summary>
    ///     Build filtered query for policy list when each record is per-version (PolicyVersion).
    ///     Same filters as CreateFilteredQueryAsync but applied on version and version.Policy.
    /// </summary>
    protected virtual async Task<IQueryable<PolicyVersion>> CreateFilteredVersionQueryAsync(GetPoliciesInput input)
    {
        var query = await PolicyVersionRepository.GetQueryableAsync();
        query = query.Where(v => !v.IsDeleted && v.Policy != null && !v.Policy.IsDeleted);

        if (!string.IsNullOrWhiteSpace(input.PolicyNo))
        {
            var policyNoFilter = input.PolicyNo.Trim();
            var policyNoPattern = "%" + policyNoFilter + "%";
            query = query.Where(v =>
                EF.Functions.ILike(v.Policy.PolicyNo ?? string.Empty, policyNoPattern)
                || EF.Functions.ILike(v.Policy.InsurerPolicyNo ?? string.Empty, policyNoPattern));
        }

        if (input.ContractId.HasValue)
            query = query.Where(v => v.Policy.ContractId == input.ContractId.Value);
        if (input.LobId.HasValue)
            query = query.Where(v => v.Policy.LobId == input.LobId.Value);
        if (input.PolicyTypeId.HasValue)
            query = query.Where(v => v.Policy.PolicyTypeId == input.PolicyTypeId.Value);
        if (input.PartnerId.HasValue)
            query = query.Where(v => v.Policy.PartnerId == input.PartnerId.Value);
        if (input.SellerId.HasValue)
            query = query.Where(v => v.Policy.SellerId == input.SellerId.Value);
        if (input.CurrencyId.HasValue)
            query = query.Where(v => v.Policy.CurrencyId == input.CurrencyId.Value);
        if (input.SellType.HasValue)
            query = query.Where(v => v.Policy.SellType == input.SellType.Value);
        if (input.Status.HasValue)
        {
            var statusStr = input.Status.Value.ToString().ToLowerInvariant();
            query = query.Where(v => EF.Functions.ILike(v.Status, statusStr));
        }
        if (!string.IsNullOrWhiteSpace(input.ApprovalStatus))
            query = query.Where(v => v.ApprovalStatus == input.ApprovalStatus);
        if (input.ChannelId.HasValue)
            query = query.Where(v => v.Policy.ChannelId == input.ChannelId.Value);

        if (input.CustomerId.HasValue)
            query = query.Where(v => v.Policy.ContractId != null && v.Policy.Contract != null && v.Policy.Contract.CustomerId == input.CustomerId.Value);
        if (!string.IsNullOrWhiteSpace(input.ContractType) && Enum.TryParse<PolicyContractType>(input.ContractType, out var contractType))
            query = query.Where(v => v.Policy.ContractId != null && v.Policy.Contract != null && v.Policy.Contract.Type == contractType);
        if (!string.IsNullOrWhiteSpace(input.ContractStatus) && Enum.TryParse<PolicyContractStatus>(input.ContractStatus, out var contractStatus))
            query = query.Where(v => v.Policy.ContractId != null && v.Policy.Contract != null && v.Policy.Contract.Status == contractStatus);

        if (!string.IsNullOrWhiteSpace(input.CertificateNo))
            query = query.Where(v => v.PolicyCertificates.Any(c =>
                EF.Functions.ILike(c.CertificateNo ?? string.Empty, $"%{input.CertificateNo}%")));

        if (input.ImplementerId.HasValue || input.PolicyIssuerId.HasValue)
        {
            var implementerId = input.PolicyIssuerId ?? input.ImplementerId;
            query = query.Where(v => v.Policy.ImplementerId == implementerId.Value);
        }

        if (input.EffectiveDateFrom.HasValue)
            query = query.Where(v => v.EffectDate >= input.EffectiveDateFrom.Value);
        if (input.EffectiveDateTo.HasValue)
        {
            var effectiveToExclusive = input.EffectiveDateTo.Value.Date.AddDays(1);
            query = query.Where(v => v.EffectDate < effectiveToExclusive);
        }
        if (input.ExpiryDateFrom.HasValue)
            query = query.Where(v => v.ExpireDate >= input.ExpiryDateFrom.Value);
        if (input.ExpiryDateTo.HasValue)
        {
            var expiryToExclusive = input.ExpiryDateTo.Value.Date.AddDays(1);
            query = query.Where(v => v.ExpireDate < expiryToExclusive);
        }

        if (!string.IsNullOrWhiteSpace(input.CarPlate))
        {
            var carPlatePattern = "%" + EscapeForLike(input.CarPlate.Trim()) + "%";
            query = query.Where(v => v.PolicyRiskObjects.Any(ro =>
                ro.PolicyRiskMotors.Any(m => EF.Functions.ILike(m.CarPlate ?? string.Empty, carPlatePattern))));
        }
        if (!string.IsNullOrWhiteSpace(input.CarVin))
            query = query.Where(v => v.PolicyRiskObjects.Any(ro =>
                ro.PolicyRiskMotors.Any(m => EF.Functions.ILike(m.CarVin ?? string.Empty, $"%{input.CarVin}%"))));
        if (!string.IsNullOrWhiteSpace(input.CarEngineNumber))
            query = query.Where(v => v.PolicyRiskObjects.Any(ro =>
                ro.PolicyRiskMotors.Any(m => EF.Functions.ILike(m.CarEngineNumber ?? string.Empty, $"%{input.CarEngineNumber}%"))));

        if (input.PrimaryInsurancePartnerId.HasValue)
            query = query.Where(v => v.Policy.ContractId != null && v.Policy.Contract != null && v.Policy.Contract.InsurerId == input.PrimaryInsurancePartnerId.Value);
        if (!string.IsNullOrWhiteSpace(input.ImportLotNumber))
            query = query.Where(v => EF.Functions.ILike(v.Policy.LotImportCode ?? string.Empty, $"%{input.ImportLotNumber}%"));

        if (!string.IsNullOrWhiteSpace(input.PaymentStatus))
        {
            var amounts = await PolicyAmountRepository.GetQueryableAsync();
            var matchingAmounts = ApplyPaymentStatusFilterToAmountsQuery(amounts, input.PaymentStatus);
            query = query.Join(matchingAmounts, v => v.Id, a => a.PolicyVersionId, (v, _) => v).Distinct();
        }

        // AND thêm: nhân viên đăng nhập = Seller hoặc Implementer trên policy
        return await ApplyPolicySearchScopeAsync(query);
    }

    /// <summary>
    ///     Apply sorting for version-based policy list. Sorts by policy/version fields.
    /// </summary>
    protected virtual IQueryable<PolicyVersion> ApplyVersionSorting(IQueryable<PolicyVersion> query, GetPoliciesInput input)
    {
        var sorting = input.Sorting?.Trim();
        if (string.IsNullOrWhiteSpace(sorting))
            return query.OrderByDescending(v => v.Policy.CreationTime).ThenByDescending(v => v.Version);

        var isDesc = sorting.StartsWith("-");
        var parts = sorting.TrimStart('-').Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var property = parts.Length > 0 ? parts[0] : "creationTime";
        if (parts.Length > 1 && string.Equals(parts[1], "desc", StringComparison.OrdinalIgnoreCase))
            isDesc = true;

        return string.Equals(property, "contractNo", StringComparison.OrdinalIgnoreCase)
            ? (isDesc
                ? query.OrderByDescending(v => v.Policy.Contract != null ? (v.Policy.Contract.Code ?? "") : "")
                : query.OrderBy(v => v.Policy.Contract != null ? (v.Policy.Contract.Code ?? "") : ""))
            : string.Equals(property, "policyNo", StringComparison.OrdinalIgnoreCase)
                ? (isDesc ? query.OrderByDescending(v => v.Policy.PolicyNo) : query.OrderBy(v => v.Policy.PolicyNo))
                : string.Equals(property, "orgEffectDate", StringComparison.OrdinalIgnoreCase)
                    ? (isDesc ? query.OrderByDescending(v => v.EffectDate) : query.OrderBy(v => v.EffectDate))
                    : string.Equals(property, "orgExpireDate", StringComparison.OrdinalIgnoreCase)
                        ? (isDesc ? query.OrderByDescending(v => v.ExpireDate) : query.OrderBy(v => v.ExpireDate))
                        : string.Equals(property, "version", StringComparison.OrdinalIgnoreCase)
                            ? (isDesc ? query.OrderByDescending(v => v.Version) : query.OrderBy(v => v.Version))
                            : string.Equals(property, "creationTime", StringComparison.OrdinalIgnoreCase)
                                ? (isDesc ? query.OrderByDescending(v => v.Policy.CreationTime) : query.OrderBy(v => v.Policy.CreationTime))
                                : query.OrderByDescending(v => v.Policy.CreationTime).ThenByDescending(v => v.Version);
    }

    /// <summary>
    ///     Get next generated code from ResSequence (bypasses app-service authorization).
    ///     Logic is aligned with Master.ResSequenceAppService.GetNextSequenceAsync.
    /// </summary>
    private async Task<string> GetNextSequenceCodeAsync(string code, Dictionary<string, string>? parameters = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new UserFriendlyException(
                L["ResSequence:CodeOrIdRequired"].Value
            );
        }

        var query = await ResSequenceRepository.GetQueryableAsync();
        var sequence = await query.FirstOrDefaultAsync(x => x.Code == code);
        if (sequence == null)
        {
            throw new UserFriendlyException(
                L["ResSequence:NotFound"].Value
            );
        }

        if (sequence.Status != ResSequenceStatus.Active)
        {
            throw new UserFriendlyException(
                L["ResSequence:NotActive"].Value
            );
        }

        // Reload to get latest NumberNext (important for concurrent requests)
        var freshSequence = await ResSequenceRepository.GetAsync(sequence.Id);
        if (freshSequence.Status != ResSequenceStatus.Active)
        {
            throw new UserFriendlyException(
                L["ResSequence:NotActive"].Value
            );
        }

        var currentNumber = freshSequence.NumberNext;
        var shouldReset = false;

        if (freshSequence.UseDateRange == ResSequenceUseDateRange.Yes && freshSequence.DateRangeType.HasValue)
        {
            shouldReset = ShouldResetSequence(freshSequence);
            if (shouldReset)
            {
                currentNumber = 0; // Reset to 0, will be incremented below
            }
        }

        var generatedCode = GenerateSequenceCode(freshSequence, currentNumber, parameters);

        var nextNumber = shouldReset
            ? freshSequence.NumberIncrement
            : currentNumber + freshSequence.NumberIncrement;

        freshSequence.UpdateNumberNext(nextNumber);
        await ResSequenceRepository.UpdateAsync(freshSequence);
        await CurrentUnitOfWork.SaveChangesAsync();

        return generatedCode;
    }

    /// <summary>
    ///     Giống logic sinh mã trong <c>ResCustomerAppService.CreateAsync</c> khi <c>Code</c> để trống (CUSTOMER_SEQ + format 4 ký tự đầu + phần số padded 7).
    /// </summary>
    private async Task<string> GenerateNewCustomerCodeForImportAsync()
    {
        var generatedCode = await GetNextSequenceCodeAsync(CustomerCodeSequenceCode);
        if (!string.IsNullOrWhiteSpace(generatedCode) && generatedCode.Length > 4)
        {
            var prefix = generatedCode.Substring(0, 4);
            var numberPart = generatedCode.Substring(4);
            if (int.TryParse(numberPart, out var number))
            {
                return $"{prefix}{number:D7}";
            }

            return generatedCode;
        }

        return generatedCode ?? string.Empty;
    }

    private bool ShouldResetSequence(ResSequence sequence)
    {
        if (sequence.UseDateRange != ResSequenceUseDateRange.Yes || !sequence.DateRangeType.HasValue)
        {
            return false;
        }

        var now = Clock.Now;
        var lastModified = sequence.LastModificationTime ?? sequence.CreationTime;

        switch (sequence.DateRangeType.Value)
        {
            case ResSequenceDateRangeType.Week:
                {
                    var currentWeek = GetWeekOfYear(now);
                    var lastWeek = GetWeekOfYear(lastModified);
                    return currentWeek != lastWeek || now.Year != lastModified.Year;
                }
            case ResSequenceDateRangeType.Month:
                return now.Year != lastModified.Year || now.Month != lastModified.Month;
            case ResSequenceDateRangeType.Quarter:
                {
                    var currentQuarter = (now.Month - 1) / 3 + 1;
                    var lastQuarter = (lastModified.Month - 1) / 3 + 1;
                    return now.Year != lastModified.Year || currentQuarter != lastQuarter;
                }
            case ResSequenceDateRangeType.Half:
                {
                    var currentHalf = now.Month <= 6 ? 1 : 2;
                    var lastHalf = lastModified.Month <= 6 ? 1 : 2;
                    return now.Year != lastModified.Year || currentHalf != lastHalf;
                }
            case ResSequenceDateRangeType.Year:
                return now.Year != lastModified.Year;
            default:
                return false;
        }
    }

    private int GetWeekOfYear(DateTime date)
    {
        var calendar = CultureInfo.CurrentCulture.Calendar;
        return calendar.GetWeekOfYear(date, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
    }

    private string GenerateSequenceCode(ResSequence sequence, long number,
        Dictionary<string, string>? parameters = null)
    {
        var sb = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(sequence.Prefix))
        {
            var prefix = ProcessTemplate(sequence.Prefix, parameters);
            sb.Append(prefix);
        }

        var numberStr = number.ToString();
        if (sequence.Padding.HasValue && sequence.Padding.Value > 0)
        {
            numberStr = numberStr.PadLeft(sequence.Padding.Value, '0');
        }

        sb.Append(numberStr);

        if (!string.IsNullOrWhiteSpace(sequence.Suffix))
        {
            var suffix = ProcessTemplate(sequence.Suffix, parameters);
            sb.Append(suffix);
        }

        return sb.ToString();
    }

    private string ProcessTemplate(string template, Dictionary<string, string>? parameters = null)
    {
        if (string.IsNullOrWhiteSpace(template))
        {
            return template;
        }

        var result = template;
        var now = Clock.Now;

        // $date(format)
        result = Regex.Replace(result, @"\$date\(([^)]+)\)", match =>
        {
            var format = match.Groups[1].Value;
            try
            {
                return now.ToString(format);
            }
            catch
            {
                return match.Value;
            }
        });

        // ${date}(format)
        result = Regex.Replace(result, @"\$\{date\}\(([^)]+)\)", match =>
        {
            var format = match.Groups[1].Value;
            try
            {
                return now.ToString(format);
            }
            catch
            {
                return match.Value;
            }
        });

        // ${param_name}
        if (parameters != null && parameters.Count > 0)
        {
            result = Regex.Replace(result, @"\$\{([^}]+)\}", match =>
            {
                var paramName = match.Groups[1].Value;
                if (paramName == "date")
                {
                    return match.Value;
                }

                if (parameters.TryGetValue(paramName, out var paramValue))
                {
                    return paramValue ?? string.Empty;
                }

                return match.Value;
            });
        }

        return result;
    }

    #region Policy Termination

    /// <summary>
    ///     Gets policies for contract termination modal (active policies with refund amounts).
    /// </summary>
    public virtual async Task<List<ContractTerminationPolicyDto>> GetContractTerminationPoliciesAsync(Guid contractId)
    {
        var now = Clock.Now.Date;
        var query = await Repository.GetQueryableAsync();
        query = query
            .Include(p => p.PolicyVersions)
            .ThenInclude(v => v.PolicyProducts)
            .ThenInclude(pp => pp.PolicyCoverages)
            .Include(p => p.PolicyVersions)
            .ThenInclude(v => v.PolicyRiskObjects)
            .ThenInclude(ro => ro.PolicyRiskMotors)
            .Include(p => p.PolicyVersions)
            .ThenInclude(v => v.PolicyCertificates)
            .Where(p => p.ContractId == contractId
                && p.Status == PolicyStatus.Active
                && !p.IsDeleted
                && p.OrgEffectDate <= now
                && p.OrgExpireDate >= now);

        var policies = await AsyncExecuter.ToListAsync(query);
        var result = new List<ContractTerminationPolicyDto>();

        var productIds = policies
            .SelectMany(p => p.PolicyVersions
                .Where(v => v.Id == p.LastVersionId)
                .SelectMany(v => v.PolicyProducts.Select(pp => pp.ProductId)))
            .Distinct()
            .ToList();

        var productQuery = await ProProductRepository.GetQueryableAsync();
        var products = await AsyncExecuter.ToListAsync(
            productQuery.Where(x => productIds.Contains(x.Id)));
        var productById = products.ToDictionary(p => p.Id, p => p);

        foreach (var policy in policies)
        {
            var currentVersion = policy.PolicyVersions
                .FirstOrDefault(v => v.Id == policy.LastVersionId && !v.IsDeleted);
            if (currentVersion == null) continue;

            var productNames = currentVersion.PolicyProducts
                .Where(pp => !pp.IsDeleted && productById.TryGetValue(pp.ProductId, out var prod))
                .Select(pp => productById[pp.ProductId].Name ?? string.Empty)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .ToList();
            var productsStr = productNames.Count > 0 ? string.Join("; ", productNames) : null;

            var certificate = currentVersion.PolicyCertificates
                .FirstOrDefault(c => !c.IsDeleted);
            var certificateNo = certificate?.CertificateNo;

            var effectDateStr = currentVersion.EffectDate.ToString("HH:mm dd/MM/yyyy", CultureInfo.InvariantCulture);
            var expireDateStr = currentVersion.ExpireDate.ToString("HH:mm dd/MM/yyyy", CultureInfo.InvariantCulture);

            decimal refundAmount = 0;
            var firstProduct = currentVersion.PolicyProducts
                .Where(pp => !pp.IsDeleted)
                .FirstOrDefault();
            if (firstProduct != null && productById.TryGetValue(firstProduct.ProductId, out var prod))
            {
                var productCode = prod.Code;
                if (!string.IsNullOrWhiteSpace(productCode))
                {
                    var riskMotor = currentVersion.PolicyRiskObjects
                        .SelectMany(ro => ro.PolicyRiskMotors)
                        .FirstOrDefault(rm => !rm.IsDeleted);
                    var attributes = new Dictionary<string, object?>();
                    if (riskMotor != null)
                    {
                        attributes["carPlate"] = riskMotor.CarPlate ?? "";
                        attributes["carAge"] = riskMotor.CarOld ?? 0;
                        attributes["carGroup"] = riskMotor.CarGroupCode ?? "";
                        attributes["carUsage"] = riskMotor.CarUsage ?? "";
                        attributes["carValue"] = riskMotor.RiskObjectValue ?? 0;
                        attributes["seat"] = riskMotor.CarSeatNumber?.ToString() ?? "";
                        attributes["payload"] = riskMotor.CarPayloadCapacity?.ToString() ?? "";
                    }

                    var productCoverages = firstProduct.PolicyCoverages
                        .Where(pc => !pc.IsDeleted)
                        .Select(pc => new CalculateRefundCoverageInput
                        {
                            ProductCoverageId = pc.Id.ToString(),
                            CoverageId = pc.CoverageId,
                            AmountLiability = pc.AmountLiability,
                            Quantity = (int)(pc.Quantity),
                            Premium = pc.Premium,
                            PremiumVAT = pc.Vat
                        })
                        .ToList();

                    try
                    {
                        var calcInput = new CalculateRefundAmountInput
                        {
                            ProductId = productCode,
                            Attributes = attributes,
                            PolicyId = policy.Id,
                            PolicyEffectDate = currentVersion.OrgEffectDate,
                            PolicyExpireDate = currentVersion.OrgExpireDate,
                            TerminationDate = now,
                            ProductCoverages = productCoverages,
                            TotalPremium = firstProduct.Premium,
                            TotalPremiumVAT = firstProduct.Vat
                        };
                        var calcResult = await CalculateRefundAmountAsync(calcInput);
                        refundAmount = calcResult.TotalRefund;
                    }
                    catch
                    {
                        refundAmount = 0;
                    }
                }
            }

            result.Add(new ContractTerminationPolicyDto
            {
                PolicyId = policy.Id,
                PolicyNo = policy.PolicyNo ?? "",
                CertificateNo = certificateNo,
                Products = productsStr,
                EffectDate = effectDateStr,
                ExpireDate = expireDateStr,
                PremiumTotal = currentVersion.PremiumTotal,
                RefundAmount = refundAmount,
                ActualRefundAmount = refundAmount
            });
        }

        return result;
    }

    /// <summary>
    ///     Calculates refund amounts for policy termination using the product's rule script.
    /// </summary>
    public virtual async Task<CalculateRefundAmountResultDto> CalculateRefundAmountAsync(
        CalculateRefundAmountInput input)
    {
        if (input == null)
        {
            throw new UserFriendlyException("Input cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(input.ProductId))
        {
            throw new UserFriendlyException("ProductId is required.");
        }

        // Override totals from PolicyAmount (sum per policy)
        var policyAmounts = await PolicyAmountRepository.GetListAsync(x => x.PolicyId == input.PolicyId && !x.IsDeleted);
        input.TotalPremium = policyAmounts.Sum(pa => pa.Amount);
        input.TotalPremiumVAT = policyAmounts.Sum(pa => pa.AmountTotal);

        // Override effect/expire dates from all policy versions (min effect, max expire)
        var policyVersions = await PolicyVersionRepository.GetListAsync(x => x.PolicyId == input.PolicyId && !x.IsDeleted);
        if (policyVersions.Count > 0)
        {
            input.PolicyEffectDate = policyVersions.Min(v => v.OrgEffectDate);
            input.PolicyExpireDate = policyVersions.Max(v => v.OrgExpireDate);
        }

        // 1. Get Product by code
        var productQuery = await ProProductRepository.GetQueryableAsync();
        var product = await AsyncExecuter.FirstOrDefaultAsync(
            productQuery.Where(x => x.Code != null && x.Code.ToUpper() == input.ProductId.ToUpperInvariant()));

        if (product == null)
        {
            throw new UserFriendlyException($"Product not found: {input.ProductId}");
        }

        // 2. Get ProRule for this product (ApplyTo = "product", ApplyToId = product.Id, Type = TERMINATE_REFUND_POLICY, Status = Active)
        var ruleQuery = await ProRuleRepository.GetQueryableAsync();
        var rule = await AsyncExecuter.FirstOrDefaultAsync(
            ruleQuery.Where(x =>
                x.ApplyTo == "product" &&
                x.ApplyToId == product.Id &&
                x.RuleType != null &&
                x.RuleType.Code == "TERMINATE_REFUND_POLICY" &&
                x.Status == ProRuleStatus.Active));

        if (rule == null || string.IsNullOrWhiteSpace(rule.RuleScript))
        {
            throw new UserFriendlyException($"No active rule script found for product: {input.ProductId}");
        }

        // 3. Build Python payload and execute script
        var totalRefund = await ExecuteRefundRuleScriptAsync(input, rule.RuleScript);

        // 4. Distribute refund to coverages based on premium ratio
        var coverageRefunds = DistributeRefundToCoverages(input.ProductCoverages, input.TotalPremiumVAT, totalRefund);

        return new CalculateRefundAmountResultDto
        {
            TotalRefund = totalRefund,
            CoverageRefunds = coverageRefunds
        };
    }

    public virtual async Task<CalculateRefundAmountResultDto> CalculateRefundAmountBatchAsync(
        CalculateRefundAmountBatchInput input)
    {
        if (input == null)
        {
            throw new UserFriendlyException("Input cannot be null.");
        }

        if (input.Products == null || input.Products.Count == 0)
        {
            return new CalculateRefundAmountResultDto
            {
                TotalRefund = 0,
                CoverageRefunds = new List<CoverageRefundDto>()
            };
        }

        // Load policy versions once to get shared effect/expire dates (min effect, max expire)
        var policyVersions = await PolicyVersionRepository.GetListAsync(
            x => x.PolicyId == input.PolicyId && !x.IsDeleted);

        var sharedEffectDate = policyVersions.Count > 0
            ? policyVersions.Min(v => v.OrgEffectDate)
            : DateTime.UtcNow;

        var sharedExpireDate = policyVersions.Count > 0
            ? policyVersions.Max(v => v.OrgExpireDate)
            : DateTime.UtcNow;

        // Constrain termination to the shared policy period
        var effectiveTerminationDate = input.TerminationDate;
        if (effectiveTerminationDate < sharedEffectDate)
            effectiveTerminationDate = sharedEffectDate;
        else if (effectiveTerminationDate > sharedExpireDate)
            effectiveTerminationDate = sharedExpireDate;

        decimal totalRefund = 0;
        var allCoverageRefunds = new List<CoverageRefundDto>();

        foreach (var productInput in input.Products)
        {
            if (string.IsNullOrWhiteSpace(productInput.ProductId))
            {
                throw new UserFriendlyException("ProductId is required for all products in the batch.");
            }

            // Build a single-product input reusing the shared dates and attributes
            var singleInput = new CalculateRefundAmountInput
            {
                ProductId = productInput.ProductId,
                Attributes = input.Attributes ?? new Dictionary<string, object?>(),
                PolicyId = input.PolicyId,
                PolicyEffectDate = sharedEffectDate,
                PolicyExpireDate = sharedExpireDate,
                TerminationDate = effectiveTerminationDate,
                ProductCoverages = productInput.ProductCoverages ?? new List<CalculateRefundCoverageInput>(),
                TotalPremium = productInput.TotalPremium,
                TotalPremiumVAT = productInput.TotalPremiumVAT
            };

            // Get Product by code
            var productQuery = await ProProductRepository.GetQueryableAsync();
            var product = await AsyncExecuter.FirstOrDefaultAsync(
                productQuery.Where(x => x.Code != null &&
                    x.Code.ToUpper() == singleInput.ProductId.ToUpperInvariant()));

            if (product == null)
            {
                throw new UserFriendlyException($"Product not found: {productInput.ProductId}");
            }

            // Get ProRule for this product
            var ruleQuery = await ProRuleRepository.GetQueryableAsync();
            var rule = await AsyncExecuter.FirstOrDefaultAsync(
                ruleQuery.Where(x =>
                    x.ApplyTo == "product" &&
                    x.ApplyToId == product.Id &&
                    x.RuleType != null &&
                    x.RuleType.Code == "TERMINATE_REFUND_POLICY" &&
                    x.Status == ProRuleStatus.Active));

            if (rule == null || string.IsNullOrWhiteSpace(rule.RuleScript))
            {
                throw new UserFriendlyException($"No active rule script found for product: {productInput.ProductId}");
            }

            // Execute Python rule script for this product
            var productRefund = await ExecuteRefundRuleScriptAsync(singleInput, rule.RuleScript);

            // Distribute refund to this product's coverages independently
            var coverageRefunds = DistributeRefundToCoverages(
                singleInput.ProductCoverages, singleInput.TotalPremiumVAT, productRefund);

            totalRefund += productRefund;
            allCoverageRefunds.AddRange(coverageRefunds);
        }

        return new CalculateRefundAmountResultDto
        {
            TotalRefund = totalRefund,
            CoverageRefunds = allCoverageRefunds
        };
    }

    private async Task<decimal> ExecuteRefundRuleScriptAsync(CalculateRefundAmountInput input, string ruleScript)
    {
        // Build payload for Python script
        var payload = new
        {
            productId = input.ProductId,
            attributes = input.Attributes ?? new Dictionary<string, object?>(),
            policyId = input.PolicyId.ToString(),
            policyEffectDate = input.PolicyEffectDate.ToString("yyyy-MM-dd"),
            policyExpireDate = input.PolicyExpireDate.ToString("yyyy-MM-dd"),
            terminationDate = input.TerminationDate.ToString("yyyy-MM-dd"),
            productCoverages = (input.ProductCoverages ?? new List<CalculateRefundCoverageInput>())
                .Select(c => new
                {
                    productCoverageId = c.ProductCoverageId,
                    coverageId = c.CoverageId.ToString(),
                    amountLiability = c.AmountLiability,
                    quantity = c.Quantity,
                    premium = c.Premium,
                    premiumVAT = c.PremiumVAT
                })
                .ToList(),
            totalPremium = input.TotalPremium,
            totalPremiumVAT = input.TotalPremiumVAT
        };

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });

        var b64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

        // Python program that executes the rule script and calculates refund
        var pythonProgram = $@"
import base64
import json
import sys
from datetime import datetime, date

payload = json.loads(base64.b64decode(sys.argv[1]).decode('utf-8'))

# Extract payload data
productId = payload.get('productId')
attributes = payload.get('attributes') or {{}}
policyId = payload.get('policyId')
policyEffectDate = datetime.strptime(payload.get('policyEffectDate'), '%Y-%m-%d').date()
policyExpireDate = datetime.strptime(payload.get('policyExpireDate'), '%Y-%m-%d').date()
terminationDate = datetime.strptime(payload.get('terminationDate'), '%Y-%m-%d').date()
productCoverages = payload.get('productCoverages') or []
totalPremium = float(payload.get('totalPremium') or 0)
totalPremiumVAT = float(payload.get('totalPremiumVAT') or 0)

# Calculate days used and total days
totalDays = (policyExpireDate - policyEffectDate).days
usedDays = (terminationDate - policyEffectDate).days
remainingDays = totalDays - usedDays

# Default refund calculation (pro-rata)
# totalRefund will be negative (money to refund to customer)
if totalDays > 0 and remainingDays > 0:
    totalRefund = -1 * (totalPremiumVAT) * remainingDays / totalDays
else:
    totalRefund = 0

# Execute the rule script which may override totalRefund
try:
    ruleScript = '''{ruleScript.Replace("'", "\\'")}'''
    
    local_ctx = {{
        'productId': productId,
        'attributes': attributes,
        'policyId': policyId,
        'policyEffectDate': policyEffectDate,
        'policyExpireDate': policyExpireDate,
        'terminationDate': terminationDate,
        'productCoverages': productCoverages,
        'totalPremium': totalPremium,
        'totalPremiumVAT': totalPremiumVAT,
        'totalDays': totalDays,
        'usedDays' : usedDays,
        'remainingDays' : remainingDays,
        'totalRefund' : totalRefund
    }}
    # Expose datetime/date so rule scripts can use them without importing (exec() uses empty glob_ctx by default)
    glob_ctx = {{'datetime': datetime, 'date': date}}
    
    if ruleScript.strip():
        try:
            result = eval(ruleScript, glob_ctx, local_ctx)
            if result is not None:
                totalRefund = float(result)
        except SyntaxError:
            exec(ruleScript, glob_ctx, local_ctx)
            if 'totalRefund' in local_ctx:
                totalRefund = float(local_ctx['totalRefund'])
except Exception as e:
    sys.stderr.write(f'Rule script error: {{e}}\\n')
    sys.exit(2)

# Output the result
print(json.dumps({{'totalRefund': totalRefund}}, ensure_ascii=False))
";

        var stdout = await RunPythonAsync(b64, pythonProgram);

        using var doc = JsonDocument.Parse(stdout);
        if (doc.RootElement.TryGetProperty("totalRefund", out var refundElement))
        {
            return refundElement.GetDecimal();
        }

        return 0m;
    }

    private static List<CoverageRefundDto> DistributeRefundToCoverages(
        List<CalculateRefundCoverageInput>? coverages,
        decimal totalPremiumVAT,
        decimal totalRefund)
    {
        var result = new List<CoverageRefundDto>();

        if (coverages == null || coverages.Count == 0)
        {
            return result;
        }

        if (totalPremiumVAT == 0)
        {
            var equalShare = totalRefund / coverages.Count;
            foreach (var coverage in coverages)
            {
                result.Add(new CoverageRefundDto
                {
                    CoverageId = coverage.CoverageId,
                    RefundAmount = Math.Round(equalShare, 2)
                });
            }
        }
        else
        {
            foreach (var coverage in coverages)
            {
                var ratio = (coverage.Premium + coverage.PremiumVAT) / totalPremiumVAT;
                var refundAmount = totalRefund * ratio;
                result.Add(new CoverageRefundDto
                {
                    CoverageId = coverage.CoverageId,
                    RefundAmount = Math.Round(refundAmount, 2)
                });
            }
        }

        return result;
    }

    /// <summary>
    ///     Distributes a total refund amount across coverages using the same logic as DistributeRefundToCoverages
    ///     (by premium ratio, or equal share if totalPremiumVAT is zero).
    /// </summary>
    public virtual Task<CalculateRefundAmountResultDto> DistributeRefundToCoveragesAsync(DistributeRefundInput input)
    {
        if (input == null)
        {
            throw new UserFriendlyException("Input cannot be null.");
        }

        var coverages = input.Coverages ?? new List<CalculateRefundCoverageInput>();
        var totalPremiumVAT = coverages.Sum(c => c.Premium + c.PremiumVAT);
        var distributed = DistributeRefundToCoverages(coverages, totalPremiumVAT, input.TotalRefund);

        var result = new CalculateRefundAmountResultDto
        {
            TotalRefund = input.TotalRefund,
            CoverageRefunds = distributed
        };
        return Task.FromResult(result);
    }

    /// <summary>
    ///     Terminates a policy by triggering the Elsa termination workflow.
    ///     PolicyAmount creation and policy status update are done by the workflow (e.g. via create-terminate-policy-amount and update-status APIs).
    /// </summary>
    public virtual async Task TerminatePolicyAsync(Guid id, TerminatePolicyInput input)
    {
        if (input == null)
        {
            throw new UserFriendlyException("Input cannot be null.");
        }

        var policyQuery = await Repository.GetQueryableAsync();
        var policy = await AsyncExecuter.FirstOrDefaultAsync(policyQuery.Where(x => x.Id == id));

        if (policy == null)
        {
            throw new UserFriendlyException($"Policy not found: {id}");
        }

        var policyVersion = await PolicyVersionRepository.GetAsync(policy.LastVersionId);
        policyVersion.UpdateTerminationStatus(PolicyTerminationStatus.Pending);
        policyVersion.UpdateRefundAmount(input.TotalRefundAmount);
        policyVersion.UpdateInternalNote(input.InternalNote);
        policyVersion.UpdateCustomerNote(input.CustomerNote);
        policy.UpdateTerminationReasonId(input.TerminationReasonId);
        policy.UpdateTerminationReasonDescription(input.TerminationReasonDescription);
        policy.UpdateTerminationDate(input.TerminationDate);

        if (input.Documents != null)
        {
            var desiredDocumentIds = input.Documents
                .Select(d => d.DocumentId)
                .Distinct()
                .ToHashSet();

            var existingDocs = await PolicyDocumentRepository.GetListAsync(x => x.PolicyId == policy.Id);
            var existingDocIds = existingDocs
                .Where(d => d.DocumentId.HasValue)
                .Select(d => d.DocumentId!.Value)
                .ToHashSet();

            foreach (var docId in desiredDocumentIds)
            {
                if (!existingDocIds.Contains(docId))
                {
                    var docEntity = new PolicyDocument(GuidGenerator.Create(), policy.Id, docId);
                    await PolicyDocumentManager.CreateAsync(docEntity);
                }
            }

            foreach (var doc in existingDocs)
            {
                if (doc.DocumentId.HasValue && !desiredDocumentIds.Contains(doc.DocumentId.Value))
                {
                    await PolicyDocumentRepository.DeleteAsync(doc);
                }
            }
        }

        await PolicyVersionRepository.UpdateAsync(policyVersion);
        await PolicyRepository.UpdateAsync(policy);

        await _elsaWorkflowService.InitTerminatePolicyRequestAsync(policy.LastVersionId, input.TerminationDate, input.TotalRefundAmount);
    }

    /// <summary>
    ///     Creates a PolicyAmount record for policy termination (fee item TERMINATE_REFUND_AMOUNT).
    ///     Called by the termination workflow; does not update policy status.
    /// </summary>
    public virtual async Task CreateTerminatePolicyAmountAsync(CreateTerminatePolicyAmountInput input)
    {
        if (input == null)
        {
            throw new UserFriendlyException("Input cannot be null.");
        }

        var versionQuery = await PolicyVersionRepository.GetQueryableAsync();
        var policyVersion = await AsyncExecuter.FirstOrDefaultAsync(
            versionQuery.Where(x => x.Id == input.PolicyVersionId));

        if (policyVersion == null)
        {
            throw new UserFriendlyException($"Policy version not found: {input.PolicyVersionId}");
        }

        var feeItemId = await GetResFeeItemIdByCodeAsync("TERMINATE_REFUND_AMOUNT");

        var issueDate = input.TerminationDate ?? Clock.Now.Date;
        var refundAmountAbs = Math.Abs(input.TotalTerminationAmount);

        var policyAmount = new PolicyAmount(
            GuidGenerator.Create(),
            policyVersion.PolicyId,
            policyVersion.Id,
            feeItemId,
            issueDate,
            refundAmountAbs,
            refundAmountAbs,
            0);
        await PolicyAmountRepository.InsertAsync(policyAmount);
    }

    /// <summary>
    /// Cancels a draft policy by setting status to Cancelled.
    /// </summary>
    public virtual async Task CancelPolicyAsync(Guid id)
    {
        // 1. Load policy
        var policyQuery = await Repository.GetQueryableAsync();
        var policy = await AsyncExecuter.FirstOrDefaultAsync(policyQuery.Where(x => x.Id == id));

        if (policy == null)
        {
            throw new UserFriendlyException($"Policy not found: {id}");
        }

        // 2. Validate policy can be cancelled (status should be Draft)
        if (policy.Status != PolicyStatus.Draft)
        {
            throw new UserFriendlyException("Only draft policies can be cancelled.");
        }

        // 3. Update policy status
        policy.UpdateStatus(PolicyStatus.Cancelled);

        // Sync latest policy_version.status
        var currentVersion = await PolicyVersionRepository.GetAsync(policy.LastVersionId);
        currentVersion.UpdateStatus(PolicyStatusToVersionStatusString(PolicyStatus.Cancelled));
        await PolicyVersionRepository.UpdateAsync(currentVersion);

        await Repository.UpdateAsync(policy);
        await CurrentUnitOfWork!.SaveChangesAsync();
    }

    public virtual async Task SubmitForApprovalAsync(Guid id, SubmitForApprovalInput? input = null)
    {
        var (policy, newestVersion) = await PreparePolicyForSubmitForApprovalAsync(id);
        var approverId = input?.ApproverId?.Trim() ?? "";
        await DispatchSubmitForApprovalWorkflowAsync(policy, newestVersion, approverId);
    }

    [Authorize(PolicyPermissions.Create)]
    public virtual async Task<CreateAndSubmitPolicyResultDto> CreateAndSubmitForApprovalAsync(CreatePolicyDto input)
    {
        if (input == null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        var createdPolicy = await CreateAsync(input);
        await CurrentUnitOfWork!.SaveChangesAsync();

        try
        {
            await SubmitForApprovalAsync(createdPolicy.Id);

            return new CreateAndSubmitPolicyResultDto
            {
                PolicyId = createdPolicy.Id,
                PolicyNo = createdPolicy.PolicyNo,
                Created = true,
                Submitted = true
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(
                ex,
                "Create-and-submit failed at submit phase for policy {PolicyId}. Draft creation was already committed.",
                createdPolicy.Id);

            return new CreateAndSubmitPolicyResultDto
            {
                PolicyId = createdPolicy.Id,
                PolicyNo = createdPolicy.PolicyNo,
                Created = true,
                Submitted = false,
                SubmitErrorCode = ex is BusinessException businessException ? businessException.Code : ex.GetType().Name,
                SubmitErrorMessage = ex.Message
            };
        }
    }

    private async Task<(iOne.Policies.Policy Policy, PolicyVersion NewestVersion)> PreparePolicyForSubmitForApprovalAsync(Guid id)
    {
        var query = await Repository.GetQueryableAsync();
        var policy = await AsyncExecuter.FirstOrDefaultAsync(
            query.Include(p => p.PolicyVersions).Include(p => p.Lob).Where(p => p.Id == id));
        if (policy == null)
        {
            throw new EntityNotFoundException(typeof(iOne.Policies.Policy), id);
        }

        var newestVersion = policy.PolicyVersions
            .FirstOrDefault(v => v.Id == policy.LastVersionId);
        if (newestVersion == null)
        {
            throw new UserFriendlyException(L["Policy:PolicyVersionNotFound"].Value);
        }
        if (ParseVersionStatusToPolicyStatus(newestVersion.Status) != PolicyStatus.Draft)
        {
            throw new UserFriendlyException(L["Policy:OnlyDraftPolicyVersionSubmitForApproval"].Value);
        }

        // Set policy approval status to pending only when null/blank
        if (string.IsNullOrWhiteSpace(policy.ApprovalStatus))
        {
            policy.UpdateApprovalStatus(PolicyApprovalStatus.pending.ToString());
        }

        newestVersion.UpdateApprovalStatus(PolicyApprovalStatus.pending.ToString());
        await PolicyVersionRepository.UpdateAsync(newestVersion);

        await Repository.UpdateAsync(policy);
        await CurrentUnitOfWork!.SaveChangesAsync();

        return (policy, newestVersion);
    }

    private async Task DispatchSubmitForApprovalWorkflowAsync(
        iOne.Policies.Policy policy,
        PolicyVersion newestVersion,
        string approverId)
    {
        var isRootPolicy = string.Equals(newestVersion.Type, "O", StringComparison.OrdinalIgnoreCase);
        if (isRootPolicy)
        {
            var isMotorbike = IsMotorbikePolicyLob(policy);
            await _elsaWorkflowService.InitCreatePolicyWorkflowAsync(
                newestVersion.Id,
                approverId,
                useMotorbikeCreatePolicyWorkflow: isMotorbike,
                approvalBusinessCode: isMotorbike ? MotorbikeCreatePolicyApprovalBusinessCode : null);
        }
        else
        {
            await _elsaWorkflowService.InitEndorsementWorkflowAsync(newestVersion.Id, approverId);
        }
    }

    private Task<string> GetNextPolicyNoForRenewalAsync()
    {
        // Per requirement: use this sequence only.
        // If not found / not active, GetNextSequenceCodeAsync will throw.
        return GetNextSequenceCodeAsync("POLICY_CODE_SEQ");
    }

    /// <summary>
    /// Renews an existing policy by cloning it into a new Draft policy.
    /// EffectiveDate = today, ExpireDate = today + 1 year, IssueDate = today.
    /// Does NOT clone any documents and creates a new contract.
    /// </summary>
    public virtual async Task<PolicyDto> RenewPolicyAsync(Guid id)
    {
        var source = await GetAsync(id);
        if (source == null)
        {
            throw new UserFriendlyException($"Policy not found: {id}");
        }

        // Business rule: allow renewing Active/Expired only (can be relaxed later)
        if (source.Status != PolicyStatus.Active && source.Status != PolicyStatus.Expired)
        {
            throw new UserFriendlyException("Only active/expired policies can be renewed.");
        }

        var today = Clock.Now.Date;
        var expire = today.AddYears(1);

        var versionPremiumTotal = source.VersionDetail?.PremiumTotal ?? source.PremiumTotal;
        var versionPremium = source.VersionDetail?.Premium ?? source.Premium;
        var versionVat = source.VersionDetail?.Vat ?? source.Vat;
        var versionDiscount = source.VersionDetail?.Discount ?? source.Discount;
        var versionDiscountRate = source.VersionDetail?.DiscountRate ?? source.DiscountRate;
        var versionMarkup = source.VersionDetail?.Markup;

        // Clone contract into a NEW contract (do not attach documents)
        CreatePolicyContractInputDto? contractInput = null;
        if (source.Contract != null)
        {
            contractInput = new CreatePolicyContractInputDto
            {
                // IMPORTANT: omit Code/Name to let server generate new contract code
                InsurerId = source.Contract.InsurerId,
                InsurerContractCode = source.Contract.InsurerContractCode,
                LobId = source.Contract.LobId,
                Type = source.Contract.Type,
                CustomerId = source.Contract.CustomerId,
                PayerName = source.Contract.PayerName,
                PayerEmail = source.Contract.PayerEmail,
                PayerPhone = source.Contract.PayerPhone,
                PayerProvinceId = source.Contract.PayerProvinceId,
                PayerWardId = source.Contract.PayerWardId,
                PayerAddress = source.Contract.PayerAddress,
                PayerFullAddress = source.Contract.PayerFullAddress,
                Description = source.Contract.Description,
                EffectDate = today,
                ExpireDate = expire,
                Quantity = source.Contract.Quantity ?? 1m,
                CurrentQuantity = source.Contract.CurrentQuantity,
                EmployeeId = source.Contract.EmployeeId,
                IsReciveInvoice = source.Contract.IsReciveInvoice
                // Documents intentionally NOT cloned
            };
        }

        // Clone products + coverages + coverage levels (documents are not part of these tables)
        var productsInput = source.Products?
            .Select(p => new CreatePolicyProductInputDto
            {
                ProductId = p.ProductId,
                InsurerProductCode = p.InsurerProductCode,
                AmountLiability = p.AmountLiability,
                PremiumTotal = p.PremiumTotal,
                Premium = p.Premium,
                Vat = p.Vat,
                Discount = p.Discount,
                DiscountRate = p.DiscountRate,
                Markup = p.Markup,
                Coverages = p.Coverages?
                    .Select(c => new CreatePolicyCoverageInputDto
                    {
                        CoverageId = c.CoverageId,
                        CoverageParentId = c.CoverageParentId,
                        InsurerCoverageCode = c.InsurerCoverageCode,
                        UomId = c.UomId ?? throw new UserFriendlyException("Coverage UomId is missing; cannot renew this policy."),
                        TaxId = c.TaxId ?? throw new UserFriendlyException("Coverage TaxId is missing; cannot renew this policy."),
                        TableRateLineId = c.TableRateLineId,
                        AmountLiability = c.AmountLiability,
                        Quantity = c.Quantity,
                        NetRate = c.NetRate,
                        BaseRate = c.BaseRate,
                        FlatRate = c.FlatRate,
                        Loading = c.Loading,
                        PremiumRate = c.PremiumRate,
                        PremiumTotal = c.PremiumTotal,
                        Premium = c.Premium,
                        Vat = c.Vat,
                        Discount = c.Discount,
                        DiscountRate = c.DiscountRate,
                        CoverageLevels = c.CoverageLevels?
                            .Select(l => new CreatePolicyCoverageLevelInputDto
                            {
                                CoverageLevelTypeId = l.CoverageLevelTypeId,
                                CoverageLevelBasisId = l.CoverageLevelBasisId,
                                AmountType = l.AmountType,
                                FromAmount = l.FromAmount,
                                ToAmount = l.ToAmount,
                                ConditionScript = l.ConditionScript,
                                ComputeScript = l.ComputeScript
                            })
                            .ToList()
                    })
                    .ToList() ?? new List<CreatePolicyCoverageInputDto>()
            })
            .ToList();

        // Clone risk object + motor (do not clone risk-object documents)
        CreatePolicyRiskObjectInputDto? riskObjectInput = null;
        if (source.RiskObject != null)
        {
            var rm = source.RiskObject.RiskObjectMotor;
            if (!source.RiskObject.ObjectTypeId.HasValue || source.RiskObject.ObjectTypeId.Value == Guid.Empty)
            {
                throw new UserFriendlyException("Risk object type is missing; cannot renew this policy.");
            }
            riskObjectInput = new CreatePolicyRiskObjectInputDto
            {
                ObjectTypeId = source.RiskObject.ObjectTypeId.Value,
                RepName = source.RiskObject.RepName,
                RepIdNo = source.RiskObject.RepIdNo,
                RepPassport = source.RiskObject.RepPassport,
                RepPhone = source.RiskObject.RepPhone,
                RepEmail = source.RiskObject.RepEmail,
                RepProvinceId = source.RiskObject.RepProvinceId,
                RepWardId = source.RiskObject.RepWardId,
                RepAddress = source.RiskObject.RepAddress,
                RepFullAddress = source.RiskObject.RepFullAddress,
                RiskObjectProvinceId = source.RiskObject.RiskObjectProvinceId,
                RiskObjectWardId = source.RiskObject.RiskObjectWardId,
                RiskObjectAddress = source.RiskObject.RiskObjectAddress,
                RiskObjectFullAddress = source.RiskObject.RiskObjectFullAddress,
                RiskObjectLat = source.RiskObject.RiskObjectLat,
                RiskObjectLong = source.RiskObject.RiskObjectLong,
                // Documents intentionally NOT cloned
                RiskObjectMotor = rm == null
                    ? null
                    : new CreatePolicyRiskMotorInputDto
                    {
                        RiskObjectValue = rm.RiskObjectValue,
                        MotorClassCode = rm.MotorClassCode,
                        CarLineCode = rm.CarLineCode,
                        CarGroupCode = rm.CarGroupCode,
                        CarTypeCode = rm.CarTypeCode,
                        CarBrandCode = rm.CarBrandCode,
                        CarModelCode = rm.CarModelCode,
                        CarCategoryCode = rm.CarCategoryCode,
                        CarUsage = rm.CarUsage,
                        CarOld = rm.CarOld,
                        CarProductionYear = rm.CarProductionYear,
                        CarPlate = rm.CarPlate,
                        CarPlateClear = rm.CarPlateClear,
                        CarSeatNumber = rm.CarSeatNumber,
                        CarVin = rm.CarVin,
                        CarEngineNumber = rm.CarEngineNumber,
                        CarPayloadCapacity = rm.CarPayloadCapacity,
                        CarColor = rm.CarColor,
                        CarOrigin = rm.CarOrigin,
                        CarNew = rm.CarNew
                    }
            };
        }

        var createInput = new CreatePolicyDto
        {
            // New policy must have a unique policy no
            PolicyNo = await GetNextPolicyNoForRenewalAsync(),
            LobId = source.LobId,
            SellType = source.SellType,
            PolicyTypeId = source.PolicyTypeId,
            PartnerId = source.PartnerId,
            SellerId = source.SellerId,
            ImplementerId = source.ImplementerId,
            CurrencyId = source.CurrencyId,
            ExchangeRate = source.ExchangeRate,

            // Dates
            OrgEffectDate = today,
            OrgExpireDate = expire,

            // New policy should start as Draft
            Status = PolicyStatus.Draft,
            IsRenewal = "Y",
            IsGift = "N",
            IsBankLoan = source.IsBankLoan,

            // Do not carry over approval/cancellation/termination fields
            ApprovalStatus = null,
            CancellationDate = null,
            TerminationDate = null,
            CancellationReasonId = null,
            TerminationReasonId = null,

            // Optional fields
            ChannelId = source.ChannelId,
            LotImportCode = source.LotImportCode,
            InsurerPolicyNo = null,

            // Insured/beneficiary info
            InsuredName = source.InsuredName,
            InsuredIdNo = source.InsuredIdNo,
            InsuredTin = source.InsuredTin,
            InsuredPassport = source.InsuredPassport,
            InsuredPhone = source.InsuredPhone,
            InsuredEmail = source.InsuredEmail,
            InsuredProvinceId = source.InsuredProvinceId,
            InsuredWardId = source.InsuredWardId,
            InsuredAddress = source.InsuredAddress,
            InsuredFullAddress = source.InsuredFullAddress,
            InsuredOrgType = source.InsuredOrgType,

            BeneficiaryName = source.BeneficiaryName,
            BeneficiaryIdNo = source.BeneficiaryIdNo,
            BeneficiaryTin = source.BeneficiaryTin,
            BeneficiaryPassport = source.BeneficiaryPassport,
            BeneficiaryPhone = source.BeneficiaryPhone,
            BeneficiaryEmail = source.BeneficiaryEmail,
            BeneficiaryProvinceId = source.BeneficiaryProvinceId,
            BeneficiaryWardId = source.BeneficiaryWardId,
            BeneficiaryAddress = source.BeneficiaryAddress,
            BeneficiaryFullAddress = source.BeneficiaryFullAddress,
            BeneficiaryOrgType = source.BeneficiaryOrgType,

            // Amounts
            PremiumTotal = source.PremiumTotal,
            Premium = source.Premium,
            Vat = source.Vat,
            Discount = source.Discount,
            DiscountRate = source.DiscountRate,

            // New Contract (instead of reusing existing)
            ContractId = null,
            Contract = contractInput,

            // New Version (required for nested children)
            LastVersionId = Guid.Empty,
            Version = new CreatePolicyVersionInputDto
            {
                Version = 0,
                EffectDate = today,
                ExpireDate = expire,
                OrgEffectDate = today,
                OrgExpireDate = expire,
                PremiumTotal = versionPremiumTotal,
                Premium = versionPremium,
                Vat = versionVat,
                InternalNote = source.VersionDetail?.InternalNote,
                CustomerNote = source.VersionDetail?.CustomerNote,
                Discount = versionDiscount,
                DiscountRate = versionDiscountRate,
                Markup = versionMarkup
            },

            Products = productsInput,
            RiskObject = riskObjectInput,

            // Documents intentionally NOT cloned
            Documents = null,
            Amount = null
        };

        return await CreateAsync(createInput);
    }

    #endregion

    #region Payment

    /// <summary>
    /// Tổng phải thu (SUM <c>policy_amount.amount_total</c> join phiên bản đơn) và tổng đã thanh toán (<c>account_payment_request</c> Approved).
    /// Phiên bản: chỉ <c>active</c>; nếu <paramref name="isPaymentOnline"/> thì thêm cả <c>draft</c>.
    /// Dùng chung cho <see cref="GetPaymentConfigAsync"/>, <see cref="CreatePaymentRequestAsync"/> và file mẫu Excel.
    /// </summary>
    private async Task<(decimal TotalOrderAmount, decimal TotalPaidApproved)> GetPolicyPaymentOrderAndPaidTotalsAsync(
        Guid policyId,
        bool isPaymentOnline = false)
    {
        var activeVersionStatus = PolicyStatus.Active.ToString().ToLowerInvariant();
        var draftVersionStatus = PolicyStatus.Draft.ToString().ToLowerInvariant();
        var policyAmountQuery = await PolicyAmountRepository.GetQueryableAsync();
        var policyVersionQuery = await PolicyVersionRepository.GetQueryableAsync();
        var totalOrderAmount = await AsyncExecuter.SumAsync(
            from pa in policyAmountQuery
            join pv in policyVersionQuery on pa.PolicyVersionId equals pv.Id
            where pa.PolicyId == policyId
                  && !pa.IsDeleted
                  && !pv.IsDeleted
                  && (pv.Status == activeVersionStatus
                      || (isPaymentOnline && pv.Status == draftVersionStatus))
            select pa.AmountTotal);
        var paymentRequestQuery = await AccountPaymentRequestRepository.GetQueryableAsync();
        var totalPaid = await paymentRequestQuery
            .Where(x => x.PolicyId == policyId && x.Status == AccountPaymentRequestStatus.Approved)
            .SumAsync(x => x.Amount);
        return (totalOrderAmount, totalPaid);
    }

    /// <summary>Số tiền còn phải thu (cùng công thức đề xuất khi mở cập nhật thanh toán).</summary>
    private static decimal ComputeAmountToPayFromOrderAndPaid(decimal totalOrderAmount, decimal totalPaidApproved) =>
        totalOrderAmount - totalPaidApproved;

    public virtual async Task<PaymentConfigDto> GetPaymentConfigAsync(Guid policyId, bool isPaymentOnline = false)
    {
        var policy = await PolicyRepository.GetAsync(policyId);
        var contract = policy.ContractId.HasValue
            ? await PolicyContractRepository.GetAsync(policy.ContractId.Value)
            : null;

        // Payment methods - sorted by name A-Z
        var paymentMethodsQuery = (await ResPaymentMethodRepository.GetQueryableAsync())
            .Where(x => !x.IsDeleted && x.Status == ResPaymentMethodStatus.Active)
            .OrderBy(x => x.Name);
        var paymentMethods = await AsyncExecuter.ToListAsync(paymentMethodsQuery);

        // Payment types (loại thanh toán công nợ)
        var paymentTypesQuery = (await ResPaymentTypeRepository.GetQueryableAsync())
            .Where(x => !x.IsDeleted && x.Status == ResPaymentTypeStatus.Active)
            .OrderBy(x => x.Name);
        var paymentTypes = await AsyncExecuter.ToListAsync(paymentTypesQuery);

        var (totalOrderAmount, totalPaid) = await GetPolicyPaymentOrderAndPaidTotalsAsync(policyId, isPaymentOnline);
        var amountToPay = ComputeAmountToPayFromOrderAndPaid(totalOrderAmount, totalPaid);

        return new PaymentConfigDto
        {
            PaymentMethods = paymentMethods.Select(x => new PaymentMethodSelectDto
            {
                Id = x.Id,
                Code = x.Code ?? string.Empty,
                Name = x.Name ?? string.Empty
            }).ToList(),
            AmountToPay = amountToPay,
            PaymentTypes = paymentTypes.Select(x => new PaymentTypeSelectDto
            {
                Id = x.Id,
                Code = x.Code ?? string.Empty,
                Name = x.Name ?? string.Empty
            }).ToList()
        };
    }

    /// <summary>
    /// Gộp <c>policy_amount.payment_status</c> nhiều dòng cùng một phiên bản đơn để hiển thị trên <see cref="PolicyDto.PaymentStatus"/>.
    /// </summary>
    private static string AggregatePaymentStatusForDisplay(IEnumerable<string?> rowStatuses)
    {
        var list = rowStatuses.Select(s => (s ?? "new").Trim().ToLowerInvariant()).ToList();
        if (list.Count == 0) return "new";
        if (list.All(s => s == "paid" || s == "done")) return "paid";
        if (list.Any(s => s == "paid" || s == "done" || s == "partial" || s == "inprogress")) return "partial";
        return "new";
    }

    private const decimal PaymentFifoTolerance = 0.0001m;

    /// <summary>
    /// Các dòng policy_amount thuộc phiên bản đơn Active (và Draft khi <paramref name="isPaymentOnline"/>), thứ tự FIFO (cũ trước).
    /// Cùng tập với tính amountToPay / totalOrderAmount trong cấu hình thanh toán.
    /// </summary>
    private async Task<List<PolicyAmount>> GetOrderedActivePolicyAmountsForPaymentAsync(Guid policyId, bool isPaymentOnline = false)
    {
        var activeVersionStatus = PolicyStatus.Active.ToString().ToLowerInvariant();
        var draftVersionStatus = PolicyStatus.Draft.ToString().ToLowerInvariant();
        var paq = await PolicyAmountRepository.GetQueryableAsync();
        var pvq = await PolicyVersionRepository.GetQueryableAsync();
        var q = from pa in paq
                join pv in pvq on pa.PolicyVersionId equals pv.Id
                where pa.PolicyId == policyId
                      && !pa.IsDeleted
                      && !pv.IsDeleted
                      && (pv.Status == activeVersionStatus
                          || (isPaymentOnline && pv.Status == draftVersionStatus))
                orderby pa.IssueDate, pa.CreationTime, pa.Id
                select pa;
        return await AsyncExecuter.ToListAsync(q);
    }

    private async Task<List<AccountPaymentRequest>> GetApprovedPolicyPaymentRequestsOrderedAsync(Guid policyId)
    {
        var qr = await AccountPaymentRequestRepository.GetQueryableAsync();
        return await AsyncExecuter.ToListAsync(
            qr.Where(x => x.PolicyId == policyId && x.Status == AccountPaymentRequestStatus.Approved)
                .OrderBy(x => x.IssueDate)
                .ThenBy(x => x.SubmittedDate ?? x.CreationTime)
                .ThenBy(x => x.Id));
    }

    /// <summary>
    /// Cùng thứ tự với <see cref="GetApprovedPolicyPaymentRequestsOrderedAsync"/>, gồm cả khoản vừa <c>InsertAsync</c>
    /// chưa flush (truy vấn DB không trả về) để FIFO / payment_status không bị lệch trong cùng unit of work.
    /// </summary>
    private static List<AccountPaymentRequest> OrderApprovedPaymentRequestsChronological(
        IEnumerable<AccountPaymentRequest> requests)
    {
        return requests
            .OrderBy(x => x.IssueDate)
            .ThenBy(x => x.SubmittedDate ?? x.CreationTime)
            .ThenBy(x => x.Id)
            .ToList();
    }

    /// <summary>
    /// Phân bổ FIFO: mỗi lần thanh toán gạch lần lượt từ policy_amount đầu danh sách đến khi hết tiền của lần đó.
    /// Trả về tổng đã gạch tích lũy trên từng policy_amount id sau khi áp dụng toàn bộ các khoản thanh toán theo thứ tự.
    /// </summary>
    private static Dictionary<Guid, decimal> BuildFifoCumulativeAllocatedByPolicyAmountId(
        IReadOnlyList<PolicyAmount> orderedRows,
        IEnumerable<decimal> paymentAmountsChronological)
    {
        var cumulative = orderedRows.ToDictionary(r => r.Id, _ => 0m);
        foreach (var pay in paymentAmountsChronological)
        {
            var left = pay;
            foreach (var row in orderedRows)
            {
                if (left <= 0) break;
                var outstanding = row.AmountTotal - cumulative[row.Id];
                if (outstanding <= 0) continue;
                var take = Math.Min(left, outstanding);
                cumulative[row.Id] += take;
                left -= take;
            }
        }

        return cumulative;
    }

    /// <summary>
    /// Phân bổ một khoản thanh toán mới theo FIFO trên các dòng đã có tích lũy <paramref name="cumulativeBefore"/>.
    /// </summary>
    private static Dictionary<Guid, decimal> AllocateFifoSinglePayment(
        IReadOnlyList<PolicyAmount> orderedRows,
        IReadOnlyDictionary<Guid, decimal> cumulativeBefore,
        decimal paymentAmount,
        out decimal leftover)
    {
        var alloc = orderedRows.ToDictionary(r => r.Id, _ => 0m);
        var left = paymentAmount;
        foreach (var row in orderedRows)
        {
            if (left <= 0) break;
            var outstanding = row.AmountTotal - cumulativeBefore[row.Id];
            if (outstanding <= 0) continue;
            var take = Math.Min(left, outstanding);
            alloc[row.Id] = take;
            left -= take;
        }

        leftover = left;
        return alloc;
    }

    /// <summary>
    /// Sau khi tạo account_payment_request: cập nhật từng policy_amount (phiên bản active) theo phân bổ FIFO;
    /// dòng có nhận tiền từ lần này thì ghi payment_method_id / payment_date theo lần thanh toán này.
    /// </summary>
    private async Task ApplyFifoPaymentToActivePolicyAmountsAsync(
        Guid policyId,
        AccountPaymentRequest newPaymentRequest,
        Guid paymentMethodId,
        DateTime paymentDate,
        bool isPaymentOnline = false)
    {
        var orderedRows = await GetOrderedActivePolicyAmountsForPaymentAsync(policyId, isPaymentOnline);
        if (orderedRows.Count == 0) return;

        var approvedFromDb = await GetApprovedPolicyPaymentRequestsOrderedAsync(policyId);
        var approvedOrdered = OrderApprovedPaymentRequestsChronological(
            approvedFromDb.Where(p => p.Id != newPaymentRequest.Id).Append(newPaymentRequest));
        var cumulativeAfter = BuildFifoCumulativeAllocatedByPolicyAmountId(
            orderedRows,
            approvedOrdered.Select(p => p.Amount));

        var beforeList = approvedOrdered.Where(p => p.Id != newPaymentRequest.Id).ToList();
        var cumulativeBefore = BuildFifoCumulativeAllocatedByPolicyAmountId(
            orderedRows,
            beforeList.Select(p => p.Amount));

        foreach (var row in orderedRows)
        {
            var allocAfter = cumulativeAfter[row.Id];
            var allocThis = allocAfter - cumulativeBefore[row.Id];

            string newStatus;
            // Dòng amount_total âm (giảm phí / điều chỉnh): không tham gia gạch FIFO; không dùng alloc so với số âm (tránh coi như "paid" nhầm).
            if (row.AmountTotal < -PaymentFifoTolerance)
            {
                newStatus = "paid";
            }
            else if (allocAfter + PaymentFifoTolerance >= row.AmountTotal) newStatus = "paid";
            else if (allocAfter > PaymentFifoTolerance) newStatus = "partial";
            else newStatus = "new";

            var methodId = allocThis > PaymentFifoTolerance ? paymentMethodId : row.PaymentMethodId;
            var payDate = allocThis > PaymentFifoTolerance ? (DateTime?)paymentDate.Date : row.PaymentDate;

            await PolicyAmountManager.UpdateAsync(
                row,
                row.IssueDate,
                row.AmountTotal,
                row.Amount,
                row.Vat,
                newStatus,
                methodId,
                payDate);
        }
    }

    /// <summary>
    /// Xác định <c>res_payment_type</c> cho đề nghị thanh toán: ưu tiên mã (<paramref name="paymentTypeCode"/>),
    /// sau đó Id (<paramref name="paymentTypeId"/>), mặc định code công nợ PAYMENT_DEBT.
    /// </summary>
    private async Task<Guid> ResolveAccountPaymentTypeIdAsync(string? paymentTypeCode, Guid? paymentTypeId)
    {
        var codeTrimmed = paymentTypeCode?.Trim();
        if (!string.IsNullOrEmpty(codeTrimmed))
        {
            var normalizedCode = codeTrimmed.ToUpperInvariant();
            var byCode = await ResPaymentTypeRepository.FirstOrDefaultAsync(x =>
                !x.IsDeleted && x.Status == ResPaymentTypeStatus.Active && x.Code == normalizedCode);
            if (byCode == null)
                throw new UserFriendlyException(L["Payment:PaymentTypeNotFound"].Value);
            return byCode.Id;
        }

        if (paymentTypeId.HasValue && paymentTypeId.Value != Guid.Empty)
        {
            var byId = await ResPaymentTypeRepository.FirstOrDefaultAsync(x =>
                x.Id == paymentTypeId.Value && !x.IsDeleted && x.Status == ResPaymentTypeStatus.Active);
            if (byId == null)
                throw new UserFriendlyException(L["Payment:PaymentTypeNotFound"].Value);
            return byId.Id;
        }

        var debtPaymentType = await ResPaymentTypeRepository.FirstOrDefaultAsync(x =>
            !x.IsDeleted && x.Status == ResPaymentTypeStatus.Active && x.Code == PaymentTypeDebtCode);
        if (debtPaymentType == null)
            throw new UserFriendlyException(L["Payment:DebtPaymentTypeNotFound"].Value);
        return debtPaymentType.Id;
    }

    /// <summary>
    /// Xác định <c>res_payment_method</c>: ưu tiên <paramref name="paymentMethodCode"/>, sau đó <paramref name="paymentMethodId"/>.
    /// </summary>
    private async Task<Guid> ResolveAccountPaymentMethodIdAsync(string? paymentMethodCode, Guid? paymentMethodId)
    {
        var codeTrimmed = paymentMethodCode?.Trim();
        if (!string.IsNullOrEmpty(codeTrimmed))
        {
            var normalizedCode = codeTrimmed.ToUpperInvariant();
            var byCode = await ResPaymentMethodRepository.FirstOrDefaultAsync(x =>
                !x.IsDeleted && x.Status == ResPaymentMethodStatus.Active && x.Code == normalizedCode);
            if (byCode == null)
                throw new UserFriendlyException(L["Payment:PaymentMethodNotFound"].Value);
            return byCode.Id;
        }

        if (paymentMethodId.HasValue && paymentMethodId.Value != Guid.Empty)
        {
            var byId = await ResPaymentMethodRepository.FirstOrDefaultAsync(x =>
                x.Id == paymentMethodId.Value && !x.IsDeleted && x.Status == ResPaymentMethodStatus.Active);
            if (byId == null)
                throw new UserFriendlyException(L["Payment:PaymentMethodNotFound"].Value);
            return byId.Id;
        }

        throw new UserFriendlyException(L["Payment:PaymentMethodIdRequired"].Value);
    }

    public virtual async Task<AccountPaymentRequestDto> CreatePaymentRequestAsync(Guid policyId, CreatePaymentRequestInput input)
    {
        if (CurrentUser.Id == null)
            throw new UserFriendlyException(L["UserNotAuthenticated"].Value);
        var employee = await HrEmployeeRepository.FirstOrDefaultAsync(e => e.UserId == CurrentUser.Id.Value);
        if (employee == null)
            throw new UserFriendlyException(L["EmployeeNotFoundForUser"].Value);

        return await CreatePaymentRequestCoreAsync(policyId, input, AccountPaymentRequestStatus.PendingApproval, employee.Id);
    }

    [AllowAnonymous]
    [RemoteService(false)]
    public virtual async Task<AccountPaymentRequestDto> CreatePaymentRequestIpnAsync(Guid policyId, CreatePaymentRequestInput input)
    {
        // Không check user đăng nhập (dùng cho IPN webhook)
        return await CreatePaymentRequestCoreAsync(policyId, input, AccountPaymentRequestStatus.Approved, submitterId: null);
    }

    /// <inheritdoc />
    [RemoteService(false)]
    public virtual async Task MarkPolicyAmountsAsPaidAsync(Guid policyId)
    {
        var rows = await PolicyAmountRepository.GetListAsync(x => x.PolicyId == policyId && !x.IsDeleted);
        foreach (var row in rows)
        {
            await PolicyAmountManager.UpdateAsync(
                row,
                row.IssueDate,
                row.AmountTotal,
                row.Amount,
                row.Vat,
                "paid",
                row.PaymentMethodId,
                row.PaymentDate);
        }
    }

    /// <summary>
    /// Core payment request creation logic shared by the HTTP endpoint, IPN webhook, and partner motorbike issue.
    /// Caller is responsible for resolving the appropriate <paramref name="submitterId"/>
    /// and <paramref name="status"/> before calling.
    /// When <paramref name="applyFifoToPolicyAmounts"/> is false (partner-motor-policy-issue), only inserts
    /// <c>account_payment_request</c>; <c>policy_amount.payment_status</c> stays unchanged (typically <c>new</c>).
    /// </summary>
    internal async Task<AccountPaymentRequestDto> CreatePaymentRequestCoreAsync(
        Guid policyId,
        CreatePaymentRequestInput input,
        AccountPaymentRequestStatus status,
        Guid? submitterId,
        bool applyFifoToPolicyAmounts = true)
    {
        // Validate
        if (input.Amount <= 0)
            throw new UserFriendlyException(L["Payment:AmountMustBePositive"].Value);

        var resolvedPaymentMethodId =
            await ResolveAccountPaymentMethodIdAsync(input.PaymentMethodCode, input.PaymentMethodId);

        var isPaymentOnline = input.IsPaymentOnline;

        var policy = await PolicyRepository.GetAsync(policyId);

        if (applyFifoToPolicyAmounts)
        {
            // Số tiền thanh toán không được vượt quá số tiền cần thanh toán (cùng nguồn với GetPaymentConfig / file mẫu).
            var (totalOrderAmount, totalPaid) = await GetPolicyPaymentOrderAndPaidTotalsAsync(policyId, isPaymentOnline);
            var amountToPay = ComputeAmountToPayFromOrderAndPaid(totalOrderAmount, totalPaid);
            if (input.Amount > amountToPay)
                throw new UserFriendlyException(L["Payment:AmountExceedsAmountToPay"].Value);

            // FIFO: đảm bảo số tiền không dư sau khi gạch lần lượt các policy_amount (active; + draft khi thanh toán online).
            var orderedAmountRows = await GetOrderedActivePolicyAmountsForPaymentAsync(policyId, isPaymentOnline);
            var priorApprovedPayments = await GetApprovedPolicyPaymentRequestsOrderedAsync(policyId);
            var cumulativeBeforeNew = BuildFifoCumulativeAllocatedByPolicyAmountId(
                orderedAmountRows,
                priorApprovedPayments.Select(p => p.Amount));
            AllocateFifoSinglePayment(orderedAmountRows, cumulativeBeforeNew, input.Amount, out var fifoLeftover);
            if (fifoLeftover > PaymentFifoTolerance)
                throw new UserFriendlyException(L["Payment:AmountExceedsAmountToPay"].Value);
        }

        var contract = policy.ContractId.HasValue
            ? await PolicyContractRepository.GetAsync(policy.ContractId.Value)
            : null;
        if (contract == null)
            throw new UserFriendlyException(L["Payment:PolicyContractNotFound"].Value);

        var resolvedPaymentTypeId =
            await ResolveAccountPaymentTypeIdAsync(input.PaymentTypeCode, input.PaymentTypeId);

        var transRef = string.IsNullOrWhiteSpace(input.TransRef) ? null : input.TransRef.Trim();
        if (transRef != null && transRef.Length > 255)
            throw new UserFriendlyException(L["Payment:TransRefTooLong"].Value);
        var paymentProvider = string.IsNullOrWhiteSpace(input.PaymentProvider) ? null : input.PaymentProvider.Trim();
        if (paymentProvider != null && paymentProvider.Length > 255)
            throw new UserFriendlyException("Payment provider is too long.");

        var entity = new AccountPaymentRequest(
            GuidGenerator.Create(),
            input.PaymentDate.Date,
            resolvedPaymentMethodId,
            resolvedPaymentTypeId,
            policy.CurrencyId,
            input.PaymentDate.Date,
            input.Amount,
            status,
            customerId: contract.CustomerId,
            policyId: policyId,
            submittedDate: DateTime.UtcNow,
            submitterId: submitterId,
            transRef: transRef,
            paymentProvider: paymentProvider
        );

        await AccountPaymentRequestManager.CreateAsync(entity);

        if (applyFifoToPolicyAmounts)
        {
            await ApplyFifoPaymentToActivePolicyAmountsAsync(
                policyId, entity, resolvedPaymentMethodId, input.PaymentDate, isPaymentOnline);
        }

        return new AccountPaymentRequestDto
        {
            Id = entity.Id,
            CustomerId = entity.CustomerId,
            PolicyId = entity.PolicyId,
            IssueDate = entity.IssueDate,
            PaymentMethodId = entity.PaymentMethodId,
            PaymentTypeId = entity.PaymentTypeId,
            CurrencyId = entity.CurrencyId,
            DueDate = entity.DueDate,
            Amount = entity.Amount,
            Status = entity.Status.ToString(),
            SubmittedDate = entity.SubmittedDate,
            SubmitterId = entity.SubmitterId,
            TransRef = entity.TransRef,
            PaymentProvider = entity.PaymentProvider
        };
    }

    public virtual async Task<byte[]> ExportPaymentTemplateAsync(Guid? lobId = null, bool isPaymentOnline = false)
    {
        var paymentMethods = await (await ResPaymentMethodRepository.GetQueryableAsync())
            .Where(x => !x.IsDeleted && x.Status == ResPaymentMethodStatus.Active)
            .OrderBy(x => x.Name)
            .Select(x => new { x.Id, x.Name })
            .ToListAsync();

        var activeVersionStatus = PolicyStatus.Active.ToString().ToLowerInvariant();
        var draftVersionStatus = PolicyStatus.Draft.ToString().ToLowerInvariant();
        var policyAmountQuery = await PolicyAmountRepository.GetQueryableAsync();
        var policyVersionQuery = await PolicyVersionRepository.GetQueryableAsync();
        var paymentRequestQuery = await AccountPaymentRequestRepository.GetQueryableAsync();

        // Cùng tập policy_amount với cấu hình / tạo thanh toán (active; + draft khi isPaymentOnline).
        var totalOrderByPolicy = await AsyncExecuter.ToListAsync(
            from pa in policyAmountQuery
            join pv in policyVersionQuery on pa.PolicyVersionId equals pv.Id
            where !pa.IsDeleted
                  && !pv.IsDeleted
                  && (pv.Status == activeVersionStatus
                      || (isPaymentOnline && pv.Status == draftVersionStatus))
            group pa by pa.PolicyId into g
            select new { PolicyId = g.Key, Total = g.Sum(x => x.AmountTotal) });
        var totalPaidByPolicy = await AsyncExecuter.ToListAsync(
            paymentRequestQuery
                .Where(apr => apr.Status == AccountPaymentRequestStatus.Approved)
                .GroupBy(apr => apr.PolicyId)
                .Select(g => new { PolicyId = g.Key, Total = g.Sum(x => x.Amount) }));

        var totalOrderDict = totalOrderByPolicy.ToDictionary(x => x.PolicyId, x => x.Total);
        var totalPaidDict = totalPaidByPolicy.ToDictionary(x => x.PolicyId, x => x.Total);

        var policyIdsNeedingPayment = totalOrderByPolicy
            .Where(x => x.Total - totalPaidDict.GetValueOrDefault(x.PolicyId, 0m) > 0m)
            .Select(x => x.PolicyId)
            .ToList();

        var policyQuery = await PolicyRepository.GetQueryableAsync();
        IQueryable<iOne.Policies.Policy> policiesQuery = policyQuery
            .Where(p => !p.IsDeleted && policyIdsNeedingPayment.Contains(p.Id));

        if (lobId.HasValue && lobId.Value != Guid.Empty)
        {
            policiesQuery = policiesQuery.Where(p => p.LobId == lobId.Value);
        }

        // Cùng phạm vi với danh sách đơn (ApplyPolicySearchScopeAsync / PolicySearchQueryScope).
        policiesQuery = await ApplyPolicySearchScopeAsync(policiesQuery);

        var policies = policyIdsNeedingPayment.Count == 0
            ? new List<iOne.Policies.Policy>()
            : await AsyncExecuter.ToListAsync(
                policiesQuery
                    .Include(p => p.Contract).ThenInclude(c => c!.Customer)
                    .OrderBy(p => p.CreationTime)
                    .ThenBy(p => p.Id));

        var rows = new List<(string PolicyNo, string CustomerName, decimal AmountToPay)>();
        foreach (var p in policies)
        {
            // Khớp từng đơn với GetPaymentConfigAsync (không Math.Max): bulk dict cùng nguồn SUM với helper.
            var totalOrder = totalOrderDict.GetValueOrDefault(p.Id, 0m);
            var totalPaid = totalPaidDict.GetValueOrDefault(p.Id, 0m);
            var amountToPay = ComputeAmountToPayFromOrderAndPaid(totalOrder, totalPaid);
            if (amountToPay <= 0m) continue;

            var customerName = p.Contract?.Customer?.Name ?? p.InsuredName ?? "";
            rows.Add((p.PolicyNo ?? "", customerName, amountToPay));
        }

        using var workbook = new XLWorkbook();

        // Sheet ẩn chứa danh sách hình thức thanh toán (Id, Name) - dùng cho dropdown
        var listSheet = workbook.Worksheets.Add("_PaymentMethods");
        listSheet.Visibility = XLWorksheetVisibility.VeryHidden;
        for (int i = 0; i < paymentMethods.Count; i++)
        {
            listSheet.Cell(i + 1, 1).Value = paymentMethods[i].Id.ToString();
            listSheet.Cell(i + 1, 2).Value = paymentMethods[i].Name ?? "";
        }
        var nameListRange = listSheet.Range(1, 2, paymentMethods.Count, 2); // Cột Name (view)

        var worksheet = workbook.Worksheets.Add("Sheet1");

        worksheet.Cell(1, 1).Value = "STT";
        worksheet.Cell(1, 2).Value = "Mã đơn";
        worksheet.Cell(1, 3).Value = "Tên khách hàng";
        worksheet.Cell(1, 4).Value = "Số tiền cần thanh toán";
        worksheet.Cell(1, 5).Value = "Số tiền thanh toán";
        worksheet.Cell(1, 6).Value = "Hình thức thanh toán";
        worksheet.Cell(1, 7).Value = "Ngày thanh toán";

        worksheet.Row(1).Style.Font.Bold = true;
        worksheet.Row(1).Style.Fill.BackgroundColor = XLColor.LightGray;

        for (int i = 0; i < rows.Count; i++)
        {
            var (policyNo, customerName, amountToPay) = rows[i];
            worksheet.Cell(i + 2, 1).Value = i + 1;
            worksheet.Cell(i + 2, 2).Value = policyNo;
            worksheet.Cell(i + 2, 3).Value = customerName;
            worksheet.Cell(i + 2, 4).Value = amountToPay;
            worksheet.Cell(i + 2, 5).Value = "";
            worksheet.Cell(i + 2, 6).Value = "";
            worksheet.Cell(i + 2, 7).Value = "";
        }

        // Data validation dropdown cho cột Hình thức thanh toán (cột 6) - key=Id, view=Name
        if (paymentMethods.Count > 0 && rows.Count > 0)
        {
            var dataRange = worksheet.Range(2, 6, rows.Count + 1, 6);
            dataRange.SetDataValidation().List(nameListRange);
            dataRange.SetDataValidation().IgnoreBlanks = true;
            dataRange.SetDataValidation().InCellDropdown = true;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public virtual async Task<ImportPaymentExcelResultDto> ImportPaymentExcelAsync(byte[] fileBytes)
    {
        var result = new ImportPaymentExcelResultDto();
        var errors = new List<ImportPaymentExcelErrorDto>();

        using var stream = new MemoryStream(fileBytes);
        using var workbook = new XLWorkbook(stream);
        // Template export có Sheet1 (dữ liệu) và _PaymentMethods (ẩn). Lấy Sheet1.
        var worksheet = workbook.Worksheets.FirstOrDefault(w => string.Equals(w.Name, "Sheet1", StringComparison.OrdinalIgnoreCase))
            ?? workbook.Worksheets.FirstOrDefault();

        if (worksheet == null)
            throw new UserFriendlyException(L["Payment:ExcelFileInvalid"].Value);

        var headerRow = worksheet.Row(1);
        int policyNoCol = 0, amountCol = 0, paymentMethodCol = 0, paymentDateCol = 0;

        // Khớp header với template export: STT, Mã đơn, Tên khách hàng, Số tiền cần thanh toán, Số tiền thanh toán, Hình thức thanh toán, Ngày thanh toán
        // Dùng RemoveDiacritics để so sánh thống nhất (tiếng Việt có dấu vs không dấu). Đ/đ được chuẩn hóa thành D.
        for (int col = 1; col <= 15; col++)
        {
            var headerValue = headerRow.Cell(col).GetString().Trim();
            if (string.IsNullOrWhiteSpace(headerValue)) continue;

            var h = RemoveDiacritics(headerValue.Replace(" ", "").ToUpperInvariant());
            if ((h == "MADON" || h.Contains("MADON")) && policyNoCol == 0) policyNoCol = col;
            else if ((h == "SOTIENTHANHTOAN" || h.Contains("SOTIENTHANHTOAN")) && !h.Contains("CAN") && amountCol == 0) amountCol = col; // Loại "Số tiền cần thanh toán"
            else if ((h == "HINHTHUCTHANHTOAN" || h.Contains("HINHTHUCTHANHTOAN")) && paymentMethodCol == 0) paymentMethodCol = col;
            else if ((h == "NGAYTHANHTOAN" || h.Contains("NGAYTHANHTOAN")) && paymentDateCol == 0) paymentDateCol = col;
        }

        if (policyNoCol == 0 || amountCol == 0 || paymentMethodCol == 0 || paymentDateCol == 0)
            throw new UserFriendlyException(L["Payment:ExcelMissingHeader"].Value);

        var paymentMethods = await (await ResPaymentMethodRepository.GetQueryableAsync())
            .Where(x => !x.IsDeleted && x.Status == ResPaymentMethodStatus.Active)
            .ToListAsync();
        var paymentMethodById = paymentMethods.ToDictionary(x => x.Id, x => x.Id);
        var paymentMethodByCode = paymentMethods.ToDictionary(x => (x.Code ?? "").Trim().ToUpperInvariant(), x => x.Id);
        var paymentMethodByName = paymentMethods.ToDictionary(x => (x.Name ?? "").Trim().ToUpperInvariant(), x => x.Id);

        var debtPaymentType = await ResPaymentTypeRepository.FirstOrDefaultAsync(x =>
            !x.IsDeleted && x.Status == ResPaymentTypeStatus.Active && x.Code == PaymentTypeDebtCode);
        if (debtPaymentType == null)
            throw new UserFriendlyException(L["Payment:DebtPaymentTypeNotFound"].Value);

        if (CurrentUser.Id == null)
            throw new UserFriendlyException(L["UserNotAuthenticated"].Value);
        var employee = await HrEmployeeRepository.FirstOrDefaultAsync(e => e.UserId == CurrentUser.Id.Value);
        if (employee == null)
            throw new UserFriendlyException(L["EmployeeNotFoundForUser"].Value);

        int rowNumber = 2;
        while (!worksheet.Row(rowNumber).IsEmpty())
        {
            var row = worksheet.Row(rowNumber);
            var policyNo = GetExcelCellString(row, policyNoCol).Trim();
            var amountText = GetExcelCellString(row, amountCol).Trim();
            var paymentMethodText = GetExcelCellString(row, paymentMethodCol).Trim();
            var paymentDateText = GetExcelCellString(row, paymentDateCol).Trim();

            // Bỏ qua dòng thiếu giá trị bắt buộc - chỉ xử lý dòng có đủ 4 trường
            if (string.IsNullOrWhiteSpace(policyNo) || string.IsNullOrWhiteSpace(amountText) ||
                string.IsNullOrWhiteSpace(paymentMethodText) || string.IsNullOrWhiteSpace(paymentDateText))
            {
                rowNumber++;
                continue;
            }

            result.TotalRows++;
            var rowErrors = new List<ImportPaymentExcelErrorDto>();

            Guid? paymentMethodId = null;
            if (Guid.TryParse(paymentMethodText.Trim(), out var guidVal) && paymentMethodById.ContainsKey(guidVal))
                paymentMethodId = guidVal;
            else
            {
                var key = paymentMethodText.Trim().ToUpperInvariant();
                if (paymentMethodByCode.TryGetValue(key, out var idByCode))
                    paymentMethodId = idByCode;
                else if (paymentMethodByName.TryGetValue(key, out var idByName))
                    paymentMethodId = idByName;
            }
            if (!paymentMethodId.HasValue)
            {
                rowErrors.Add(new ImportPaymentExcelErrorDto { RowNumber = rowNumber, Field = "Hình thức thanh toán", Message = L["Payment:PaymentMethodNotFound"].Value, Value = paymentMethodText });
            }

            decimal amount = 0;
            if (!decimal.TryParse(amountText.Replace(",", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out amount))
            {
                rowErrors.Add(new ImportPaymentExcelErrorDto { RowNumber = rowNumber, Field = "Số tiền thanh toán", Message = L["Payment:AmountInvalid"].Value, Value = amountText });
            }
            else if (amount <= 0)
            {
                rowErrors.Add(new ImportPaymentExcelErrorDto { RowNumber = rowNumber, Field = "Số tiền thanh toán", Message = L["Payment:AmountMustBePositive"].Value });
            }

            DateTime? paymentDate = null;
            if (DateTime.TryParse(paymentDateText, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                paymentDate = dt.Date;
            else if (double.TryParse(paymentDateText.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out var oaDate))
                paymentDate = DateTime.FromOADate(oaDate);
            else
                rowErrors.Add(new ImportPaymentExcelErrorDto { RowNumber = rowNumber, Field = "Ngày thanh toán", Message = L["Payment:DateInvalid"].Value, Value = paymentDateText });

            if (rowErrors.Count > 0)
            {
                result.ErrorCount++;
                result.Errors.AddRange(rowErrors);
            }
            else
            {
                var policyQuery = await PolicyRepository.GetQueryableAsync();
                var policy = await AsyncExecuter.FirstOrDefaultAsync(
                    policyQuery.Where(x => x.PolicyNo == policyNo));
                if (policy == null)
                {
                    result.ErrorCount++;
                    result.Errors.Add(new ImportPaymentExcelErrorDto { RowNumber = rowNumber, Field = "Mã đơn", Message = L["Payment:PolicyNotFound"].Value, Value = policyNo });
                }
                else
                {
                    try
                    {
                        await CreatePaymentRequestAsync(policy.Id, new CreatePaymentRequestInput
                        {
                            PaymentMethodId = paymentMethodId!.Value,
                            Amount = amount,
                            PaymentDate = paymentDate!.Value
                        });
                        result.SuccessCount++;
                    }
                    catch (UserFriendlyException ex)
                    {
                        result.ErrorCount++;
                        result.Errors.Add(new ImportPaymentExcelErrorDto { RowNumber = rowNumber, Field = "", Message = ex.Message, Value = policyNo });
                    }
                }
            }

            rowNumber++;
        }

        return result;
    }

    private static string GetExcelCellString(IXLRow row, int column)
    {
        var cell = row.Cell(column);
        if (cell.DataType == XLDataType.DateTime)
            return cell.GetDateTime().ToString("yyyy-MM-dd");
        if (cell.DataType == XLDataType.Number)
            return cell.GetDouble().ToString(CultureInfo.InvariantCulture);
        return cell.GetString();
    }

    /// <summary>
    /// Đọc cột mã hợp đồng (Số HĐ): tránh coi ô là số rồi đọc qua double (mất số 0 đầu, sai mã dài).
    /// Ưu tiên text gốc hoặc chuỗi hiển thị trong Excel (định dạng sẵn có).
    /// </summary>
    private static string GetExcelCellStringForContractCode(IXLRow row, int column)
    {
        var cell = row.Cell(column);
        if (cell.IsEmpty())
            return string.Empty;

        if (cell.DataType == XLDataType.Text)
            return cell.GetString().Trim();

        var formatted = cell.GetFormattedString();
        if (!string.IsNullOrWhiteSpace(formatted))
            return formatted.Trim();

        return GetExcelCellString(row, column).Trim();
    }

    private static string NormalizeImportedContractCode(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;
        // NBSP / khoảng đặc biệt thường gặp khi copy từ Excel hoặc web
        return value.Replace('\u00A0', ' ').Trim();
    }

    /// <summary>
    /// Map cột "Loại hợp đồng" từ Excel (khớp template xuất individual/group; lẻ/nhóm/bao; 0/1).
    /// Ô trống → Individual. Trả false nếu có nội dung nhưng không nhận dạng được.
    /// </summary>
    private static bool TryMapPolicyContractTypeFromImport(string? cellValue, out PolicyContractType type)
    {
        type = PolicyContractType.Individual;
        if (string.IsNullOrWhiteSpace(cellValue))
            return true;

        var t = cellValue.Trim();
        if (Enum.TryParse<PolicyContractType>(t, ignoreCase: true, out type))
            return true;

        if (int.TryParse(t, NumberStyles.Integer, CultureInfo.InvariantCulture, out var ni) &&
            (ni == (int)PolicyContractType.Individual || ni == (int)PolicyContractType.Group))
        {
            type = (PolicyContractType)ni;
            return true;
        }

        var n = RemoveDiacritics(t.ToUpperInvariant()).Replace(" ", "", StringComparison.Ordinal);

        if (n.Contains("GROUP", StringComparison.Ordinal) || n.Contains("NHOM", StringComparison.Ordinal) ||
            n.Contains("BAO", StringComparison.Ordinal) || n.Contains("TAPTHE", StringComparison.Ordinal) ||
            n.Contains("DONGBAO", StringComparison.Ordinal) || n.Contains("DONGNHOM", StringComparison.Ordinal))
        {
            type = PolicyContractType.Group;
            return true;
        }

        if (n.Contains("INDIVIDUAL", StringComparison.Ordinal) || n.Contains("DONGLE", StringComparison.Ordinal) ||
            n == "LE" || n.Contains("CANHAN", StringComparison.Ordinal))
        {
            type = PolicyContractType.Individual;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Parse số tiền từ ô Excel (vi-VN / Anh-Mỹ / bỏ dấu ngăn cách).
    /// </summary>
    private static bool TryParseDecimalFromExcelImport(string? raw, out decimal value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(raw))
            return false;
        var text = raw.Trim().Replace('\u00A0', ' ').Trim();
        if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.GetCultureInfo("vi-VN"), out value))
            return true;
        if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
            return true;
        var noCommaThousands = text.Replace(",", "", StringComparison.Ordinal);
        return decimal.TryParse(noCommaThousands, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
    }

    /// <summary>
    /// Chuyển giá trị nội bộ Y/N sang giá trị hiển thị trong Excel: Có/Không.
    /// </summary>
    private static string YesNoToExcelDisplay(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "Không";
        return string.Equals(value.Trim(), "Y", StringComparison.OrdinalIgnoreCase) ? "Có" : "Không";
    }

    /// <summary>
    /// Parse giá trị Có/Không hoặc Y/N từ ô Excel sang giá trị nội bộ "Y" hoặc "N".
    /// </summary>
    private static string ParseYesNoFromExcel(string? cellValue)
    {
        if (string.IsNullOrWhiteSpace(cellValue)) return "N";
        var v = cellValue.Trim();
        if (string.Equals(v, "Có", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(v, "Y", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(v, "Yes", StringComparison.OrdinalIgnoreCase) ||
            v == "1" ||
            string.Equals(v, "True", StringComparison.OrdinalIgnoreCase))
            return "Y";
        return "N";
    }

    /// <summary>
    /// Loại bỏ dấu tiếng Việt để so sánh header thống nhất giữa export và import.
    /// Xử lý cả Đ/đ (không phân tách được bằng NFD).
    /// </summary>
    private static string RemoveDiacritics(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        text = text.Replace('Đ', 'D').Replace('đ', 'd');
        var normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    #endregion

    #region Import Policy Excel

    /// <summary>
    /// Danh sách tên cột cố định trong template (theo thứ tự).
    /// </summary>
    private static readonly string[] FixedColumnHeaders = new[]
    {
        "STT",
        "Đối tác BH gốc",
        "Số HĐ",
        "Loại hợp đồng",
        "Nghiệp vụ BH",
        "Tên Khách hàng",
        "Số ĐT khách hàng",
        "Email khách hàng",
        "Loại khách hàng",
        "Tỉnh/Thành",
        "Phường xã",
        "Địa chỉ",
        "CCCD",
        "Mã số thuế",
        "Tên Người thanh toán",
        "Số ĐT Người thanh toán",
        "Email Người thanh toán",
        "Loại Người thanh toán",
        "Tỉnh/Thành Người TT",
        "Phường xã Người TT",
        "Địa chỉ Người TT",
        "CCCD Người TT",
        "Mã số thuế Người TT",
        "Nhận hóa đơn",
        "Số đơn BH gốc",
        "Số GCN",
        "Có sử dụng khoản vay?",
        "Tên Người Thụ hưởng",
        "Số ĐT Người TH",
        "Email Người TH",
        "Loại Người TH",
        "Tỉnh/Thành Người TH",
        "Phường xã Người TH",
        "Địa chỉ Người TH",
        "CCCD Người TH",
        "Mã số thuế Người TH",
        "Tiền tệ",
        "Kênh khai thác",
        "Người khai thác",
        "Người cấp đơn",
        "Tên Chủ xe",
        "Số ĐT Chủ xe",
        "CCCD Chủ xe",
        "Email Chủ xe",
        "Tỉnh/Thành Chủ xe",
        "Phường xã Chủ xe",
        "Địa chỉ Chủ xe",
        "Mục đích kinh doanh",
        "Hãng xe",
        "Hiệu xe",
        "Số chỗ ngồi",
        "Trọng tải (tấn)",
        "Xe mới",
        "Giá trị xe\n(VNĐ)",
        "Biển số xe",
        "Số khung",
        "Số máy",
        "Năm sản xuất/Đăng ký lần đầu",
        "Dòng xe",
        "Nhóm xe",
        "Loại xe",
        "Thời hạn bảo hiểm (từ)\n(giờ:phút ngày/tháng/năm)",
        "Thời hạn bảo hiểm (đến)\n(giờ:phút ngày/tháng/năm)",
    };

    /// <summary>Mã <see cref="ProProductType.Code"/> cho layout import/export TNDS bắt buộc.</summary>
    private const string ImportTemplateProductTypeCodeTndsbb = "TNDSBB";
    private const string ImportTemplateProductTypeCodeVcx = "VCX";

    /// <summary>TNDSBB: cột cố định đứng trước nhóm cột sản phẩm động.</summary>
    private static readonly string[] TndsbbFixedColumnHeadersBeforeProductColumns = new[]
    {
        "STT",
        "Đối tác BH gốc",
        "Số HĐ",
        "Loại hợp đồng",
        "Nghiệp vụ BH",
        "Số đơn BH gốc",
        "Số GCN",
        "Người khai thác",
        "Người cấp đơn",
        "Kênh khai thác",
        "Tên Chủ xe",
        "Số ĐT Chủ xe",
        "CCCD Chủ xe",
        "Email Chủ xe",
        "Tỉnh/Thành Chủ xe",
        "Phường xã Chủ xe",
        "Địa chỉ Chủ xe",
        "Mục đích kinh doanh",
        "Hãng xe",
        "Hiệu xe",
        "Số chỗ ngồi",
        "Trọng tải (tấn)",
        "Biển số xe",
        "Số khung",
        "Số máy",
        "Năm sản xuất/Đăng ký lần đầu",
        "Dòng xe",
        "Nhóm xe",
        "Loại xe",
        "Thời hạn bảo hiểm (từ)\n(giờ:phút ngày/tháng/năm)",
        "Thời hạn bảo hiểm (đến)\n(giờ:phút ngày/tháng/năm)",
    };

    /// <summary>TNDSBB: cột cố định đứng sau [sản phẩm động + phí].</summary>
    private static readonly string[] TndsbbFixedColumnHeadersAfterProductColumns = new[]
    {
        "Nhận hóa đơn",
        "Tên Người thanh toán",
        "Số ĐT Người thanh toán",
        "Email Người thanh toán",
        "Loại Người thanh toán",
        "Tỉnh/Thành Người TT",
        "Phường xã Người TT",
        "Địa chỉ Người TT",
        "CCCD Người TT",
        "Mã số thuế Người TT",
    };

    /// <summary>Tập cột cố định TNDSBB để import nhận diện theo tên cột (không phụ thuộc vị trí).</summary>
    private static readonly string[] TndsbbFixedColumnHeaders =
        TndsbbFixedColumnHeadersBeforeProductColumns.Concat(TndsbbFixedColumnHeadersAfterProductColumns).ToArray();

    /// <summary>VCX: cột cố định đứng trước nhóm cột sản phẩm động.</summary>
    private static readonly string[] VcxFixedColumnHeadersBeforeProductColumns = new[]
    {
        "STT",
        "Đối tác BH gốc",
        "Số HĐ",
        "Loại hợp đồng",
        "Nghiệp vụ BH",
        "Số đơn BH gốc",
        "Số GCN",
        "Kênh khai thác",
        "Người khai thác",
        "Người cấp đơn",
        "Tên Chủ xe",
        "Số ĐT Chủ xe",
        "CCCD Chủ xe",
        "Email Chủ xe",
        "Tỉnh/Thành Chủ xe",
        "Phường xã Chủ xe",
        "Địa chỉ Chủ xe",
        "Mục đích kinh doanh",
        "Hãng xe",
        "Hiệu xe",
        "Số chỗ ngồi",
        "Trọng tải (tấn)",
        "Xe mới",
        "Giá trị xe\n(VNĐ)",
        "Biển số xe",
        "Số khung",
        "Số máy",
        "Năm sản xuất/Đăng ký lần đầu",
        "Dòng xe",
        "Nhóm xe",
        "Loại xe",
        "Thời hạn bảo hiểm (từ)\n(giờ:phút ngày/tháng/năm)",
        "Thời hạn bảo hiểm (đến)\n(giờ:phút ngày/tháng/năm)",
    };

    /// <summary>VCX: cột cố định đứng sau [sản phẩm động + phí].</summary>
    private static readonly string[] VcxFixedColumnHeadersAfterProductColumns = new[]
    {
        "Có sử dụng khoản vay?",
        "Tên Người Thụ hưởng",
        "Số ĐT Người TH",
        "Email Người TH",
        "Loại Người TH",
        "Tỉnh/Thành Người TH",
        "Phường xã Người TH",
        "Địa chỉ Người TH",
        "CCCD Người TH",
        "Mã số thuế Người TH",
        "Nhận hóa đơn",
        "Tên Người thanh toán",
        "Số ĐT Người thanh toán",
        "Email Người thanh toán",
        "Loại Người thanh toán",
        "Tỉnh/Thành Người TT",
        "Phường xã Người TT",
        "Địa chỉ Người TT",
        "CCCD Người TT",
        "Mã số thuế Người TT",
        "Tên Khách hàng",
        "Số ĐT khách hàng",
        "Email khách hàng",
        "Loại khách hàng",
        "Tỉnh/Thành",
        "Phường xã",
        "Địa chỉ",
        "CCCD",
        "Mã số thuế",
    };

    private static readonly string[] VcxFixedColumnHeaders =
        VcxFixedColumnHeadersBeforeProductColumns.Concat(VcxFixedColumnHeadersAfterProductColumns).ToArray();

    private async Task<(string[] Headers, bool IsTndsbbLayout, bool IsVcxLayout)> ResolvePolicyImportTemplateLayoutAsync(Guid? productTypeId)
    {
        if (!productTypeId.HasValue || productTypeId.Value == Guid.Empty)
            return (FixedColumnHeaders, false, false);

        var productType = await ProProductTypeRepository.FindAsync(productTypeId.Value);
        if (productType == null)
            return (FixedColumnHeaders, false, false);

        var code = (productType.Code ?? "").Trim();
        if (string.Equals(code, ImportTemplateProductTypeCodeTndsbb, StringComparison.OrdinalIgnoreCase))
            return (TndsbbFixedColumnHeaders, true, false);

        if (string.Equals(code, ImportTemplateProductTypeCodeVcx, StringComparison.OrdinalIgnoreCase))
            return (VcxFixedColumnHeaders, false, true);

        return (FixedColumnHeaders, false, false);
    }

    // After product columns, these 2 columns follow
    private const string DiscountColumnHeader = "Giảm phí";
    private const string PremiumColumnHeader = "Phí BH";

    public virtual async Task<byte[]> ExportPolicyImportTemplateAsync(Guid? contractId = null, Guid? insurerId = null, Guid? productTypeId = null)
    {
        var (templateFixedHeaders, isTndsbbLayout, isVcxLayout) = await ResolvePolicyImportTemplateLayoutAsync(productTypeId);

        // 1. Load contract first (if provided) to decide product list by Đối tác BH gốc + LOB
        iOne.PolicyContracts.PolicyContract? contract = null;
        string insurerCode = "", contractCode = "", contractTypeStr = "", lobCode = "";
        string customerName = "", customerPhone = "", customerEmail = "", customerOrgType = "";
        string customerProvince = "", customerWard = "", customerAddress = "", customerIdNo = "", customerTin = "";
        string payerName = "", payerPhone = "", payerEmail = "";
        string payerProvince = "", payerWard = "", payerAddress = "", payerTin = "";
        string isReceiveInvoice = "", currencyCode = "", employeeCode = "";
        Guid? resolvedLobId = null;
        Guid? resolvedInsurerId = null;
        ResPartner? resolvedInsurerPartnerForTemplate = null;

        if (contractId.HasValue && contractId.Value != Guid.Empty)
        {
            var contractQuery = await PolicyContractRepository.GetQueryableAsync();
            contract = await AsyncExecuter.FirstOrDefaultAsync(
                contractQuery
                    .Include(c => c.Insurer)
                    .Include(c => c.Customer).ThenInclude(cust => cust.OrganizationType)
                    .Include(c => c.Lob)
                    .Where(c => c.Id == contractId.Value));

            if (contract != null)
            {
                insurerCode = contract.Insurer?.Code ?? "";
                contractCode = contract.Code ?? "";
                contractTypeStr = contract.Type.ToString().ToLowerInvariant(); // "individual" or "group"
                lobCode = contract.Lob?.Code ?? "";
            }
        }
        else if (insurerId.HasValue && insurerId.Value != Guid.Empty)
        {
            // Không có contractId: dùng insurerId + LOB mặc định mã CAR để gen template
            resolvedInsurerId = insurerId.Value;
            lobCode = "CAR";

            resolvedInsurerPartnerForTemplate = await ResPartnerRepository.FindAsync(resolvedInsurerId.Value);
            insurerCode = resolvedInsurerPartnerForTemplate?.Code ?? "";

            var lobQuery = await ProLineOfBusinessRepository.GetQueryableAsync();
            var lob = await AsyncExecuter.FirstOrDefaultAsync(lobQuery.Where(l => l.Code == "CAR"));
            if (lob != null)
                resolvedLobId = lob.Id;
        }

        // 2. Load products: theo LOB + đối tác (không lọc kênh — template import); nếu không đủ ngữ cảnh thì lấy tất cả SP active
        List<(Guid Id, string Code, string Name)> products;
        if (contract != null && contract.LobId.HasValue && contract.LobId.Value != Guid.Empty
            && contract.InsurerId.HasValue && contract.InsurerId.Value != Guid.Empty)
        {
            var dtos = await _proProductAppService.GetByLobIdAndPartnerIdAsync(
                contract.LobId.Value,
                contract.InsurerId.Value,
                channelId: null,
                appChannelId: null,
                applyChannelDistributionFilter: false);
            products = dtos
                .OrderBy(p => p.Code)
                .Select(p => (p.Id, Code: p.Code ?? "", Name: p.Name ?? p.ShortName ?? ""))
                .ToList();
        }
        else if (resolvedLobId.HasValue && resolvedInsurerId.HasValue)
        {
            var dtos = await _proProductAppService.GetByLobIdAndPartnerIdAsync(
                resolvedLobId.Value,
                resolvedInsurerId.Value,
                channelId: null,
                appChannelId: null,
                applyChannelDistributionFilter: false);
            products = dtos
                .OrderBy(p => p.Code)
                .Select(p => (p.Id, Code: p.Code ?? "", Name: p.Name ?? p.ShortName ?? ""))
                .ToList();
        }
        else
        {
            var productQuery = await ProProductRepository.GetQueryableAsync();
            var productList = await AsyncExecuter.ToListAsync(
                productQuery.Where(p => !p.IsDeleted && p.Status == ProProductStatus.Active)
                    .OrderBy(p => p.Code)
                    .Select(p => new { p.Id, p.Code, p.Name })
            );
            products = productList.Select(p => (p.Id, Code: p.Code ?? "", Name: p.Name ?? "")).ToList();
        }

        if (productTypeId.HasValue && productTypeId.Value != Guid.Empty)
        {
            var allowedProductQuery = await ProProductRepository.GetQueryableAsync();
            var allowedIds = await AsyncExecuter.ToListAsync(
                allowedProductQuery
                    .Where(p => !p.IsDeleted && p.Status == ProProductStatus.Active && p.ProductTypeId == productTypeId.Value)
                    .Select(p => p.Id));
            var allowedSet = allowedIds.ToHashSet();
            products = products.Where(p => allowedSet.Contains(p.Id)).ToList();
        }

        if (contract != null)
        {
            // Customer info → Insured fields
            var cust = contract.Customer;
            if (cust != null)
            {
                customerName = cust.Name ?? "";
                customerPhone = cust.Phone ?? "";
                customerEmail = cust.Email ?? "";
                customerIdNo = cust.IdNo ?? "";
                customerTin = cust.Tin ?? "";
                customerAddress = cust.Address ?? "";

                if (cust.ProvinceId.HasValue && cust.ProvinceId.Value != Guid.Empty)
                {
                    var prov = await ResProvinceRepository.FindAsync(cust.ProvinceId.Value);
                    customerProvince = prov?.Code ?? "";
                }
                if (cust.WardId.HasValue && cust.WardId.Value != Guid.Empty)
                {
                    var ward = await ResWardRepository.FindAsync(cust.WardId.Value);
                    customerWard = ward?.Code ?? "";
                }
                customerOrgType = cust.OrganizationType?.Code ?? "";
            }

            payerName = contract.PayerName ?? "";
            payerPhone = contract.PayerPhone ?? "";
            payerEmail = contract.PayerEmail ?? "";
            payerAddress = contract.PayerAddress ?? "";
            payerTin = contract.PayerTin ?? "";

            if (contract.PayerProvinceId.HasValue && contract.PayerProvinceId.Value != Guid.Empty)
            {
                var prov = await ResProvinceRepository.FindAsync(contract.PayerProvinceId.Value);
                payerProvince = prov?.Code ?? "";
            }
            if (contract.PayerWardId.HasValue && contract.PayerWardId.Value != Guid.Empty)
            {
                var ward = await ResWardRepository.FindAsync(contract.PayerWardId.Value);
                payerWard = ward?.Code ?? "";
            }

            isReceiveInvoice = YesNoToExcelDisplay(contract.IsReciveInvoice);

            if (contract.EmployeeId.HasValue && contract.EmployeeId.Value != Guid.Empty)
            {
                var emp = await HrEmployeeRepository.FindAsync(contract.EmployeeId.Value);
                employeeCode = emp?.Code ?? "";
            }

            currencyCode = "VND";
        }

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Sheet1");

        var leadingFixedHeaders = isTndsbbLayout
            ? TndsbbFixedColumnHeadersBeforeProductColumns
            : isVcxLayout
                ? VcxFixedColumnHeadersBeforeProductColumns
                : templateFixedHeaders;
        var trailingFixedHeaders = isTndsbbLayout
            ? TndsbbFixedColumnHeadersAfterProductColumns
            : isVcxLayout
                ? VcxFixedColumnHeadersAfterProductColumns
                : Array.Empty<string>();

        // 3. Write leading fixed headers
        for (int i = 0; i < leadingFixedHeaders.Length; i++)
        {
            ws.Cell(1, i + 1).Value = leadingFixedHeaders[i];
        }

        // 4. Write product code columns (dynamic)
        int productColStart = leadingFixedHeaders.Length + 1;
        for (int i = 0; i < products.Count; i++)
        {
            var cell = ws.Cell(1, productColStart + i);
            cell.Value = products[i].Code;
            // Add comment with product name for clarity
            cell.GetComment().AddText(products[i].Name);
        }

        // 5. Write premium/discount columns.
        int trailingStart = productColStart + products.Count;
        if (isTndsbbLayout || isVcxLayout)
        {
            ws.Cell(1, trailingStart).Value = PremiumColumnHeader;
            ws.Cell(1, trailingStart + 1).Value = DiscountColumnHeader;
        }
        else
        {
            ws.Cell(1, trailingStart).Value = DiscountColumnHeader;
            ws.Cell(1, trailingStart + 1).Value = PremiumColumnHeader;
        }

        // 6. Write trailing fixed headers for layouts that need it (e.g. TNDSBB).
        int trailingFixedStart = trailingStart + 2;
        for (int i = 0; i < trailingFixedHeaders.Length; i++)
        {
            ws.Cell(1, trailingFixedStart + i).Value = trailingFixedHeaders[i];
        }

        // 7. Style header row
        var headerRange = ws.Range(1, 1, 1, trailingFixedStart + trailingFixedHeaders.Length - 1);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
        headerRange.Style.Alignment.WrapText = true;
        headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

        // 8. Pre-fill contract data in row 2 if contract was loaded
        if (contract != null)
        {
            int r = 2;
            // Helper to set cell by header name
            void SetCell(string header, string value)
            {
                if (string.IsNullOrWhiteSpace(value))
                    return;

                var idx = Array.IndexOf(leadingFixedHeaders, header);
                if (idx >= 0)
                {
                    ws.Cell(r, idx + 1).Value = value;
                    return;
                }

                idx = Array.IndexOf(trailingFixedHeaders, header);
                if (idx >= 0)
                    ws.Cell(r, trailingFixedStart + idx).Value = value;
            }

            SetCell("STT", "1");
            SetCell("Đối tác BH gốc", insurerCode);
            SetCell("Số HĐ", contractCode);
            SetCell("Loại hợp đồng", contractTypeStr);
            SetCell("Nghiệp vụ BH", lobCode);

            // Customer → Insured
            SetCell("Tên Khách hàng", customerName);
            SetCell("Số ĐT khách hàng", customerPhone);
            SetCell("Email khách hàng", customerEmail);
            SetCell("Loại khách hàng", customerOrgType);
            SetCell("Tỉnh/Thành", customerProvince);
            SetCell("Phường xã", customerWard);
            SetCell("Địa chỉ", customerAddress);
            SetCell("CCCD", customerIdNo);
            SetCell("Mã số thuế", customerTin);

            // Payer
            SetCell("Tên Người thanh toán", payerName);
            SetCell("Số ĐT Người thanh toán", payerPhone);
            SetCell("Email Người thanh toán", payerEmail);
            SetCell("Địa chỉ Người TT", payerAddress);
            SetCell("Tỉnh/Thành Người TT", payerProvince);
            SetCell("Phường xã Người TT", payerWard);
            SetCell("Mã số thuế Người TT", payerTin);
            SetCell("Nhận hóa đơn", isReceiveInvoice);

            // Currency & Employee
            SetCell("Tiền tệ", currencyCode);
            SetCell("Người khai thác", employeeCode);
        }
        else if (!string.IsNullOrEmpty(insurerCode) || !string.IsNullOrEmpty(lobCode))
        {
            // Không có HĐ nhưng có insurerId: điền Đối tác BH gốc + Nghiệp vụ BH (LOB CAR) vào dòng 2
            int r = 2;
            void SetCellPartial(string header, string value)
            {
                var idx = Array.IndexOf(leadingFixedHeaders, header);
                if (idx >= 0 && !string.IsNullOrWhiteSpace(value))
                    ws.Cell(r, idx + 1).Value = value;
            }
            SetCellPartial("STT", "1");
            SetCellPartial("Đối tác BH gốc", insurerCode);
            SetCellPartial("Nghiệp vụ BH", lobCode);
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public virtual async Task<ImportPolicyExcelResultDto> ImportPolicyExcelAsync(Guid? contractId, Guid? insurerId, byte[] fileBytes, Guid? productTypeId = null)
    {
        _ = insurerId; // kept for API/query compatibility; insurer vs contract is validated from Excel columns only

        var result = new ImportPolicyExcelResultDto();
        var errors = new List<ImportPolicyExcelErrorDto>();
        var rowErrorDetails = new Dictionary<int, List<string>>();

        // 1. Load contract (optional): có contractId thì dùng HĐ có sẵn; không thì mỗi dòng sẽ tạo HĐ mới qua CreateAsync
        iOne.PolicyContracts.PolicyContract? contract = null;
        if (contractId.HasValue && contractId.Value != Guid.Empty)
        {
            contract = await PolicyContractRepository.GetAsync(contractId.Value);
            if (contract == null)
                throw new UserFriendlyException(L["Policy:ImportPolicy:ContractNotFound"]);
        }

        // 2. Parse Excel
        using var stream = new MemoryStream(fileBytes);
        using var workbook = new XLWorkbook(stream);
        var ws = workbook.Worksheets.FirstOrDefault(w =>
            string.Equals(w.Name, "Sheet1", StringComparison.OrdinalIgnoreCase)) ?? workbook.Worksheets.FirstOrDefault();
        if (ws == null)
            throw new UserFriendlyException(L["Policy:ImportPolicy:ExcelFileInvalid"]);

        var (importTemplateFixedHeaders, isTndsbbImportLayout, isVcxImportLayout) = await ResolvePolicyImportTemplateLayoutAsync(productTypeId);

        // 3. Parse header row — detect column positions
        var headerRow = ws.Row(1);
        var colMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var productCodeColumns = new Dictionary<int, string>(); // col index -> product code
        int discountCol = 0, premiumCol = 0;

        for (int col = 1; col <= 200; col++)
        {
            var headerValue = headerRow.Cell(col).GetString().Trim();
            if (string.IsNullOrWhiteSpace(headerValue)) continue;

            var normalized = RemoveDiacritics(headerValue.Replace("\n", " ").Replace("\r", " ").Replace("  ", " ").Trim().ToUpperInvariant());

            if (normalized == RemoveDiacritics(DiscountColumnHeader.ToUpperInvariant()))
            {
                discountCol = col;
                continue;
            }
            if (normalized == RemoveDiacritics(PremiumColumnHeader.ToUpperInvariant()))
            {
                premiumCol = col;
                continue;
            }

            // Check if this is a fixed column
            bool isFixed = false;
            foreach (var fixedHeader in importTemplateFixedHeaders)
            {
                var normalizedFixed = RemoveDiacritics(fixedHeader.Replace("\n", " ").Replace("\r", " ").Replace("  ", " ").Trim().ToUpperInvariant());
                if (normalized == normalizedFixed)
                {
                    colMap[fixedHeader] = col;
                    isFixed = true;
                    break;
                }
            }

            if (!isFixed && discountCol == 0 && premiumCol == 0)
            {
                // Assume this is a product code column
                productCodeColumns[col] = headerValue.Trim();
            }
        }

        // 4. Build lookup dictionaries (code → ID)
        var partnerLookup = await BuildLookupAsync<ResPartner, Guid>(ResPartnerRepository, p => p.Code ?? "", p => p.Id);
        var lobLookup = await BuildLookupAsync<ProLineOfBusiness, Guid>(ProLineOfBusinessRepository, l => l.Code, l => l.Id);
        var currencyLookup = await BuildLookupAsync<ResCurrency, Guid>(ResCurrencyRepository, c => c.Code, c => c.Id);
        var channelLookup = await BuildLookupAsync(ResChannelRepository, c => c.Code, c => c.Id);
        var employeeLookup = await BuildLookupAsync<HrEmployee, Guid>(HrEmployeeRepository, e => e.Code, e => e.Id);
        var provinceLookup = await BuildLookupAsync(ResProvinceRepository, p => p.Code, p => p.Id, p => p.Status == ResProvinceStatus.Active);
        var wardLookup = await BuildLookupAsync(ResWardRepository, w => w.Code, w => w.Id, w => w.Status == ResWardStatus.Active);
        var provinceQueryableForNames = await ResProvinceRepository.GetQueryableAsync();
        var provinceIdToName = (await AsyncExecuter.ToListAsync(provinceQueryableForNames.Where(p => !p.IsDeleted)))
            .ToDictionary(p => p.Id, p => p.Name);
        var wardQueryableForNames = await ResWardRepository.GetQueryableAsync();
        var wardIdToName = (await AsyncExecuter.ToListAsync(wardQueryableForNames.Where(w => !w.IsDeleted)))
            .ToDictionary(w => w.Id, w => w.Name);
        var carBrandLookup = await BuildLookupAsync<ResCarBrand, Guid>(ResCarBrandRepository, b => b.Code, b => b.Id);
        var carCategoryLookup = await BuildLookupAsync<ResCarCategory, Guid>(ResCarCategoryRepository, c => c.Code, c => c.Id);
        var carLineLookup = await BuildLookupAsync<ResCarLine, Guid>(ResCarLineRepository, l => l.Code, l => l.Id);
        var carGroupLookup = await BuildLookupAsync<ResCarGroup, Guid>(ResCarGroupRepository, g => g.Code, g => g.Id);
        var carTypeLookup = await BuildLookupAsync<ResCarType, Guid>(ResCarTypeRepository, t => t.Code, t => t.Id);

        // PolicyType lookup: find the default policy type (code BHG)
        var policyTypeQuery = await PolicyTypeRepository.GetQueryableAsync();
        var newPolicyType = await AsyncExecuter.FirstOrDefaultAsync(
            policyTypeQuery.Where(pt => pt.Code == "BHG" && !pt.IsDeleted));
        var newPolicyTypeId = newPolicyType?.Id ?? Guid.Empty;

        // ObjectType lookup: find the "CAR" object type
        var objectTypeQuery = await ResObjectTypeRepository.GetQueryableAsync();
        var carObjectType = await AsyncExecuter.FirstOrDefaultAsync(
            objectTypeQuery.Where(ot => ot.Code == "CAR" && !ot.IsDeleted));
        var carObjectTypeId = carObjectType?.Id ?? Guid.Empty;

        // Product lookup: code → ProProduct (optional filter theo loại sản phẩm — khớp template đã tải)
        var productQuery = await ProProductRepository.GetQueryableAsync();
        var productQueryable = productQuery.Where(p => !p.IsDeleted && p.Status == ProProductStatus.Active);
        if (productTypeId.HasValue && productTypeId.Value != Guid.Empty)
            productQueryable = productQueryable.Where(p => p.ProductTypeId == productTypeId.Value);
        var allProducts = await AsyncExecuter.ToListAsync(productQueryable);
        var productByCode = allProducts.ToDictionary(p => p.Code.Trim().ToUpperInvariant(), p => p);

        // ProductCoverage lookup: productId → { coverageCode → ProProductCoverage }
        var productCoverageQuery = await ProProductCoverageRepository.GetQueryableAsync();
        var allProductCoverages = await AsyncExecuter.ToListAsync(
            productCoverageQuery
                .Include(pc => pc.Coverage)
                .Where(pc => !pc.IsDeleted));
        var productCoverageMap = allProductCoverages
            .GroupBy(pc => pc.ProductId)
            .ToDictionary(
                g => g.Key,
                g => g.Where(pc => pc.Coverage != null)
                      .GroupBy(pc => pc.Coverage!.Code.Trim().ToUpperInvariant())
                      .ToDictionary(
                          cg => cg.Key,
                          cg => cg.First(),
                          StringComparer.OrdinalIgnoreCase));

        // Cache customers created during this import (CCCD/IdNo -> customerId)
        var createdCustomerIdByIdNo = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);

        // One random import lot code per Excel file run; all policies created in this import share it.
        var importLotCode = GenerateRandomLotImportBaseCode();
        result.ImportLotCode = importLotCode;

        // 4b. Khi import gắn hợp đồng: đếm số dòng dữ liệu, check (số đơn hiện có + số dòng) <= Quantity, nếu vượt thì bắn lỗi luôn
        if (contract != null)
        {
            var dataRowCount = 0;
            for (int r = 2; r <= 100_000; r++)
            {
                if (ws.Row(r).IsEmpty()) break;
                dataRowCount++;
            }
            var currentPolicyCount = await PolicyRepository.CountAsync(p => p.ContractId == contract.Id && !p.IsDeleted);
            if ((decimal)(currentPolicyCount + dataRowCount) > contract.Quantity)
                throw new UserFriendlyException(L["Policy:ImportPolicy:ImportExceedsContractQuantity"].Value);
        }

        // 5. Process each data row
        int rowNumber = 2;
        while (!ws.Row(rowNumber).IsEmpty())
        {
            var row = ws.Row(rowNumber);
            result.TotalRows++;
            var rowErrors = new List<ImportPolicyExcelErrorDto>();

            try
            {
                // 5a. Thu thập tất cả cột sản phẩm có dữ liệu (không chỉ lấy cột đầu tiên)
                var productDataList = new List<(string ProductCode, string CoverageCellValue)>();
                foreach (var (col, code) in productCodeColumns)
                {
                    var cellVal = GetExcelCellString(row, col).Trim();
                    if (!string.IsNullOrWhiteSpace(cellVal))
                        productDataList.Add((code, cellVal));
                }

                if (productDataList.Count == 0)
                    rowErrors.Add(new ImportPolicyExcelErrorDto { RowNumber = rowNumber, Field = "Sản phẩm", Message = L["Policy:ImportPolicy:NoProductData"] });

                // 5b. Với từng sản phẩm: resolve product, parse coverages; gom vào productDataForRow
                var productDataForRow = new List<(ProProduct Product, List<(string CoverageCode, decimal AmountLiability, decimal Deductible, int Quantity)> ParsedCoverages, Dictionary<string, ProProductCoverage> CoverageLookup)>();
                foreach (var (productCode, coverageCellValue) in productDataList)
                {
                    var productKey = productCode.Trim().ToUpperInvariant();
                    if (!productByCode.TryGetValue(productKey, out var product))
                    {
                        rowErrors.Add(new ImportPolicyExcelErrorDto { RowNumber = rowNumber, Field = "Sản phẩm", Message = L["Policy:ImportPolicy:ProductNotFound"], Value = productCode });
                        continue;
                    }

                    var coverageLines = coverageCellValue.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    var parsedCoverages = new List<(string CoverageCode, decimal AmountLiability, decimal Deductible, int Quantity)>();
                    var coverageLookup = productCoverageMap.GetValueOrDefault(product.Id) ?? new Dictionary<string, ProProductCoverage>(StringComparer.OrdinalIgnoreCase);

                    foreach (var line in coverageLines)
                    {
                        var parts = line.Trim().Split(':');
                        if (parts.Length < 2)
                        {
                            rowErrors.Add(new ImportPolicyExcelErrorDto { RowNumber = rowNumber, Field = productCode, Message = L["Policy:ImportPolicy:InvalidCoverageFormat"], Value = line.Trim() });
                            continue;
                        }

                        var covCode = parts[0].Trim();
                        decimal.TryParse(parts.Length > 1 ? parts[1] : "0", NumberStyles.Any, CultureInfo.InvariantCulture, out var amountLiability);
                        decimal.TryParse(parts.Length > 2 && !string.IsNullOrWhiteSpace(parts[2]) ? parts[2] : "0", NumberStyles.Any, CultureInfo.InvariantCulture, out var deductible);
                        int.TryParse(parts.Length > 3 ? parts[3] : "1", NumberStyles.Any, CultureInfo.InvariantCulture, out var qty);
                        if (qty <= 0) qty = 1;

                        if (!coverageLookup.ContainsKey(covCode.ToUpperInvariant()))
                        {
                            rowErrors.Add(new ImportPolicyExcelErrorDto { RowNumber = rowNumber, Field = productCode, Message = L["Policy:ImportPolicy:CoverageNotFound"], Value = covCode });
                            continue;
                        }

                        parsedCoverages.Add((covCode, amountLiability, deductible, qty));
                    }

                    if (parsedCoverages.Count > 0)
                        productDataForRow.Add((product, parsedCoverages, coverageLookup));
                }

                // 5c. Read fixed columns
                string GetCell(string header) => colMap.TryGetValue(header, out var c) ? GetExcelCellString(row, c).Trim() : "";

                var partnerCode = GetCell("Đối tác BH gốc");
                var excelContractCode = colMap.TryGetValue("Số HĐ", out var soHdCol)
                    ? GetExcelCellStringForContractCode(row, soHdCol)
                    : string.Empty;
                excelContractCode = NormalizeImportedContractCode(excelContractCode);

                iOne.PolicyContracts.PolicyContract? contractFromExcelRow = null;
                if (!string.IsNullOrWhiteSpace(excelContractCode))
                {
                    var excelCodeLower = excelContractCode.ToLowerInvariant();
                    var ccQuery = await PolicyContractRepository.GetQueryableAsync();
                    // So khớp đúng mã Code (không dùng ILIKE pattern) để tránh %/_ và escape.
                    contractFromExcelRow = await AsyncExecuter.FirstOrDefaultAsync(
                        ccQuery.Where(c =>
                            !c.IsDeleted &&
                            c.Code != null &&
                            c.Code.Trim().ToLower() == excelCodeLower));
                    if (contractFromExcelRow == null)
                    {
                        rowErrors.Add(new ImportPolicyExcelErrorDto
                        {
                            RowNumber = rowNumber,
                            Field = "Số HĐ",
                            Message = L["Policy:ImportPolicy:ContractCodeNotFound"].Value,
                            Value = excelContractCode
                        });
                    }
                    else if (contract != null && contractFromExcelRow.Id != contract.Id)
                    {
                        rowErrors.Add(new ImportPolicyExcelErrorDto
                        {
                            RowNumber = rowNumber,
                            Field = "Số HĐ",
                            Message = L["Policy:ImportPolicy:ContractCodeNotMatchImportContext"].Value,
                            Value = excelContractCode
                        });
                    }
                }

                var lobCode = GetCell("Nghiệp vụ BH");
                var currencyCode = GetCell("Tiền tệ");
                if ((isTndsbbImportLayout || isVcxImportLayout) && string.IsNullOrWhiteSpace(currencyCode))
                    currencyCode = "VND";
                var channelCode = GetCell("Kênh khai thác");
                var sellerCode = GetCell("Người khai thác");
                var implementerCode = GetCell("Người cấp đơn");

                // Khi tạo HĐ mới từ import (không có contractId API): lấy loại HĐ từ cột "Loại hợp đồng" — trước đây cố định Individual nên HĐ nhóm/bao bị thành lẻ.
                var resolvedNewContractType = PolicyContractType.Individual;
                if (contract == null)
                {
                    var contractTypeCell = GetCell("Loại hợp đồng");
                    if (!TryMapPolicyContractTypeFromImport(contractTypeCell, out resolvedNewContractType))
                    {
                        rowErrors.Add(new ImportPolicyExcelErrorDto
                        {
                            RowNumber = rowNumber,
                            Field = "Loại hợp đồng",
                            Message = L["Policy:ImportPolicy:InvalidContractType"].Value,
                            Value = contractTypeCell
                        });
                    }
                }

                // Resolve IDs (validate value exists in master when provided)
                if (!ResolveId(L, partnerLookup, partnerCode, "Đối tác BH gốc", rowNumber, rowErrors, out var partnerId))
                {
                    // Ô "Đối tác BH gốc" trống: lấy insurer từ HĐ tra được theo cột "Số HĐ" trong file (ưu tiên), sau đó mới HĐ chọn trên UI (nếu có).
                    if (contractFromExcelRow?.InsurerId is { } insFromFile && insFromFile != Guid.Empty
                        && string.IsNullOrWhiteSpace(partnerCode))
                    {
                        partnerId = insFromFile;
                    }
                    else if (contract != null && contract.InsurerId.HasValue && contract.InsurerId.Value != Guid.Empty
                             && string.IsNullOrWhiteSpace(partnerCode))
                    {
                        partnerId = contract.InsurerId.Value;
                    }
                    else
                    {
                        partnerId = Guid.Empty;
                    }
                }
                if (!ResolveId(L, lobLookup, lobCode, "Nghiệp vụ BH", rowNumber, rowErrors, out var lobId)) lobId = contract?.LobId ?? Guid.Empty;
                if (!ResolveId(L, currencyLookup, currencyCode, "Tiền tệ", rowNumber, rowErrors, out var currencyId)) { /* will skip */ }
                // Kênh khai thác: nhập mã (res_channel.code), không dùng id — tra bảng ResChannel; sai mã → báo không tồn tại.
                var channelId = Guid.Empty;
                if (string.IsNullOrWhiteSpace(channelCode))
                {
                    // để trống → RequiredFieldMissing ở khối kiểm tra bắt buộc
                }
                else if (!channelLookup.TryGetValue(channelCode.Trim(), out channelId))
                {
                    rowErrors.Add(new ImportPolicyExcelErrorDto
                    {
                        RowNumber = rowNumber,
                        Field = "Kênh khai thác",
                        Message = L["Policy:ImportPolicy:ChannelCodeNotFound", channelCode.Trim()].Value,
                        Value = channelCode.Trim()
                    });
                }
                ResolveId(L, employeeLookup, sellerCode, "Người khai thác", rowNumber, rowErrors, out var sellerId);
                if (!ResolveId(L, employeeLookup, implementerCode, "Người cấp đơn", rowNumber, rowErrors, out var implementerId)) { /* will skip */ }

                // Province / Ward (Insured)
                ResolveId(L, provinceLookup, GetCell("Tỉnh/Thành"), "Tỉnh/Thành", rowNumber, rowErrors, out var insuredProvinceId);
                ResolveId(L, wardLookup, GetCell("Phường xã"), "Phường xã", rowNumber, rowErrors, out var insuredWardId);

                // Province / Ward (Payer)
                ResolveId(L, provinceLookup, GetCell("Tỉnh/Thành Người TT"), "Tỉnh/Thành Người TT", rowNumber, rowErrors, out var payerProvinceId);
                ResolveId(L, wardLookup, GetCell("Phường xã Người TT"), "Phường xã Người TT", rowNumber, rowErrors, out var payerWardId);

                // Province / Ward (Beneficiary)
                ResolveId(L, provinceLookup, GetCell("Tỉnh/Thành Người TH"), "Tỉnh/Thành Người TH", rowNumber, rowErrors, out var beneficiaryProvinceId);
                ResolveId(L, wardLookup, GetCell("Phường xã Người TH"), "Phường xã Người TH", rowNumber, rowErrors, out var beneficiaryWardId);

                // Province / Ward (RiskObject)
                ResolveId(L, provinceLookup, GetCell("Tỉnh/Thành Chủ xe"), "Tỉnh/Thành Chủ xe", rowNumber, rowErrors, out var riskProvinceId);
                ResolveId(L, wardLookup, GetCell("Phường xã Chủ xe"), "Phường xã Chủ xe", rowNumber, rowErrors, out var riskWardId);

                // Check critical errors (missing required IDs) — align with policy create/update required fields
                if (lobId == Guid.Empty) rowErrors.Add(new ImportPolicyExcelErrorDto { RowNumber = rowNumber, Field = "Nghiệp vụ BH", Message = L["Policy:ImportPolicy:RequiredFieldMissing"], Value = lobCode });
                if (currencyId == Guid.Empty) rowErrors.Add(new ImportPolicyExcelErrorDto { RowNumber = rowNumber, Field = "Tiền tệ", Message = L["Policy:ImportPolicy:RequiredFieldMissing"], Value = currencyCode });
                if (implementerId == Guid.Empty) rowErrors.Add(new ImportPolicyExcelErrorDto { RowNumber = rowNumber, Field = "Người cấp đơn", Message = L["Policy:ImportPolicy:RequiredFieldMissing"], Value = implementerCode });
                if (partnerId == Guid.Empty && string.IsNullOrWhiteSpace(partnerCode))
                    rowErrors.Add(new ImportPolicyExcelErrorDto { RowNumber = rowNumber, Field = "Đối tác BH gốc", Message = L["Policy:ImportPolicy:RequiredFieldMissing"], Value = partnerCode });
                // Đối tác BH gốc (file) phải trùng insurer của HĐ tra theo "Số HĐ" (file) — không dùng insurerId truyền từ UI.
                if (contractFromExcelRow != null
                    && contractFromExcelRow.InsurerId.HasValue && contractFromExcelRow.InsurerId.Value != Guid.Empty
                    && partnerId != Guid.Empty
                    && partnerId != contractFromExcelRow.InsurerId.Value)
                {
                    rowErrors.Add(new ImportPolicyExcelErrorDto
                    {
                        RowNumber = rowNumber,
                        Field = "Đối tác BH gốc",
                        Message = L["Policy:ImportPolicy:InsurerMismatchWithContractInFile"].Value,
                        Value = partnerCode
                    });
                }
                if (sellerId == Guid.Empty) rowErrors.Add(new ImportPolicyExcelErrorDto { RowNumber = rowNumber, Field = "Người khai thác", Message = L["Policy:ImportPolicy:RequiredFieldMissing"], Value = sellerCode });
                if (string.IsNullOrWhiteSpace(channelCode))
                    rowErrors.Add(new ImportPolicyExcelErrorDto { RowNumber = rowNumber, Field = "Kênh khai thác", Message = L["Policy:ImportPolicy:RequiredFieldMissing"], Value = channelCode });

                // Parse dates
                var effectDateStr = GetCell("Thời hạn bảo hiểm (từ)\n(giờ:phút ngày/tháng/năm)");
                var expireDateStr = GetCell("Thời hạn bảo hiểm (đến)\n(giờ:phút ngày/tháng/năm)");
                var effectParsed = TryParseFlexibleDate(effectDateStr, out var effectDate);
                if (!effectParsed)
                {
                    rowErrors.Add(new ImportPolicyExcelErrorDto { RowNumber = rowNumber, Field = "Thời hạn BH (từ)", Message = L["Policy:ImportPolicy:InvalidDate"], Value = effectDateStr });
                }
                var expireParsed = TryParseFlexibleDate(expireDateStr, out var expireDate);
                if (!expireParsed)
                {
                    rowErrors.Add(new ImportPolicyExcelErrorDto { RowNumber = rowNumber, Field = "Thời hạn BH (đến)", Message = L["Policy:ImportPolicy:InvalidDate"], Value = expireDateStr });
                }
                if (effectParsed && expireParsed && effectDate >= expireDate)
                {
                    rowErrors.Add(new ImportPolicyExcelErrorDto
                    {
                        RowNumber = rowNumber,
                        Field = "Thời hạn BH",
                        Message = L["PolicyContract:ExpireDateMustBeAfterEffectDate"].Value,
                        Value = $"{effectDateStr} / {expireDateStr}"
                    });
                }

                // Read other fields
                var insuredName = GetCell("Tên Khách hàng");
                var insuredPhone = GetCell("Số ĐT khách hàng");
                var insuredEmail = GetCell("Email khách hàng");
                var insuredOrgType = GetCell("Loại khách hàng");
                var insuredAddress = GetCell("Địa chỉ");
                var insuredIdNo = GetCell("CCCD");
                var insuredTin = GetCell("Mã số thuế");

                var payerName = GetCell("Tên Người thanh toán");
                var payerPhone = GetCell("Số ĐT Người thanh toán");
                var payerEmail = GetCell("Email Người thanh toán");
                var payerAddress = GetCell("Địa chỉ Người TT");
                var payerTin = GetCell("Mã số thuế Người TT");

                // TNDSBB / VCX: khách hàng & người thanh toán không bắt buộc; nếu để trống thì mặc định từ nhóm Chủ xe.
                if (isTndsbbImportLayout || isVcxImportLayout)
                {
                    if (string.IsNullOrWhiteSpace(insuredName)) insuredName = GetCell("Tên Chủ xe");
                    if (string.IsNullOrWhiteSpace(insuredPhone)) insuredPhone = GetCell("Số ĐT Chủ xe");
                    if (string.IsNullOrWhiteSpace(insuredEmail)) insuredEmail = GetCell("Email Chủ xe");
                    if (string.IsNullOrWhiteSpace(insuredAddress)) insuredAddress = GetCell("Địa chỉ Chủ xe");
                    if (string.IsNullOrWhiteSpace(insuredIdNo)) insuredIdNo = GetCell("CCCD Chủ xe");
                    if (insuredProvinceId == Guid.Empty) ResolveId(L, provinceLookup, GetCell("Tỉnh/Thành Chủ xe"), "Tỉnh/Thành Chủ xe", rowNumber, rowErrors, out insuredProvinceId);
                    if (insuredWardId == Guid.Empty) ResolveId(L, wardLookup, GetCell("Phường xã Chủ xe"), "Phường xã Chủ xe", rowNumber, rowErrors, out insuredWardId);

                    if (string.IsNullOrWhiteSpace(payerName)) payerName = GetCell("Tên Chủ xe");
                    if (string.IsNullOrWhiteSpace(payerPhone)) payerPhone = GetCell("Số ĐT Chủ xe");
                    if (string.IsNullOrWhiteSpace(payerEmail)) payerEmail = GetCell("Email Chủ xe");
                    if (string.IsNullOrWhiteSpace(payerAddress)) payerAddress = GetCell("Địa chỉ Chủ xe");
                    if (payerProvinceId == Guid.Empty) payerProvinceId = riskProvinceId;
                    if (payerWardId == Guid.Empty) payerWardId = riskWardId;
                }

                var insurerPolicyNo = GetCell("Số đơn BH gốc");
                var certificateNo = GetCell("Số GCN");
                var isBankLoan = isTndsbbImportLayout ? "N" : ParseYesNoFromExcel(GetCell("Có sử dụng khoản vay?"));
                var isReceiveInvoice = ParseYesNoFromExcel(GetCell("Nhận hóa đơn"));

                var beneficiaryName = GetCell("Tên Người Thụ hưởng");
                var beneficiaryPhone = GetCell("Số ĐT Người TH");
                var beneficiaryEmail = GetCell("Email Người TH");
                var beneficiaryOrgType = GetCell("Loại Người TH");
                var beneficiaryAddress = GetCell("Địa chỉ Người TH");
                var beneficiaryIdNo = GetCell("CCCD Người TH");
                var beneficiaryTin = GetCell("Mã số thuế Người TH");

                // Fallback: nếu không nhập thông tin Nhóm Người Thụ hưởng thì lấy từ Nhóm Chủ xe.
                // (Excel có thể chỉ điền "Tên Chủ xe / SĐT Chủ xe / Email Chủ xe / Địa chỉ Chủ xe" mà để trống các cột Người TH.)
                var isBeneficiaryEmpty =
                    string.IsNullOrWhiteSpace(beneficiaryName) &&
                    string.IsNullOrWhiteSpace(beneficiaryPhone) &&
                    string.IsNullOrWhiteSpace(beneficiaryEmail) &&
                    string.IsNullOrWhiteSpace(beneficiaryOrgType) &&
                    string.IsNullOrWhiteSpace(beneficiaryAddress) &&
                    string.IsNullOrWhiteSpace(beneficiaryIdNo) &&
                    string.IsNullOrWhiteSpace(beneficiaryTin) &&
                    beneficiaryProvinceId == Guid.Empty &&
                    beneficiaryWardId == Guid.Empty;

                if (isBeneficiaryEmpty)
                {
                    beneficiaryName = GetCell("Tên Chủ xe");
                    beneficiaryPhone = GetCell("Số ĐT Chủ xe");
                    beneficiaryEmail = GetCell("Email Chủ xe");
                    beneficiaryAddress = GetCell("Địa chỉ Chủ xe");
                    beneficiaryProvinceId = riskProvinceId;
                    beneficiaryWardId = riskWardId;
                    if (string.IsNullOrWhiteSpace(beneficiaryIdNo))
                        beneficiaryIdNo = GetCell("CCCD Chủ xe");
                }

                // RiskObject / Motor
                var carUsage = GetCell("Mục đích kinh doanh");
                var carBrandCode = GetCell("Hãng xe");
                var carModelCode = GetCell("Hiệu xe");
                var carSeatNumberStr = GetCell("Số chỗ ngồi");
                var carPayloadStr = GetCell("Trọng tải (tấn)");
                var carNewStr = ParseYesNoFromExcel(GetCell("Xe mới")); // Có/Không hoặc Y/N trong Excel → "Y"/"N"
                var carValueStr = GetCell("Giá trị xe\n(VNĐ)");
                var carPlate = GetCell("Biển số xe");
                var carVin = GetCell("Số khung");
                var carEngineNumber = GetCell("Số máy");
                var carProductionYearStr = GetCell("Năm sản xuất/Đăng ký lần đầu");
                var carLineCode = GetCell("Dòng xe");
                var carGroupCode = GetCell("Nhóm xe");
                var carTypeCode = GetCell("Loại xe");

                decimal.TryParse(carSeatNumberStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var carSeatNumber);
                decimal.TryParse(carPayloadStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var carPayload);
                decimal.TryParse(carValueStr?.Replace(",", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out var carValue);
                DateTime? carProductionYear = null;
                if (TryParseFlexibleDate(carProductionYearStr, out var cpDate)) carProductionYear = cpDate;

                // Required fields (align with policy create/update UI)
                var ownerNameCell = GetCell("Tên Chủ xe");
                var ownerPhoneCell = GetCell("Số ĐT Chủ xe");
                if (string.IsNullOrWhiteSpace(ownerNameCell))
                    rowErrors.Add(new ImportPolicyExcelErrorDto { RowNumber = rowNumber, Field = "Tên Chủ xe", Message = L["Policy:ImportPolicy:RequiredFieldMissing"], Value = ownerNameCell });
                if (string.IsNullOrWhiteSpace(ownerPhoneCell))
                    rowErrors.Add(new ImportPolicyExcelErrorDto { RowNumber = rowNumber, Field = "Số ĐT Chủ xe", Message = L["Policy:ImportPolicy:RequiredFieldMissing"], Value = ownerPhoneCell });

                if (string.IsNullOrWhiteSpace(insuredName)) rowErrors.Add(new ImportPolicyExcelErrorDto { RowNumber = rowNumber, Field = "Tên Khách hàng", Message = L["Policy:ImportPolicy:RequiredFieldMissing"], Value = insuredName });
                if (string.IsNullOrWhiteSpace(insuredPhone)) rowErrors.Add(new ImportPolicyExcelErrorDto { RowNumber = rowNumber, Field = "Số ĐT khách hàng", Message = L["Policy:ImportPolicy:RequiredFieldMissing"], Value = insuredPhone });
                if (string.IsNullOrWhiteSpace(insuredAddress)) rowErrors.Add(new ImportPolicyExcelErrorDto { RowNumber = rowNumber, Field = "Địa chỉ", Message = L["Policy:ImportPolicy:RequiredFieldMissing"], Value = insuredAddress });
                if (string.IsNullOrWhiteSpace(carUsage))
                {
                    rowErrors.Add(new ImportPolicyExcelErrorDto { RowNumber = rowNumber, Field = "Mục đích kinh doanh", Message = L["Policy:ImportPolicy:RequiredFieldMissing"], Value = carUsage });
                }
                else
                {
                    var carUsageNorm = carUsage.Trim();
                    if (string.Equals(carUsageNorm, "KDVT", StringComparison.OrdinalIgnoreCase))
                    {
                        carUsage = "KDVT";
                    }
                    else if (string.Equals(carUsageNorm, "KKDVT", StringComparison.OrdinalIgnoreCase))
                    {
                        carUsage = "KKDVT";
                    }
                    else
                    {
                        rowErrors.Add(new ImportPolicyExcelErrorDto
                        {
                            RowNumber = rowNumber,
                            Field = "Mục đích kinh doanh",
                            Message = L["Policy:ImportPolicy:InvalidCarUsage"].Value,
                            Value = carUsage
                        });
                    }
                }
                if (string.IsNullOrWhiteSpace(carBrandCode)) rowErrors.Add(new ImportPolicyExcelErrorDto { RowNumber = rowNumber, Field = "Hãng xe", Message = L["Policy:ImportPolicy:RequiredFieldMissing"], Value = carBrandCode });
                if (string.IsNullOrWhiteSpace(carModelCode)) rowErrors.Add(new ImportPolicyExcelErrorDto { RowNumber = rowNumber, Field = "Hiệu xe", Message = L["Policy:ImportPolicy:RequiredFieldMissing"], Value = carModelCode });
                if (string.IsNullOrWhiteSpace(carSeatNumberStr) || carSeatNumber < 0)
                    rowErrors.Add(new ImportPolicyExcelErrorDto { RowNumber = rowNumber, Field = "Số chỗ ngồi", Message = L["Policy:ImportPolicy:RequiredFieldMissing"], Value = carSeatNumberStr });
                if (string.IsNullOrWhiteSpace(carPayloadStr) || carPayload < 0)
                    rowErrors.Add(new ImportPolicyExcelErrorDto { RowNumber = rowNumber, Field = "Trọng tải (tấn)", Message = L["Policy:ImportPolicy:RequiredFieldMissing"], Value = carPayloadStr });
                if (!isTndsbbImportLayout && (string.IsNullOrWhiteSpace(carValueStr) || carValue <= 0))
                    rowErrors.Add(new ImportPolicyExcelErrorDto { RowNumber = rowNumber, Field = "Giá trị xe (VNĐ)", Message = L["Policy:ImportPolicy:RequiredFieldMissing"], Value = carValueStr });
                if (string.IsNullOrWhiteSpace(carLineCode)) rowErrors.Add(new ImportPolicyExcelErrorDto { RowNumber = rowNumber, Field = "Dòng xe", Message = L["Policy:ImportPolicy:RequiredFieldMissing"], Value = carLineCode });
                if (string.IsNullOrWhiteSpace(carGroupCode)) rowErrors.Add(new ImportPolicyExcelErrorDto { RowNumber = rowNumber, Field = "Nhóm xe", Message = L["Policy:ImportPolicy:RequiredFieldMissing"], Value = carGroupCode });
                if (string.IsNullOrWhiteSpace(carTypeCode)) rowErrors.Add(new ImportPolicyExcelErrorDto { RowNumber = rowNumber, Field = "Loại xe", Message = L["Policy:ImportPolicy:RequiredFieldMissing"], Value = carTypeCode });

                // Khi import không gắn hợp đồng: nhận diện khách hàng theo CCCD (IdNo), bắt buộc có CCCD.
                if (contract == null && string.IsNullOrWhiteSpace((insuredIdNo ?? string.Empty).Trim()))
                    rowErrors.Add(new ImportPolicyExcelErrorDto { RowNumber = rowNumber, Field = "CCCD", Message = L["Policy:ImportPolicy:RequiredFieldMissing"], Value = insuredIdNo });

                // Validate selectbox values exist in master (avoid e.g. "AUDI1" when only "AUDI" exists)
                if (!string.IsNullOrWhiteSpace(carBrandCode) && !carBrandLookup.ContainsKey(carBrandCode.Trim()))
                    rowErrors.Add(new ImportPolicyExcelErrorDto { RowNumber = rowNumber, Field = "Hãng xe", Message = L["Policy:ImportPolicy:ValueNotFoundInMaster", carBrandCode.Trim(), "Hãng xe"].Value, Value = carBrandCode.Trim() });
                if (!string.IsNullOrWhiteSpace(carModelCode) && !carCategoryLookup.ContainsKey(carModelCode.Trim()))
                    rowErrors.Add(new ImportPolicyExcelErrorDto { RowNumber = rowNumber, Field = "Hiệu xe", Message = L["Policy:ImportPolicy:ValueNotFoundInMaster", carModelCode.Trim(), "Hiệu xe"].Value, Value = carModelCode.Trim() });
                if (!string.IsNullOrWhiteSpace(carLineCode) && !carLineLookup.ContainsKey(carLineCode.Trim()))
                    rowErrors.Add(new ImportPolicyExcelErrorDto { RowNumber = rowNumber, Field = "Dòng xe", Message = L["Policy:ImportPolicy:ValueNotFoundInMaster", carLineCode.Trim(), "Dòng xe"].Value, Value = carLineCode.Trim() });
                if (!string.IsNullOrWhiteSpace(carGroupCode) && !carGroupLookup.ContainsKey(carGroupCode.Trim()))
                    rowErrors.Add(new ImportPolicyExcelErrorDto { RowNumber = rowNumber, Field = "Nhóm xe", Message = L["Policy:ImportPolicy:ValueNotFoundInMaster", carGroupCode.Trim(), "Nhóm xe"].Value, Value = carGroupCode.Trim() });
                if (!string.IsNullOrWhiteSpace(carTypeCode) && !carTypeLookup.ContainsKey(carTypeCode.Trim()))
                    rowErrors.Add(new ImportPolicyExcelErrorDto { RowNumber = rowNumber, Field = "Loại xe", Message = L["Policy:ImportPolicy:ValueNotFoundInMaster", carTypeCode.Trim(), "Loại xe"].Value, Value = carTypeCode.Trim() });

                // Khi không có contractId: tìm khách hàng theo CCCD (IdNo) để tạo HĐ mới (CreateAsync sẽ tự sinh HĐ)
                Guid? customerIdForRow = contract?.CustomerId;
                if (contract == null)
                {
                    string? insuredWardNameForFullAddress = null;
                    string? insuredProvinceNameForFullAddress = null;
                    if (insuredWardId != Guid.Empty && wardIdToName.TryGetValue(insuredWardId, out var wn))
                        insuredWardNameForFullAddress = wn;
                    if (insuredProvinceId != Guid.Empty && provinceIdToName.TryGetValue(insuredProvinceId, out var pn))
                        insuredProvinceNameForFullAddress = pn;

                    var normalizedIdNo = (insuredIdNo ?? string.Empty).Trim();
                    var normalizedPhone = (insuredPhone ?? string.Empty).Trim();
                    if (!string.IsNullOrWhiteSpace(normalizedIdNo) && createdCustomerIdByIdNo.TryGetValue(normalizedIdNo, out var createdCustomerId))
                    {
                        customerIdForRow = createdCustomerId;
                    }
                    else if (!string.IsNullOrWhiteSpace(normalizedIdNo))
                    {
                        var customerQuery = await ResCustomerRepository.GetQueryableAsync();
                        var idNoLikePattern = EscapeForLike(normalizedIdNo);
                        var customer = await AsyncExecuter.FirstOrDefaultAsync(
                            customerQuery.Where(c =>
                                !c.IsDeleted
                                && c.IdNo != null
                                && EF.Functions.ILike(c.IdNo.Trim(), idNoLikePattern)));

                        if (customer != null)
                        {
                            // Có customer theo CCCD: cập nhật thông tin từ file (gồm SĐT nếu có) rồi gắn lại cho đơn.
                            try
                            {
                                if (!string.IsNullOrWhiteSpace(insuredName))
                                    customer.UpdateName(insuredName.Trim());

                                if (insuredProvinceId != Guid.Empty)
                                    customer.UpdateProvinceId(insuredProvinceId);

                                if (insuredWardId != Guid.Empty)
                                    customer.UpdateWardId(insuredWardId);

                                if (!string.IsNullOrWhiteSpace(insuredAddress))
                                    customer.UpdateAddress(insuredAddress.Trim());

                                // Giống UI / ResCustomerAppService: địa chỉ, phường/xã, tỉnh/thành — nối ", ".
                                customer.UpdateFullAddress(insuredWardNameForFullAddress, insuredProvinceNameForFullAddress);

                                if (!string.IsNullOrWhiteSpace(normalizedPhone))
                                    customer.UpdatePhone(normalizedPhone);

                                customer.UpdateEmail(NullIfEmpty(insuredEmail));
                                customer.UpdateTin(NullIfEmpty(insuredTin));
                                customer.UpdateIdNo(normalizedIdNo);
                                customer.UpdateStatus(ResCustomerStatus.Active);

                                await ResCustomerRepository.UpdateAsync(customer);
                            }
                            catch (Exception ex)
                            {
                                rowErrors.Add(new ImportPolicyExcelErrorDto
                                {
                                    RowNumber = rowNumber,
                                    Field = "Khách hàng",
                                    Message = GetDisplayMessageForImportError(ex),
                                    Value = normalizedIdNo
                                });
                            }
                            customerIdForRow = customer.Id;
                        }
                        else
                        {
                            // Không tìm thấy theo CCCD -> tạo mới customer và gắn ngược vào đơn.
                            // Customer domain requires province/ward, name, address, phone.
                            if (string.IsNullOrWhiteSpace(insuredName))
                                rowErrors.Add(new ImportPolicyExcelErrorDto { RowNumber = rowNumber, Field = "Tên Khách hàng", Message = L["Policy:ImportPolicy:RequiredFieldMissing"], Value = insuredName });
                            if (string.IsNullOrWhiteSpace(normalizedPhone))
                                rowErrors.Add(new ImportPolicyExcelErrorDto { RowNumber = rowNumber, Field = "Số ĐT khách hàng", Message = L["Policy:ImportPolicy:RequiredFieldMissing"], Value = insuredPhone });
                            if (string.IsNullOrWhiteSpace(insuredAddress))
                                rowErrors.Add(new ImportPolicyExcelErrorDto { RowNumber = rowNumber, Field = "Địa chỉ", Message = L["Policy:ImportPolicy:RequiredFieldMissing"], Value = insuredAddress });
                            if (insuredProvinceId == Guid.Empty)
                                rowErrors.Add(new ImportPolicyExcelErrorDto { RowNumber = rowNumber, Field = "Tỉnh/Thành", Message = L["Policy:ImportPolicy:RequiredFieldMissing"], Value = GetCell("Tỉnh/Thành") });
                            if (insuredWardId == Guid.Empty)
                                rowErrors.Add(new ImportPolicyExcelErrorDto { RowNumber = rowNumber, Field = "Phường xã", Message = L["Policy:ImportPolicy:RequiredFieldMissing"], Value = GetCell("Phường xã") });

                            if (rowErrors.Count == 0)
                            {
                                try
                                {
                                    var customerCode = await GenerateNewCustomerCodeForImportAsync();
                                    var newCustomer = new ResCustomer(
                                        GuidGenerator.Create(),
                                        code: customerCode,
                                        name: insuredName.Trim(),
                                        provinceId: insuredProvinceId,
                                        wardId: insuredWardId,
                                        address: insuredAddress.Trim(),
                                        phone: normalizedPhone,
                                        status: ResCustomerStatus.Active,
                                        email: NullIfEmpty(insuredEmail),
                                        tin: NullIfEmpty(insuredTin),
                                        idNo: normalizedIdNo,
                                        saleId: implementerId != Guid.Empty ? implementerId : null
                                    );

                                    newCustomer.UpdateFullAddress(insuredWardNameForFullAddress, insuredProvinceNameForFullAddress);

                                    await ResCustomerRepository.InsertAsync(newCustomer);
                                    customerIdForRow = newCustomer.Id;
                                    createdCustomerIdByIdNo[normalizedIdNo] = newCustomer.Id;
                                }
                                catch (Exception ex)
                                {
                                    rowErrors.Add(new ImportPolicyExcelErrorDto
                                    {
                                        RowNumber = rowNumber,
                                        Field = "CCCD",
                                        Message = GetDisplayMessageForImportError(ex),
                                        Value = normalizedIdNo
                                    });
                                }
                            }
                        }
                    }
                }

                // Gom hết lỗi của dòng: chỉ skip tạo đơn khi có lỗi; trả toàn bộ lỗi một lần
                if (rowErrors.Count > 0)
                {
                    rowErrorDetails[rowNumber] = rowErrors
                        .Select(e => string.IsNullOrWhiteSpace(e.Field) ? e.Message : $"{e.Field}: {e.Message}")
                        .ToList();
                    errors.AddRange(rowErrors);
                    result.ErrorCount++;
                    rowNumber++;
                    continue;
                }

                // Discount & Premium from trailing columns (Giảm phí / Phí BH — cùng cách tính số với UI qua CreateAsync + PolicyAmount)
                decimal? discountFromExcel = null;
                var premiumFromExcel = 0m;
                if (discountCol > 0)
                {
                    var rawDiscount = GetExcelCellString(row, discountCol).Trim();
                    if (!string.IsNullOrWhiteSpace(rawDiscount) && TryParseDecimalFromExcelImport(rawDiscount, out var d))
                        discountFromExcel = d;
                }
                if (premiumCol > 0)
                {
                    var rawPremium = GetExcelCellString(row, premiumCol).Trim();
                    if (!string.IsNullOrWhiteSpace(rawPremium))
                        TryParseDecimalFromExcelImport(rawPremium, out premiumFromExcel);
                }

                // 5d. Build RiskObject for attribute extraction
                var riskMotorInput = new CreatePolicyRiskMotorInputDto
                {
                    RiskObjectValue = carValue,
                    CarUsage = NullIfEmpty(carUsage),
                    CarBrandCode = NullIfEmpty(carBrandCode),
                    CarCategoryCode = NullIfEmpty(carModelCode),
                    CarSeatNumber = carSeatNumber > 0 ? carSeatNumber : null,
                    CarPayloadCapacity = carPayload > 0 ? carPayload : null,
                    CarNew = NullIfEmpty(carNewStr),
                    CarPlate = NullIfEmpty(carPlate),
                    CarVin = NullIfEmpty(carVin),
                    CarEngineNumber = NullIfEmpty(carEngineNumber),
                    CarProductionYear = carProductionYear,
                    CarLineCode = NullIfEmpty(carLineCode),
                    CarGroupCode = NullIfEmpty(carGroupCode),
                    CarTypeCode = NullIfEmpty(carTypeCode),
                };

                // 5e–5g. Với từng sản phẩm: extract attributes, tính phí (hoặc lấy từ Excel), build coverage inputs và product input
                decimal totalPremium = 0;
                decimal totalVat = 0;
                decimal totalPremiumWithVat = 0;
                var productInputs = new List<CreatePolicyProductInputDto>();
                var productIndex = 0;

                foreach (var (product, parsedCoverages, coverageLookup) in productDataForRow)
                {
                    var productEntityQuery = await ProProductRepository.GetQueryableAsync();
                    var productWithAttrs = await AsyncExecuter.FirstOrDefaultAsync(
                        productEntityQuery
                            .Include(p => p.ProductAttributes)
                                .ThenInclude(pa => pa.Attribute)
                            .Where(p => p.Id == product.Id));

                    var attrDefs = productWithAttrs?.ProductAttributes?
                        .Where(pa => pa.Attribute != null)
                        .Select(pa => new ExtractPolicyAttributeDefinitionInputDto
                        {
                            Code = pa.Attribute!.Code,
                            Name = pa.Attribute.Name,
                            Status = pa.Attribute.Status.ToString(),
                            Spec = pa.Attribute.Spec.ToString(),
                            DataPath = pa.Attribute.DataPath,
                            DataType = pa.Attribute.DataType.ToString(),
                            ComputeScript = pa.Attribute.ComputeScript,
                            ClearDataScript = pa.Attribute.ClearDataScript,
                        })
                        .ToList() ?? new List<ExtractPolicyAttributeDefinitionInputDto>();

                    var extractInput = new ExtractPolicyAttributeParametersInputDto
                    {
                        AmountLiability = parsedCoverages.FirstOrDefault().AmountLiability,
                        RiskObject = new ExtractPolicyRiskObjectForAttributeInputDto { RiskObjectMotor = riskMotorInput },
                        Product = new ExtractPolicyProductForAttributeInputDto { ProductId = product.Id.ToString(), Attributes = attrDefs }
                    };

                    var extractedList = await ExtractAttributeParametersAsync(extractInput);
                    var extractedAttrs = extractedList.FirstOrDefault() ?? new Dictionary<string, string>();

                    var coverageInputs = new List<CreatePolicyCoverageInputDto>();
                    decimal productPremium = 0, productVat = 0, productPremiumWithVat = 0;

                    if (premiumFromExcel <= 0)
                    {
                        var calcAttributes = new Dictionary<string, object>();
                        foreach (var kvp in extractedAttrs)
                        {
                            if (decimal.TryParse(kvp.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var numVal))
                                calcAttributes[kvp.Key] = numVal;
                            else
                                calcAttributes[kvp.Key] = kvp.Value;
                        }

                        var calcCoverages = new List<ProductCoveragePremiumRequestDto>();
                        foreach (var (covCode, amountLiability, deductible, qty) in parsedCoverages)
                        {
                            var pcov = coverageLookup[covCode.ToUpperInvariant()];
                            calcCoverages.Add(new ProductCoveragePremiumRequestDto
                            {
                                ProductCoverageId = pcov.Id,
                                CoverageId = pcov.CoverageId,
                                AmountLiability = amountLiability,
                                Quantity = qty
                            });
                        }

                        var calcInput = new CalculatePremiumRequestDto
                        {
                            EffectiveDate = effectDate,
                            ExpireDate = expireDate,
                            ProductId = product.Id,
                            Attributes = calcAttributes,
                            ProductCoverages = calcCoverages
                        };

                        var calcResult = await ProductPriceAppService.CalculatePremiumAsync(product.Code, calcInput);

                        foreach (var calcCov in calcResult.ProductCoverages)
                        {
                            var pcov = allProductCoverages.First(pc => pc.Id == calcCov.ProductCoverageId);
                            var covPremium = calcCov.Premium;
                            var covVat = calcCov.Vat ?? 0;
                            var covPremiumTotal = calcCov.PremiumVat ?? (covPremium + covVat);

                            var parsed = parsedCoverages.FirstOrDefault(c =>
                                coverageLookup.TryGetValue(c.CoverageCode.ToUpperInvariant(), out var pc) && pc.Id == calcCov.ProductCoverageId);

                            coverageInputs.Add(new CreatePolicyCoverageInputDto
                            {
                                CoverageId = calcCov.CoverageId,
                                UomId = pcov.UomId ?? Guid.Empty,
                                TaxId = pcov.TaxId,
                                AmountLiability = parsed.AmountLiability,
                                Quantity = parsed.Quantity,
                                PremiumRate = calcCov.Rate?.BaseRate ?? 0,
                                BaseRate = calcCov.Rate?.BaseRate,
                                FlatRate = calcCov.Rate?.FlatRate,
                                Premium = covPremium,
                                Vat = covVat,
                                PremiumTotal = covPremiumTotal,
                            });

                            productPremium += covPremium;
                            productVat += covVat;
                            productPremiumWithVat += covPremiumTotal;
                        }
                    }
                    else
                    {
                        foreach (var (covCode, amountLiability, deductible, qty) in parsedCoverages)
                        {
                            var pcov = coverageLookup[covCode.ToUpperInvariant()];
                            coverageInputs.Add(new CreatePolicyCoverageInputDto
                            {
                                CoverageId = pcov.CoverageId,
                                UomId = pcov.UomId ?? Guid.Empty,
                                TaxId = pcov.TaxId,
                                AmountLiability = amountLiability,
                                Quantity = qty,
                                PremiumRate = 0,
                                Premium = 0,
                                Vat = 0,
                                PremiumTotal = 0,
                            });
                        }
                        if (productIndex == 0)
                        {
                            productPremiumWithVat = premiumFromExcel;
                            productPremium = premiumFromExcel;
                            productVat = 0;
                        }
                    }

                    totalPremium += productPremium;
                    totalVat += productVat;
                    totalPremiumWithVat += productPremiumWithVat;

                    productInputs.Add(new CreatePolicyProductInputDto
                    {
                        ProductId = product.Id,
                        InsurerProductCode = product.InsurerProductCode,
                        PremiumTotal = productPremiumWithVat,
                        Premium = productPremium,
                        Vat = productVat,
                        Discount = (discountFromExcel.HasValue && productIndex == 0) ? discountFromExcel : null,
                        Coverages = coverageInputs,
                    });
                    productIndex++;
                }

                if (premiumFromExcel > 0)
                {
                    totalPremiumWithVat = premiumFromExcel;
                    totalPremium = premiumFromExcel;
                    totalVat = 0;
                    if (productInputs.Count > 0)
                    {
                        var first = productInputs[0];
                        first.PremiumTotal = premiumFromExcel;
                        first.Premium = premiumFromExcel;
                        first.Vat = 0;
                    }
                }

                // 5h. Build CreatePolicyDto (có contractId thì gắn HĐ có sẵn; không thì truyền Contract để CreateAsync tự sinh HĐ)
                var createInput = new CreatePolicyDto
                {
                    ContractId = contract != null ? contractId : null,
                    Contract = contract == null ? new CreatePolicyContractInputDto
                    {
                        Type = resolvedNewContractType,
                        CustomerId = customerIdForRow!.Value,
                        InsurerId = partnerId,
                        LobId = lobId,
                        Code = null,
                        Name = NullIfEmpty(insuredName) ?? "Import",
                        EffectDate = effectDate,
                        ExpireDate = expireDate,
                        Quantity = 1,
                        CurrentQuantity = 1,
                        EmployeeId = implementerId,
                        IsReciveInvoice = isReceiveInvoice,
                        PayerName = NullIfEmpty(payerName),
                        PayerPhone = NullIfEmpty(payerPhone),
                        PayerEmail = NullIfEmpty(payerEmail),
                        PayerAddress = NullIfEmpty(payerAddress),
                        PayerFullAddress = NullIfEmpty(payerAddress),
                        PayerTaxCode = NullIfEmpty(payerTin),
                        PayerProvinceId = payerProvinceId != Guid.Empty ? payerProvinceId : null,
                        PayerWardId = payerWardId != Guid.Empty ? payerWardId : null,
                    } : null,
                    LobId = lobId,
                    SellType = PolicySellType.Direct,
                    PolicyTypeId = newPolicyTypeId,
                    PartnerId = partnerId != Guid.Empty ? partnerId : (contract?.InsurerId ?? Guid.Empty),
                    SellerId = sellerId != Guid.Empty ? sellerId : implementerId,
                    ImplementerId = implementerId,
                    CurrencyId = currencyId,
                    ExchangeRate = 1m,
                    Status = PolicyStatus.Draft,
                    OrgEffectDate = effectDate,
                    OrgExpireDate = expireDate,
                    IsRenewal = "N",
                    IsGift = "N",
                    IsBankLoan = isBankLoan,
                    InsurerPolicyNo = NullIfEmpty(insurerPolicyNo),
                    CertificateNo = NullIfEmpty(certificateNo),
                    ChannelId = channelId != Guid.Empty ? channelId : null,
                    LotImportCode = importLotCode,
                    Discount = discountFromExcel,
                    PremiumTotal = totalPremiumWithVat,
                    Premium = totalPremium,
                    Vat = totalVat,

                    InsuredName = NullIfEmpty(insuredName),
                    InsuredPhone = NullIfEmpty(insuredPhone),
                    InsuredEmail = NullIfEmpty(insuredEmail),
                    InsuredOrgType = NullIfEmpty(insuredOrgType),
                    InsuredAddress = NullIfEmpty(insuredAddress),
                    InsuredIdNo = NullIfEmpty(insuredIdNo),
                    InsuredTin = NullIfEmpty(insuredTin),
                    InsuredProvinceId = insuredProvinceId != Guid.Empty ? insuredProvinceId : null,
                    InsuredWardId = insuredWardId != Guid.Empty ? insuredWardId : null,

                    BeneficiaryName = NullIfEmpty(beneficiaryName),
                    BeneficiaryPhone = NullIfEmpty(beneficiaryPhone),
                    BeneficiaryEmail = NullIfEmpty(beneficiaryEmail),
                    BeneficiaryOrgType = NullIfEmpty(beneficiaryOrgType),
                    BeneficiaryAddress = NullIfEmpty(beneficiaryAddress),
                    BeneficiaryIdNo = NullIfEmpty(beneficiaryIdNo),
                    BeneficiaryTin = NullIfEmpty(beneficiaryTin),
                    BeneficiaryProvinceId = beneficiaryProvinceId != Guid.Empty ? beneficiaryProvinceId : null,
                    BeneficiaryWardId = beneficiaryWardId != Guid.Empty ? beneficiaryWardId : null,

                    LastVersionId = Guid.Empty,
                    Version = new CreatePolicyVersionInputDto
                    {
                        Version = 0,
                        EffectDate = effectDate,
                        ExpireDate = expireDate,
                        OrgEffectDate = effectDate,
                        OrgExpireDate = expireDate,
                        PremiumTotal = totalPremiumWithVat,
                        Premium = totalPremium,
                        Vat = totalVat,
                        Discount = discountFromExcel,
                    },

                    Products = productInputs,

                    RiskObject = new CreatePolicyRiskObjectInputDto
                    {
                        ObjectTypeId = carObjectTypeId,
                        RepName = NullIfEmpty(GetCell("Tên Chủ xe")),
                        RepPhone = NullIfEmpty(GetCell("Số ĐT Chủ xe")),
                        RepEmail = NullIfEmpty(GetCell("Email Chủ xe")),
                        RepProvinceId = riskProvinceId != Guid.Empty ? riskProvinceId : null,
                        RepWardId = riskWardId != Guid.Empty ? riskWardId : null,
                        RepAddress = NullIfEmpty(GetCell("Địa chỉ Chủ xe")),
                        RiskObjectMotor = riskMotorInput,
                    },

                    Amount = new CreatePolicyAmountInputDto
                    {
                        PremiumTotal = totalPremiumWithVat,
                        Premium = totalPremium,
                        Vat = totalVat,
                        Discount = discountFromExcel,
                    },
                };

                await CreateAsync(createInput);
                result.SuccessCount++;
                result.LotImportCodes.Add(new ImportPolicyLotImportCodeDto
                {
                    RowNumber = rowNumber,
                    LotImportCode = importLotCode
                });
            }
            catch (Exception ex)
            {
                result.ErrorCount++;
                var displayMessage = GetDisplayMessageForImportError(ex);
                rowErrorDetails[rowNumber] = new List<string> { displayMessage };
                errors.Add(new ImportPolicyExcelErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "",
                    Message = displayMessage,
                });
            }

            rowNumber++;
        }

        result.Errors = errors;

        // Return the same import workbook with an extra error-detail column per row.
        if (rowErrorDetails.Count > 0 && ws != null)
        {
            var lastUsedCol = ws.LastColumnUsed()?.ColumnNumber() ?? 1;
            var errorCol = lastUsedCol + 1;
            ws.Cell(1, errorCol).Value = "Chi tiết lỗi";

            foreach (var (excelRow, messages) in rowErrorDetails)
            {
                if (messages.Count == 0) continue;
                ws.Cell(excelRow, errorCol).Value = string.Join(" | ", messages.Distinct());
            }

            ws.Column(errorCol).AdjustToContents();

            using var errorStream = new MemoryStream();
            workbook.SaveAs(errorStream);
            result.ErrorFileName = $"import_policy_errors_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            result.ErrorFileBase64 = Convert.ToBase64String(errorStream.ToArray());
        }

        // Cập nhật CurrentQuantity của hợp đồng = số đơn thuộc hợp đồng (khi import gắn với hợp đồng)
        if (contractId.HasValue && contractId.Value != Guid.Empty && contract != null)
        {
            var policyCount = await PolicyRepository.CountAsync(p => p.ContractId == contractId.Value && !p.IsDeleted);
            contract.UpdateCurrentQuantity(policyCount);
            await PolicyContractRepository.UpdateAsync(contract);
        }

        return result;
    }

    private static string GenerateRandomLotImportBaseCode()
    {
        // 10 bytes -> 20 hex chars (ALPHANUMERIC), safe under Policy:LotImportCodeMaxLength (50)
        var bytes = RandomNumberGenerator.GetBytes(10);
        var sb = new StringBuilder(bytes.Length * 2);
        foreach (var b in bytes)
        {
            sb.Append(b.ToString("X2"));
        }
        return sb.ToString();
    }

    /// <summary>
    /// Build a dictionary from code → id for a given entity repository.
    /// </summary>
    private async Task<Dictionary<string, Guid>> BuildLookupAsync<TEntity, TKey>(
        IRepository<TEntity, TKey> repository,
        Func<TEntity, string> codeSelector,
        Func<TEntity, TKey> idSelector)
        where TEntity : class, Volo.Abp.Domain.Entities.IEntity<TKey>
    {
        var query = await repository.GetQueryableAsync();
        var items = await AsyncExecuter.ToListAsync(query);
        return BuildLookupDictionary(items, codeSelector, idSelector);
    }

    /// <summary>
    /// Build a dictionary from code → id, optionally filtered (e.g. import master data: Active only).
    /// </summary>
    private async Task<Dictionary<string, Guid>> BuildLookupAsync<TEntity, TKey>(
        IRepository<TEntity, TKey> repository,
        Func<TEntity, string> codeSelector,
        Func<TEntity, TKey> idSelector,
        Expression<Func<TEntity, bool>> filter)
        where TEntity : class, Volo.Abp.Domain.Entities.IEntity<TKey>
    {
        var query = (await repository.GetQueryableAsync()).Where(filter);
        var items = await AsyncExecuter.ToListAsync(query);
        return BuildLookupDictionary(items, codeSelector, idSelector);
    }

    private static Dictionary<string, Guid> BuildLookupDictionary<TEntity, TKey>(
        IEnumerable<TEntity> items,
        Func<TEntity, string> codeSelector,
        Func<TEntity, TKey> idSelector)
    {
        var dict = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in items)
        {
            var code = codeSelector(item)?.Trim();
            if (!string.IsNullOrWhiteSpace(code) && idSelector(item) is Guid id && !dict.ContainsKey(code))
            {
                dict[code] = id;
            }
        }
        return dict;
    }

    /// <summary>
    /// Build a lookup from IResChannelRepository (custom interface).
    /// </summary>
    private async Task<Dictionary<string, Guid>> BuildLookupAsync(
        IResChannelRepository repository,
        Func<ResChannel, string> codeSelector,
        Func<ResChannel, Guid> idSelector)
    {
        var query = await repository.GetQueryableAsync();
        var items = await AsyncExecuter.ToListAsync(query);
        var dict = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in items)
        {
            var code = codeSelector(item)?.Trim();
            if (!string.IsNullOrWhiteSpace(code) && !dict.ContainsKey(code))
            {
                dict[code] = idSelector(item);
            }
        }
        return dict;
    }

    /// <summary>
    /// Lấy thông báo hiển thị cho lỗi khi import (tránh hiển thị "Exception of type 'Volo.Abp.BusinessException' was thrown.").
    /// </summary>
    private string GetDisplayMessageForImportError(Exception ex)
    {
        if (ex is BusinessException be && !string.IsNullOrWhiteSpace(be.Code))
            return L[be.Code].Value ?? be.Code;
        if (ex is UserFriendlyException ufe && !string.IsNullOrWhiteSpace(ufe.Message))
            return ufe.Message;
        var msg = ex?.Message?.Trim();
        if (!string.IsNullOrEmpty(msg) && !msg.Contains("Exception of type", StringComparison.OrdinalIgnoreCase))
            return msg;
        return L["Policy:ImportPolicy:Failed"].Value ?? "Import error.";
    }

    private static bool ResolveId(IStringLocalizer localizer, Dictionary<string, Guid> lookup, string code, string fieldName, int rowNumber, List<ImportPolicyExcelErrorDto> errors, out Guid id)
    {
        id = Guid.Empty;
        if (string.IsNullOrWhiteSpace(code)) return false;
        if (lookup.TryGetValue(code.Trim(), out id)) return true;

        // Code provided but not found in master data — report specific error
        var msg = localizer["Policy:ImportPolicy:ValueNotFoundInMaster", code.Trim(), fieldName];
        errors.Add(new ImportPolicyExcelErrorDto { RowNumber = rowNumber, Field = fieldName, Message = msg.Value, Value = code.Trim() });
        return false;
    }

    private static bool TryParseFlexibleDate(string dateStr, out DateTime result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(dateStr)) return false;

        // Try standard formats
        var formats = new[]
        {
            "dd/MM/yyyy", "d/M/yyyy", "dd/MM/yyyy HH:mm", "d/M/yyyy H:mm",
            "H:mm dd/MM/yyyy", "HH:mm dd/MM/yyyy",
            "yyyy-MM-dd", "yyyy-MM-dd HH:mm",
            "MM/yyyy", "M/yyyy"
        };

        if (DateTime.TryParseExact(dateStr, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
            return true;

        if (DateTime.TryParse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
            return true;

        // OA date (Excel serial number)
        if (double.TryParse(dateStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var oaDate) && oaDate > 1)
        {
            result = DateTime.FromOADate(oaDate);
            return true;
        }

        return false;
    }

    private static string? NullIfEmpty(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    #endregion
}