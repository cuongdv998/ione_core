using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using iOne.Product.Localization;
using iOne.Product.Permissions;
using iOne.Product.ProductPricing;
using iOne.ProAttributes;
using iOne.ProProducts;
using iOne.ProRules;
using iOne.ProRuleTypes;
using iOne.ProTableRateLines;
using iOne.ProTableRates;
using iOne.ProTableRateVariables;
using iOne.ResTaxes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace iOne.Product.ProductPricing;

/// <summary>
/// Application service for calculating insurance product premiums.
/// PremiumVAT is computed first (from baseRate/100 * amountLiability, or flatRate, or minimumRate floor),
/// then premium is derived by reversing VAT when rate source is BaseRate: premium = premiumVAT / (1 + tax/100).
/// </summary>
[RemoteService(Name = ProductRemoteServiceConsts.RemoteServiceName)]
[Authorize(ProProductPermissions.View)]
public class ProductPriceAppService : ApplicationService, IProductPriceAppService
{
    protected IProProductRepository ProductRepository { get; }
    protected IRepository<ProProductTableRate, Guid> ProductTableRateRepository { get; }
    protected IRepository<ProTableRate, Guid> TableRateRepository { get; }
    protected IRepository<ProTableRateVariable, Guid> TableRateVariableRepository { get; }
    protected IRepository<ProAttribute, Guid> AttributeRepository { get; }
    protected IRepository<ProProductAttribute, Guid> ProductAttributeRepository { get; }
    protected IRepository<ProProductCoverage, Guid> ProductCoverageRepository { get; }
    protected IProTableRateLineRepository TableRateLineRepository { get; }
    protected IRepository<ResTax, Guid> TaxRepository { get; }
    protected IRepository<ProRule, Guid> ProRuleRepository { get; }
    protected IRepository<ProRuleType, Guid> ProRuleTypeRepository { get; }

    public ProductPriceAppService(
        IProProductRepository productRepository,
        IRepository<ProProductTableRate, Guid> productTableRateRepository,
        IRepository<ProTableRate, Guid> tableRateRepository,
        IRepository<ProTableRateVariable, Guid> tableRateVariableRepository,
        IRepository<ProAttribute, Guid> attributeRepository,
        IRepository<ProProductAttribute, Guid> productAttributeRepository,
        IRepository<ProProductCoverage, Guid> productCoverageRepository,
        IProTableRateLineRepository tableRateLineRepository,
        IRepository<ResTax, Guid> taxRepository,
        IRepository<ProRule, Guid> proRuleRepository,
        IRepository<ProRuleType, Guid> proRuleTypeRepository)
    {
        ProductRepository = productRepository;
        ProductTableRateRepository = productTableRateRepository;
        TableRateRepository = tableRateRepository;
        TableRateVariableRepository = tableRateVariableRepository;
        AttributeRepository = attributeRepository;
        ProductAttributeRepository = productAttributeRepository;
        ProductCoverageRepository = productCoverageRepository;
        TableRateLineRepository = tableRateLineRepository;
        TaxRepository = taxRepository;
        ProRuleRepository = proRuleRepository;
        ProRuleTypeRepository = proRuleTypeRepository;
        LocalizationResource = typeof(ProductResource);
    }

    /// <inheritdoc />
    public async Task<CalculatePremiumResponseDto> CalculatePremiumAsync(string productCode, CalculatePremiumRequestDto input)
    {
        // 1. Validate and get product by code
        var product = await GetProductByCodeAsync(productCode);

        // 2. Validate productId in request matches (if provided)
        if (input.ProductId.HasValue && input.ProductId.Value != product.Id)
        {
            throw new UserFriendlyException(L["Product:Price:ProductIdMismatch"]);
        }

        // 3. Get product's table rates (active ones)
        var productTableRates = await GetActiveProductTableRatesAsync(product.Id);
        if (!productTableRates.Any())
        {
            throw new UserFriendlyException(L["Product:Price:NoTableRatesConfigured"]);
        }

        // 4. Get table rate IDs and load their variables with attribute codes
        var tableRateIds = productTableRates.Select(ptr => ptr.TableRateId).ToList();
        var variablesWithAttributes = await GetTableRateVariablesWithAttributesAsync(tableRateIds);

        // Extract coverage codes from ProductCoverages for rate line matching (condition->'coverages' overlap)
        var coverageCodes = input.ProductCoverages
            .Select(c => c.CoverageCode)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct()
            .ToList();

        // Inject coverageCode into Attributes so rule/script can access the full set of selected coverage codes
        var coverageCodeValue = coverageCodes.Any() ? string.Join(",", coverageCodes) : null;
        if (!string.IsNullOrEmpty(coverageCodeValue))
        {
            if (input.Attributes.ContainsKey("coverageCode"))
                input.Attributes["coverageCode"] = coverageCodeValue;
            else
                input.Attributes.Add("coverageCode", coverageCodeValue);
        }
        else if (input.Attributes.ContainsKey("coverageCode"))
        {
            input.Attributes.Remove("coverageCode");
        }
        
        // 5. Validate that all required attributes are provided
        var requiredAttributeCodes = await GetRequiredProductAttributeCodesAsync(product.Id);
        ValidateRequiredAttributes(requiredAttributeCodes, input.Attributes);

        // 6. Calculate premiums for each coverage
        var response = new CalculatePremiumResponseDto
        {
            ProductId = product.Id,
            Attributes = input.Attributes
        };

        foreach (var coverageRequest in input.ProductCoverages)
        {
            // Validate product coverage exists and belongs to this product
            await ValidateProductCoverageAsync(product.Id, coverageRequest);

            // Try to find matching rate line for this coverage
            if (input.Attributes.ContainsKey("amountLiability"))
            {
                input.Attributes["amountLiability"] = coverageRequest.AmountLiability; // override
            }
            else
            {
                input.Attributes.Add("amountLiability", coverageRequest.AmountLiability);
            }

            // Mức miễn thường (deductible) – cùng cấp với coverageCode, amountLiability; đưa vào Attributes cho rule/script tính phí
            if (!string.IsNullOrEmpty(coverageRequest.Deductible))
            {
                if (input.Attributes.ContainsKey("deductibles"))
                    input.Attributes["deductibles"] = coverageRequest.Deductible;
                else
                    input.Attributes.Add("deductibles", coverageRequest.Deductible);
            }
            else if (input.Attributes.ContainsKey("deductibles"))
            {
                input.Attributes.Remove("deductibles");
            }

            var matchedPremium = await CalculateCoveragePremiumAsync(
                tableRateIds,
                variablesWithAttributes,
                coverageRequest,
                input.Attributes);

            // Only add to response if a matching rate was found
            if (matchedPremium != null)
            {
                response.ProductCoverages.Add(matchedPremium);
            }
        }

        input.Attributes.Remove("amountLiability");
        if (input.Attributes.ContainsKey("deductibles"))
            input.Attributes.Remove("deductibles");
        if (input.Attributes.ContainsKey("coverageCode"))
            input.Attributes.Remove("coverageCode");

        // 7. Rate-by-time: when both dates provided and not exactly 1 year, apply RATE_BY_TIME rule if found
        if (!input.EffectiveDate.HasValue || !input.ExpireDate.HasValue ||
            IsExactlyOneYearApart(input.EffectiveDate.Value, input.ExpireDate.Value))
        {
            ApplyVndRoundingIfNeeded(response, product.Currency?.Code);
            return response;
        }

        var rateByTimeRule = await GetRateByTimeRuleAsync(product.Id);
        if (rateByTimeRule == null || string.IsNullOrWhiteSpace(rateByTimeRule.RuleScript))
        {
            ApplyVndRoundingIfNeeded(response, product.Currency?.Code);
            return response;
        }

        var rate = await ExecuteRateByTimeRuleScriptAsync(input, product.Id, rateByTimeRule.RuleScript);
        if (rate < 0)
        {
            throw new UserFriendlyException(L["Product:Price:RateByTimeInvalidRate"]);
        }

        foreach (var coverage in response.ProductCoverages)
        {
            var premiumVat = coverage.PremiumVat ?? 0;
            var newPremiumVat = premiumVat * rate;
            coverage.PremiumVat = newPremiumVat;

            if (coverage.Rate?.TaxValue.HasValue == true && coverage.Rate.TaxValue.Value > 0)
            {
                coverage.Premium = newPremiumVat / (1 + coverage.Rate.TaxValue.Value / 100m);
            }
            else
            {
                coverage.Premium = newPremiumVat;
            }

            coverage.Vat = newPremiumVat - coverage.Premium;
        }

        ApplyVndRoundingIfNeeded(response, product.Currency?.Code);
        return response;
    }

    private const string CurrencyCodeVnd = "VND";

    /// <summary>
    /// Rounds Premium, PremiumVat, and Vat to whole numbers for each coverage when product currency is VND.
    /// </summary>
    private static void ApplyVndRoundingIfNeeded(CalculatePremiumResponseDto response, string? currencyCode)
    {
        if (string.IsNullOrEmpty(currencyCode) || currencyCode != CurrencyCodeVnd)
        {
            return;
        }

        foreach (var coverage in response.ProductCoverages)
        {
            coverage.Premium = Math.Round(coverage.Premium, 0, MidpointRounding.AwayFromZero);
            if (coverage.PremiumVat.HasValue)
            {
                coverage.PremiumVat = Math.Round(coverage.PremiumVat.Value, 0, MidpointRounding.AwayFromZero);
            }

            if (coverage.Vat.HasValue)
            {
                coverage.Vat = coverage.PremiumVat - coverage.Premium;
            }
        }
    }

    /// <summary>
    /// Returns true if expireDate is exactly one year after effectiveDate (date parts only).
    /// </summary>
    private static bool IsExactlyOneYearApart(DateTime effectiveDate, DateTime expireDate)
    {
        return effectiveDate.Date.AddYears(1) == expireDate.Date;
    }

    /// <summary>
    /// Gets a product by its code.
    /// </summary>
    private async Task<ProProduct> GetProductByCodeAsync(string productCode)
    {
        if (string.IsNullOrWhiteSpace(productCode))
        {
            throw new UserFriendlyException(L["Product:Price:ProductCodeRequired"]);
        }

        var query = await ProductRepository.GetQueryableAsync();
        var product = await query
            .Include(p => p.Currency)
            .Where(p => EF.Functions.ILike(p.Code, productCode))
            .FirstOrDefaultAsync();

        if (product == null)
        {
            throw new UserFriendlyException(L["Product:Price:ProductNotFound"])
                .WithData("Code", productCode);
        }

        // Validate product is active
        if (product.Status != ProProductStatus.Active)
        {
            throw new UserFriendlyException(L["Product:Price:ProductNotActive"])
                .WithData("Code", productCode);
        }

        return product;
    }

    /// <summary>
    /// Gets active table rates for a product (within effect/expire date range).
    /// </summary>
    private async Task<List<ProProductTableRate>> GetActiveProductTableRatesAsync(Guid productId)
    {
        var today = DateTime.Today;
        var query = await ProductTableRateRepository.GetQueryableAsync();
        
        return await query
            .Where(ptr => ptr.ProductId == productId)
            .Where(ptr => ptr.EffectDate <= today)
            .Where(ptr => ptr.ExpireDate == null || ptr.ExpireDate >= today)
            .ToListAsync();
    }

    /// <summary>
    /// Gets table rate variables with their attribute codes and operators.
    /// </summary>
    private async Task<List<TableRateVariableInfo>> GetTableRateVariablesWithAttributesAsync(List<Guid> tableRateIds)
    {
        var variableQuery = await TableRateVariableRepository.GetQueryableAsync();
        var attributeQuery = await AttributeRepository.GetQueryableAsync();

        var variables = await variableQuery
            .Where(v => tableRateIds.Contains(v.TableRateId))
            .Join(
                attributeQuery,
                v => v.AttributeId,
                a => a.Id,
                (v, a) => new TableRateVariableInfo
                {
                    TableRateId = v.TableRateId,
                    AttributeId = v.AttributeId,
                    AttributeCode = a.Code,
                    Operator = v.Operator
                })
            .ToListAsync();

        return variables;
    }

    /// <summary>
    /// Returns attribute codes that are marked as required (is_required = 'Y') for the given product,
    /// sourced from pro_product_attribute joined with pro_attribute.
    /// </summary>
    private async Task<List<string>> GetRequiredProductAttributeCodesAsync(Guid productId)
    {
        var ppaQuery = await ProductAttributeRepository.GetQueryableAsync();
        var attrQuery = await AttributeRepository.GetQueryableAsync();

        return await ppaQuery
            .Where(ppa => ppa.ProductId == productId && ppa.IsRequired == "Y")
            .Join(attrQuery,
                  ppa => ppa.AttributeId,
                  a => a.Id,
                  (ppa, a) => a.Code)
            .ToListAsync();
    }

    /// <summary>
    /// Validates that all required attributes (from pro_product_attribute where is_required='Y') are provided in the request.
    /// </summary>
    private void ValidateRequiredAttributes(
        List<string> requiredAttributeCodes,
        Dictionary<string, object> providedAttributes)
    {
        var missingAttributes = requiredAttributeCodes
            .Where(code => !providedAttributes.ContainsKey(code))
            .ToList();

        if (missingAttributes.Any())
        {
            throw new UserFriendlyException(L["Product:Price:MissingAttributes"])
                .WithData("Attributes", string.Join(", ", missingAttributes));
        }
    }

    /// <summary>
    /// Validates that the product coverage exists and belongs to the product.
    /// </summary>
    private async Task ValidateProductCoverageAsync(Guid productId, ProductCoveragePremiumRequestDto coverageRequest)
    {
        var query = await ProductCoverageRepository.GetQueryableAsync();
        var productCoverage = await query
            .Where(pc => pc.Id == coverageRequest.ProductCoverageId)
            .Where(pc => pc.ProductId == productId)
            .FirstOrDefaultAsync();

        if (productCoverage == null)
        {
            throw new UserFriendlyException(L["Product:Price:ProductCoverageNotFound"])
                .WithData("ProductCoverageId", coverageRequest.ProductCoverageId);
        }

        // Validate coverage ID matches
        if (productCoverage.CoverageId != coverageRequest.CoverageId)
        {
            throw new UserFriendlyException(L["Product:Price:CoverageIdMismatch"])
                .WithData("ProductCoverageId", coverageRequest.ProductCoverageId)
                .WithData("ExpectedCoverageId", productCoverage.CoverageId)
                .WithData("ProvidedCoverageId", coverageRequest.CoverageId);
        }

        // Validate amount liability is positive
        if (coverageRequest.AmountLiability < 0)
        {
            throw new UserFriendlyException(L["Product:Price:AmountLiabilityMustBePositive"]);
        }
    }

    /// <summary>
    /// Calculates premium for a single coverage by finding matching rate line. Uses premiumVAT-first flow:
    /// premiumVAT per unit from rate, then premiumVAT total (× quantity), then premium from reverse VAT formula.
    /// When coverageCodes is provided, rate lines with condition->'coverages' overlapping the list will match.
    /// </summary>
    private async Task<ProductCoveragePremiumResponseDto?> CalculateCoveragePremiumAsync(
        List<Guid> tableRateIds,
        List<TableRateVariableInfo> variablesWithAttributes,
        ProductCoveragePremiumRequestDto coverageRequest,
        Dictionary<string, object> attributes)
    {
        // Try each table rate until we find a matching rate line
        foreach (var tableRateId in tableRateIds)
        {
            // Get variables for this specific table rate
            var tableVariables = variablesWithAttributes
                .Where(v => v.TableRateId == tableRateId)
                .ToList();

            // Build conditions dictionary with operators
            var conditions = new Dictionary<string, (string Operator, object Value)>();
            foreach (var variable in tableVariables)
            {
                if (attributes.TryGetValue(variable.AttributeCode, out var value))
                {
                    conditions[variable.AttributeCode] = (variable.Operator, value);
                }
            }

            // Find matching rate line (coverageCodes enables overlap match on condition->'coverages')
            var matchingRateLine = await TableRateLineRepository.FindMatchingRateLineAsync(
                tableRateId,
                coverageRequest.CoverageId,
                conditions);

            if (matchingRateLine == null)
            {
                continue;
            }

            // 1. Calculate premiumVAT per unit from rate line (baseRate nếu có, không thì flatRate; nếu loading != null và != 0 thì cộng thêm vào rate)
            var (premiumVatPerUnit, rateSource) = CalculatePremiumVatPerUnit(matchingRateLine, coverageRequest.AmountLiability);

            // 2. Apply quantity
            var premiumVatTotal = premiumVatPerUnit * coverageRequest.Quantity;
            if (matchingRateLine.MinimumRate.HasValue && premiumVatTotal < matchingRateLine.MinimumRate.Value)
            {
                premiumVatTotal = matchingRateLine.MinimumRate.Value;
            }

            // 3. Reverse to premium using tax (premium = premiumVatTotal / (1 + tax/100) when BaseRate)
            var (premiumTotal, taxValue) = await CalculatePremiumFromVatAsync(coverageRequest.ProductCoverageId, premiumVatTotal, rateSource);

            // Effective rate for response: rate + loading when loading != null && != 0 (để UI hiển thị đúng tỷ lệ đã áp dụng)
            var effectiveBaseRate = matchingRateLine.BaseRate;
            var effectiveFlatRate = matchingRateLine.FlatRate;
            if (matchingRateLine.Loading.HasValue && matchingRateLine.Loading.Value != 0)
            {
                if (matchingRateLine.BaseRate.HasValue)
                    effectiveBaseRate = matchingRateLine.BaseRate.Value + matchingRateLine.Loading.Value;
                else
                    effectiveFlatRate = (matchingRateLine.FlatRate ?? 0) + matchingRateLine.Loading.Value;
            }

            return new ProductCoveragePremiumResponseDto
            {
                ProductCoverageId = coverageRequest.ProductCoverageId,
                CoverageId = coverageRequest.CoverageId,
                Rate = new MatchedRateDto
                {
                    RateId = matchingRateLine.Id,
                    BaseRate = effectiveBaseRate,
                    FlatRate = effectiveFlatRate,
                    Loading = matchingRateLine.Loading,
                    TaxValue = taxValue
                },
                Premium = premiumTotal,
                PremiumVat = premiumVatTotal,
                Vat = premiumVatTotal - premiumTotal
            };
        }

        // No matching rate found for this coverage - return null (will be omitted from response)
        return null;
    }

    /// <summary>
    /// Calculates premiumVAT per unit from the rate line.
    /// Rate: nếu baseRate != null thì dùng baseRate, không thì dùng flatRate.
    /// Nếu loading != null và != 0 thì cộng thêm loading vào rate hiện tại rồi mới tính phí.
    /// BaseRate là phần trăm (0-100); chia 100 khi dùng.
    /// </summary>
    /// <returns>A tuple containing (premiumVatPerUnit, rateSource).</returns>
    private static (decimal PremiumVatPerUnit, PremiumRateSource Source) CalculatePremiumVatPerUnit(ProTableRateLine rateLine, decimal amountLiability)
    {
        decimal premiumVatPerUnit;
        PremiumRateSource source;
        var loading = (rateLine.Loading.HasValue && rateLine.Loading.Value != 0) ? rateLine.Loading.Value : 0m;

        if (rateLine.BaseRate.HasValue)
        {
            var effectiveRate = rateLine.BaseRate.Value + loading;
            premiumVatPerUnit = (effectiveRate / 100m) * amountLiability;
            source = PremiumRateSource.BaseRate;
        }
        else
        {
            var effectiveRate = (rateLine.FlatRate ?? 0m) + loading;
            premiumVatPerUnit = effectiveRate;
            source = PremiumRateSource.FlatRate;
        }

        return (premiumVatPerUnit, source);
    }

    /// <summary>
    /// Derives premium from premiumVAT using the reverse formula. When rate source is BaseRate:
    /// premiumTotal = premiumVatTotal / (1 + tax/100). Otherwise premiumTotal = premiumVatTotal (no VAT).
    /// </summary>
    /// <returns>A tuple containing (premiumTotal, taxValue). TaxValue is null if tax not found.</returns>
    private async Task<(decimal PremiumTotal, decimal? TaxValue)> CalculatePremiumFromVatAsync(Guid productCoverageId, decimal premiumVatTotal, PremiumRateSource rateSource)
    {
        // Get the product coverage to retrieve TaxId
        var query = await ProductCoverageRepository.GetQueryableAsync();
        var productCoverage = await query
            .Where(pc => pc.Id == productCoverageId)
            .FirstOrDefaultAsync();

        if (productCoverage == null)
        {
            return (premiumVatTotal, null);
        }

        // Get the tax record to retrieve tax value (TaxId is required in ProProductCoverage)
        var tax = await TaxRepository.FindAsync(productCoverage.TaxId);
        if (tax == null)
        {
            return (premiumVatTotal, null);
        }

        decimal premiumTotal = premiumVatTotal / (1 + tax.Value / 100m);
        return (premiumTotal, tax.Value);
    }

    /// <summary>
    /// Gets the RATE_BY_TIME ProRule for the product (ApplyTo = "product", ApplyToId = productId, RuleType = RATE_BY_TIME, Active).
    /// Returns null if rule type or rule not found.
    /// </summary>
    private async Task<ProRule?> GetRateByTimeRuleAsync(Guid productId)
    {
        var ruleTypeQuery = await ProRuleTypeRepository.GetQueryableAsync();
        var rateByTimeType = await ruleTypeQuery
            .Where(rt => rt.Code == "RATE_BY_TIME")
            .FirstOrDefaultAsync();

        if (rateByTimeType == null)
        {
            return null;
        }

        var ruleQuery = await ProRuleRepository.GetQueryableAsync();
        var rule = await ruleQuery
            .Where(r => r.ApplyTo == "product" && r.ApplyToId == productId && r.RuleTypeId == rateByTimeType.Id && r.Status == ProRuleStatus.Active)
            .FirstOrDefaultAsync();

        return rule;
    }

    /// <summary>
    /// Executes the RATE_BY_TIME rule script (Python) with effectiveDate, expireDate, productId, attributes.
    /// Script must set variable "rate" (float). Returns the rate or throws if invalid.
    /// </summary>
    private async Task<decimal> ExecuteRateByTimeRuleScriptAsync(CalculatePremiumRequestDto input, Guid productId, string ruleScript)
    {
        var payload = new
        {
            effectiveDate = input.EffectiveDate!.Value.ToString("yyyy-MM-dd"),
            expireDate = input.ExpireDate!.Value.ToString("yyyy-MM-dd"),
            productId = productId.ToString(),
            attributes = input.Attributes ?? new Dictionary<string, object>()
        };

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });

        var b64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

        var pythonProgram = $@"
import base64
import json
import sys
import datetime as _datetime_module

payload = json.loads(base64.b64decode(sys.argv[1]).decode('utf-8'))
effectiveDate = _datetime_module.datetime.strptime(payload.get('effectiveDate'), '%Y-%m-%d').date()
expireDate = _datetime_module.datetime.strptime(payload.get('expireDate'), '%Y-%m-%d').date()
productId = payload.get('productId')
attributes = payload.get('attributes') or {{}}

rate = 1.0
ruleScript = '''{ruleScript.Replace("'", "\\'")}'''
local_ctx = {{'effectiveDate': effectiveDate, 'expireDate': expireDate, 'productId': productId, 'attributes': attributes, 'rate': rate}}
glob_ctx = {{'datetime': _datetime_module.datetime}}

try:
    if ruleScript.strip():
        try:
            result = eval(ruleScript, glob_ctx, local_ctx)
            if result is not None:
                rate = float(result)
        except SyntaxError:
            exec(ruleScript, glob_ctx, local_ctx)
            if 'rate' in local_ctx:
                rate = float(local_ctx['rate'])
except Exception as e:
    sys.stderr.write(f'Rule script error: {{e}}\\n')
    sys.exit(2)

if 'rate' not in local_ctx:
    sys.stderr.write('RATE_BY_TIME rule script must set variable rate (float)\\n')
    sys.exit(2)
rate = float(local_ctx['rate'])
print(json.dumps({{'rate': rate}}, ensure_ascii=False))
";

        var stdout = await RunPythonAsync(b64, pythonProgram);

        using var doc = JsonDocument.Parse(stdout);
        if (doc.RootElement.TryGetProperty("rate", out var rateElement))
        {
            var rate = rateElement.GetDecimal();
            return rate;
        }

        throw new UserFriendlyException(L["Product:Price:RateByTimeNoRateReturned"]);
    }

    /// <summary>
    /// Runs a Python program with base64-encoded payload as first argument. Returns stdout.
    /// </summary>
    private static async Task<string> RunPythonAsync(string payloadB64, string pythonProgram)
    {
        var overrideExe = (Environment.GetEnvironmentVariable("IONE_PYTHON") ?? string.Empty).Trim();

        var candidates = new List<string>();
        if (!string.IsNullOrWhiteSpace(overrideExe))
        {
            candidates.Add(overrideExe);
        }
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
                    throw new UserFriendlyException(
                        $"RATE_BY_TIME rule script failed (Python '{exe}' exit {p.ExitCode}). {stderr}".Trim());
                }

                return stdout;
            }
            catch (UserFriendlyException)
            {
                throw;
            }
            catch (Exception ex)
            {
                lastError = ex;
            }
        }

        var tried = string.Join(", ", candidates.Distinct(StringComparer.OrdinalIgnoreCase));
        var overrideInfo = string.IsNullOrWhiteSpace(overrideExe) ? "not set" : overrideExe;
        throw new UserFriendlyException(
            ("Python runtime not available for RATE_BY_TIME rule. " +
             $"IONE_PYTHON: {overrideInfo}. WorkingDirectory: {workingDir}. Tried: {tried}. " +
             "Please install python3 and set IONE_PYTHON if needed. " +
             $"{lastError?.Message}").Trim());
    }

    /// <summary>
    /// Indicates which rate was used to compute the premium (for VAT logic).
    /// </summary>
    private enum PremiumRateSource
    {
        BaseRate,
        FlatRate,
        MinimumRate
    }

    /// <summary>
    /// Helper class to hold table rate variable information with attribute code.
    /// </summary>
    private class TableRateVariableInfo
    {
        public Guid TableRateId { get; set; }
        public Guid AttributeId { get; set; }
        public string AttributeCode { get; set; } = null!;
        public string Operator { get; set; } = null!;
    }
}
