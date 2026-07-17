using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using iOne.PartnerIntegration.Pti.Models;
using iOne.Policies;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace iOne.PartnerIntegration.Pti;

/// <summary>
/// Maps iOne domain objects to a PTI <see cref="PtiCreateVehicleRequest"/>.
/// Codebook fields resolve via <see cref="IPtiInsurerCodebookResolver"/> and <c>insurer_dictionary</c>.
/// </summary>
public class PtiRequestMapper : ITransientDependency
{
    private static readonly CultureInfo PtiCulture = CultureInfo.InvariantCulture;
    private const string DateFormat = "dd/MM/yyyy";
    private const string TimeFormat = "HH:mm";

    private readonly IPtiInsurerCodebookResolver _codebookResolver;

    /// <summary>
    /// PTI package codes — Bảng 7 in PTI API doc.
    /// </summary>
    private static readonly Dictionary<string, (string Code, string Name, string AssuranceType)> PtiPackages = new()
    {
        ["MOTO_CIVIL_LIABILITY"] = ("PTISOS.XM.IBDS.0001",
            "Bảo hiểm bắt buộc Trách nhiệm dân sự của chủ xe mô tô - xe máy",
            "MOTO_CIVIL_LIABILITY"),
        ["MOTO_VEHICLE_PERSONAL_ACCIDENT"] = ("PTISOS.XM.IBDS.0002",
            "Bảo hiểm Trách nhiệm của Chủ xe/lái xe đối với Người ngồi trên xe máy",
            "MOTO_VEHICLE_PERSONAL_ACCIDENT")
    };

    public PtiRequestMapper(IPtiInsurerCodebookResolver codebookResolver)
    {
        _codebookResolver = codebookResolver;
    }

    public async Task<PtiCreateVehicleRequest> ToRequestAsync(
        Policy policy,
        PolicyVersion version,
        List<PolicyProduct> products,
        PolicyRiskMotor? motor,
        string channelCode,
        PartnerIssueBillInfo? billInfo = null,
        PartnerIssueOwnerInfo? ownerInfo = null,
        CancellationToken cancellationToken = default)
    {
        var insurerId = policy.Contract?.InsurerId;
        if (!insurerId.HasValue || insurerId.Value == Guid.Empty)
        {
            throw new BusinessException("Policy:Partner:PtiInsurerContractMissing")
                .WithData("PolicyId", policy.Id);
        }

        var id = insurerId.Value;

        var vehicleTypeOwn = ResolveVehicleTypeOwnCode(motor);
        var vehicleType = await _codebookResolver.ResolveRequiredAsync(
            id, PtiInsurerDictionaryCategories.MotorVehicleType, vehicleTypeOwn, cancellationToken);

        var vehicleStatusOwn = ResolveVehicleStatusOwnCode(motor);
        var vehicleStatus = await _codebookResolver.ResolveRequiredAsync(
            id, PtiInsurerDictionaryCategories.MotorVehicleStatus, vehicleStatusOwn, cancellationToken);

        var customerTypeOwn = ResolveCustomerTypeOwnCode(policy);
        var customerType = await _codebookResolver.ResolveRequiredAsync(
            id, PtiInsurerDictionaryCategories.MotorCustomerType, customerTypeOwn, cancellationToken);

        var orgType = await TryResolveOrgTypeAsync(id, customerType.Code, policy, cancellationToken);

        var ppcSelections = BuildPpcSelections(products);

        var effectDateStr = policy.OrgEffectDate.ToString(DateFormat, PtiCulture);
        var effectHourStr = policy.OrgEffectDate.ToString(TimeFormat, PtiCulture);
        var expiryDateStr = policy.OrgExpireDate.ToString(DateFormat, PtiCulture);
        var expiryHourStr = policy.OrgExpireDate.ToString(TimeFormat, PtiCulture);

        var customer = policy.Contract?.Customer;
        var customerInfo = new PtiCustomerInfo
        {
            CustomerType = customerType,
            OrgType      = orgType,
            FullName     = customer?.Name ?? "",
            Phone        = customer?.Phone ?? "",
            Address      = customer?.FullAddress ?? customer?.Address ?? "",
            Email        = string.IsNullOrWhiteSpace(customer?.Email) ? null : customer!.Email,
            TaxCode      = string.IsNullOrWhiteSpace(policy.InsuredTin) ? null : policy.InsuredTin
        };

        PtiCodebook? billActor = null;
        if (billInfo?.IsChoice == true && !string.IsNullOrWhiteSpace(billInfo.BillActor))
        {
            billActor = await _codebookResolver.ResolveRequiredAsync(
                id,
                PtiInsurerDictionaryCategories.MotorBillActor,
                billInfo.BillActor.Trim(),
                cancellationToken);
        }

        return new PtiCreateVehicleRequest
        {
            ChannelCode = channelCode,
            TransId = policy.Id.ToString(),
            VehicleInfo = new PtiVehicleInfo
            {
                VehicleType    = vehicleType,
                VehicleStatus  = vehicleStatus,
                LicensePlate   = string.IsNullOrWhiteSpace(motor?.CarPlate) ? null : (motor!.CarPlateClear ?? motor!.CarPlate),
                FrameNumber    = string.IsNullOrWhiteSpace(motor?.CarVin) ? null : motor!.CarVin,
                EngineNumber   = string.IsNullOrWhiteSpace(motor?.CarEngineNumber) ? null : motor!.CarEngineNumber
            },
            PpcInfoSelection = ppcSelections,
            InsurancePeriod = new PtiInsurancePeriod
            {
                EffectiveDate = effectDateStr,
                EffectiveHour = effectHourStr,
                ExpiryDate = expiryDateStr,
                ExpiryHour = expiryHourStr
            },
            CustomerInfo = customerInfo,
            OwnerInfo = BuildOwnerInfo(ownerInfo),
            BeneficiaryInfo = new PtiBeneficiaryInfo { IsOwner = true },
            BillInfo = new PtiBillInfo
            {
                IsChoice = billInfo?.IsChoice ?? false,
                BillActor = billActor,
                Email = string.IsNullOrWhiteSpace(billInfo?.Email) ? null : billInfo!.Email
            },
            CoverageLimit = BuildCoverageLimit(products, motor)
        };
    }

    private static PtiCoverageLimit? BuildCoverageLimit(List<PolicyProduct> products, PolicyRiskMotor? motor)
    {
        var accidentProduct = products.FirstOrDefault(p =>
        {
            var ic = p.InsurerProductCode ?? p.Product?.InsurerProductCode ?? p.Product?.ProductType?.Code ?? "";
            return ic.Contains("ACCIDENT", StringComparison.OrdinalIgnoreCase)
                   || ic.Contains("PERSONAL", StringComparison.OrdinalIgnoreCase)
                   || ic.Contains("0002", StringComparison.OrdinalIgnoreCase);
        });

        if (accidentProduct == null) return null;

        var amountLiability = (long)(accidentProduct.AmountLiability ?? 0);
        var numPeople = (int)(motor?.CarSeatNumber ?? 0);

        // Default to 2 for motor if not specified
        if (numPeople <= 0) numPeople = 2;

        var amountStr = amountLiability.ToString("N0", PtiCulture).Replace(",", ".");

        // Mapping codes based on standard PTI levels: 10M -> MTN_10, 20M -> MTN_20, etc.
        var codeSuffix = (amountLiability / 1_000_000).ToString("00");
        var mtnCode = $"MTN_{codeSuffix}";

        return new PtiCoverageLimit
        {
            AssuranceRange = new List<PtiAssuranceRange>
            {
                new PtiAssuranceRange
                {
                    Name = $"BH tai nạn người ngồi trên xe: {numPeople:00} người",
                    Code = $"XM.{numPeople}",
                    Value = new List<PtiAssuranceValue>
                    {
                        new PtiAssuranceValue
                        {
                            Code = mtnCode,
                            Name = amountStr,
                            IsChoice = true
                        }
                    },
                    Unit = "đồng/người/vụ",
                    IsChoice = true,
                    NumberPeople = numPeople
                }
            }
        };
    }

    private static string ResolveVehicleTypeOwnCode(PolicyRiskMotor? motor)
    {
        var code = motor?.CarTypeCode?.Trim();
        return string.IsNullOrEmpty(code)
            ? PtiInsurerDictionaryCategories.MotorVehicleTypeFallbackOwnCode
            : code;
    }

    private static string ResolveVehicleStatusOwnCode(PolicyRiskMotor? motor)
    {
        // If there is a license plate, the vehicle is considered "OLD" (in circulation).
        // Otherwise, it is considered "NEW" (not yet in circulation).
        return motor != null && !string.IsNullOrWhiteSpace(motor.CarPlate)
            ? "OLD"
            : "NEW";
    }

    /// <summary>
    /// Own-code keys for <c>MOTOR_CUSTOMER_TYPE</c> must match <c>insurer_dictionary.own_code</c>
    /// (typically <c>LKH.CN</c> / <c>LKH.DN</c> / <c>LKH.PDN</c>).
    /// </summary>
    private static string ResolveCustomerTypeOwnCode(Policy policy)
    {
        if (string.IsNullOrWhiteSpace(policy.InsuredOrgType))
            return "LKH.CN";

        return "LKH.DN";
    }

    private async Task<PtiCodebook?> TryResolveOrgTypeAsync(
        Guid insurerId,
        string resolvedCustomerTypeInsurerCode,
        Policy policy,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(policy.InsuredOrgType))
            return null;

        var own = policy.InsuredOrgType.Trim();

        if (string.Equals(resolvedCustomerTypeInsurerCode, "LKH.DN", StringComparison.OrdinalIgnoreCase))
        {
            return await _codebookResolver.ResolveRequiredAsync(
                insurerId, PtiInsurerDictionaryCategories.MotorOrgTypeDn, own, cancellationToken);
        }

        if (string.Equals(resolvedCustomerTypeInsurerCode, "LKH.PDN", StringComparison.OrdinalIgnoreCase))
        {
            return await _codebookResolver.ResolveRequiredAsync(
                insurerId, PtiInsurerDictionaryCategories.MotorOrgTypePdn, own, cancellationToken);
        }

        return null;
    }

    private static List<PtiPpcInfoSelection> BuildPpcSelections(List<PolicyProduct> products)
    {
        var selected = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var product in products)
        {
            var insurerCode = product.InsurerProductCode
                ?? product.Product?.InsurerProductCode
                ?? product.Product?.ProductType?.Code
                ?? "";

            if (insurerCode.Contains("TNDS", StringComparison.OrdinalIgnoreCase)
                || insurerCode.Contains("CIVIL", StringComparison.OrdinalIgnoreCase)
                || insurerCode.Contains("0001", StringComparison.OrdinalIgnoreCase))
            {
                selected.Add("MOTO_CIVIL_LIABILITY");
            }

            if (insurerCode.Contains("ACCIDENT", StringComparison.OrdinalIgnoreCase)
                || insurerCode.Contains("PERSONAL", StringComparison.OrdinalIgnoreCase)
                || insurerCode.Contains("0002", StringComparison.OrdinalIgnoreCase))
            {
                selected.Add("MOTO_VEHICLE_PERSONAL_ACCIDENT");
            }
        }

        var result = new List<PtiPpcInfoSelection>();

        foreach (var (key, (code, name, assuranceType)) in PtiPackages)
        {
            var isChoice = selected.Contains(key);
            var feeValue = 0;

            if (isChoice)
            {
                var matchingProduct = products.FirstOrDefault(p =>
                {
                    var ic = p.InsurerProductCode ?? p.Product?.InsurerProductCode ?? p.Product?.ProductType?.Code ?? "";
                    return (key == "MOTO_CIVIL_LIABILITY" &&
                            (ic.Contains("TNDS", StringComparison.OrdinalIgnoreCase)
                             || ic.Contains("CIVIL", StringComparison.OrdinalIgnoreCase)
                             || ic.Contains("0001", StringComparison.OrdinalIgnoreCase)))
                        || (key == "MOTO_VEHICLE_PERSONAL_ACCIDENT" &&
                            (ic.Contains("ACCIDENT", StringComparison.OrdinalIgnoreCase)
                             || ic.Contains("PERSONAL", StringComparison.OrdinalIgnoreCase)
                             || ic.Contains("0002", StringComparison.OrdinalIgnoreCase)));
                });

                feeValue = (int)(matchingProduct?.PremiumTotal ?? 0);
            }

            result.Add(new PtiPpcInfoSelection
            {
                Code = code,
                Name = name,
                IsChoice = isChoice,
                AssuranceType = assuranceType,
                TargetFeeValue = feeValue
            });
        }

        return result;
    }

    private static PtiOwnerInfo BuildOwnerInfo(PartnerIssueOwnerInfo? ownerInfo)
    {
        var isCustomer = ownerInfo?.IsCustomer ?? true;

        if (isCustomer)
            return new PtiOwnerInfo { IsCustomer = true };

        return new PtiOwnerInfo
        {
            IsCustomer   = false,
            CustomerType = new PtiCodebook { Code = "LKH.CN", Name = "Cá nhân" },
            FullName     = ownerInfo?.FullName,
            Phone        = ownerInfo?.Phone,
            Address      = ownerInfo?.Address,
            Email        = ownerInfo?.Email
        };
    }
}
