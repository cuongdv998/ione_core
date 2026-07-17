using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using iOne.Policies;
using iOne.ProProducts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace iOne.Policy.Policies;

[RemoteService(Name = PolicyRemoteServiceConsts.RemoteServiceName)]
[Authorize] // default: only requires login
    public class PolicyClaimLookupAppService : ApplicationService, IPolicyClaimLookupAppService
    {
        protected IRepository<iOne.Policies.Policy, Guid> PolicyRepository { get; }
        protected IRepository<PolicyVersion, Guid> PolicyVersionRepository { get; }
        protected IRepository<PolicyCertificate, Guid> PolicyCertificateRepository { get; }
        protected IRepository<PolicyRiskObject, Guid> PolicyRiskObjectRepository { get; }
        protected IRepository<PolicyRiskMotor, Guid> PolicyRiskMotorRepository { get; }
        protected IRepository<ProProduct, Guid> ProProductRepository { get; }
        protected IRepository<PolicyAmount, Guid> PolicyAmountRepository { get; }

        public PolicyClaimLookupAppService(
            IRepository<iOne.Policies.Policy, Guid> policyRepository,
            IRepository<PolicyVersion, Guid> policyVersionRepository,
            IRepository<PolicyCertificate, Guid> policyCertificateRepository,
            IRepository<PolicyRiskObject, Guid> policyRiskObjectRepository,
            IRepository<PolicyRiskMotor, Guid> policyRiskMotorRepository,
            IRepository<ProProduct, Guid> proProductRepository,
            IRepository<PolicyAmount, Guid> policyAmountRepository)
        {
            PolicyRepository = policyRepository;
            PolicyVersionRepository = policyVersionRepository;
            PolicyCertificateRepository = policyCertificateRepository;
            PolicyRiskObjectRepository = policyRiskObjectRepository;
            PolicyRiskMotorRepository = policyRiskMotorRepository;
            ProProductRepository = proProductRepository;
            PolicyAmountRepository = policyAmountRepository;
        }

    /// <summary>
    /// Lấy đơn bảo hiểm theo tiêu chí: CertificateNo/BSX/SK/SM dùng để **xác định** đơn bảo hiểm.
    /// Khi có nhiều tiêu chí thì dùng AND (đơn phải thỏa tất cả), không phải liệt kê mọi đơn trùng từng tiêu chí.
    /// Tối ưu: 1 query lọc ID (EXISTS) + 1 query load policy kèm includes.
    /// </summary>
    public virtual async Task<List<PolicyClaimLookupDto>> GetClaimLookupAsync(GetPolicyClaimLookupInput input)
    {
        var certificateNo = input.CertificateNo?.Trim();
        var carPlate = input.CarPlate?.Trim();
        var vin = input.Vin?.Trim();
        var engineNumber = input.EngineNumber?.Trim();
        // Nếu không truyền IncidentDate từ client, mặc định dùng ngày hiện tại để chỉ lấy đơn còn hiệu lực
        var incidentDate = input.IncidentDate ?? DateTime.Now;

        if (string.IsNullOrWhiteSpace(certificateNo) &&
            string.IsNullOrWhiteSpace(carPlate) &&
            string.IsNullOrWhiteSpace(vin) &&
            string.IsNullOrWhiteSpace(engineNumber))
        {
            return new List<PolicyClaimLookupDto>();
        }

        var maxPolicies = input.MaxResultCount <= 0 ? 20 : Math.Min(input.MaxResultCount, 100);

        var policyQuery = await PolicyRepository.GetQueryableAsync();

        // Một query duy nhất: policy phải thỏa TẤT CẢ tiêu chí được nhập (AND)
        var filteredQuery = policyQuery.Where(p => !p.IsDeleted && p.Status == PolicyStatus.Active);

        filteredQuery = filteredQuery.Where(p =>
        p.PolicyVersions.Any(v =>
                    !v.IsDeleted
                    && v.EffectDate <= incidentDate
                    && v.ExpireDate >= incidentDate));

        if (!string.IsNullOrWhiteSpace(certificateNo))
        {
            filteredQuery = filteredQuery.Where(p =>
                p.PolicyVersions.Any(v =>
                    !v.IsDeleted
                    && ((v.EffectDate <= incidentDate && v.ExpireDate >= incidentDate))
                    && v.PolicyCertificates.Any(c =>
                        !c.IsDeleted
                        && c.CertificateNo != null
                        && EF.Functions.ILike(c.CertificateNo, $"%{certificateNo}%"))));
        }

        // Thông tin xe: (Biển số OR Số khung OR Số máy) - gộp trong một khối OR
        if (!string.IsNullOrWhiteSpace(carPlate) ||
            !string.IsNullOrWhiteSpace(vin) ||
            !string.IsNullOrWhiteSpace(engineNumber))
        {
            filteredQuery = filteredQuery.Where(p =>
                p.PolicyVersions.Any(v =>
                    !v.IsDeleted
                    && v.EffectDate <= incidentDate
                    && v.ExpireDate >= incidentDate
                    && v.PolicyRiskObjects.Any(ro =>
                        !ro.IsDeleted
                        && ro.PolicyRiskMotors.Any(rm =>
                            !rm.IsDeleted
                            && (
                                (!string.IsNullOrWhiteSpace(carPlate) &&
                                    (rm.CarPlate == carPlate || rm.CarPlateClear == carPlate))
                                || (!string.IsNullOrWhiteSpace(vin) && rm.CarVin == vin)
                                || (!string.IsNullOrWhiteSpace(engineNumber) &&
                                    rm.CarEngineNumber == engineNumber)
                            )))));
        }

        // Lấy danh sách Id thỏa điều kiện (1 round-trip, EF sinh EXISTS)
        var matchingIds = await AsyncExecuter.ToListAsync(
            filteredQuery
                .OrderByDescending(p => p.OrgEffectDate)
                .Take(maxPolicies)
                .Select(p => p.Id));

        if (matchingIds.Count == 0)
        {
            return new List<PolicyClaimLookupDto>();
        }

        // 1 query load đủ policy + includes (1 round-trip)
        var policies = await AsyncExecuter.ToListAsync(
            policyQuery
                .Where(p => matchingIds.Contains(p.Id))
                .Include(p => p.PolicyVersions)
                    .ThenInclude(v => v.PolicyCertificates)
                .Include(p => p.PolicyVersions)
                    .ThenInclude(v => v.PolicyProducts)
                .Include(p => p.PolicyVersions)
                    .ThenInclude(v => v.PolicyRiskObjects)
                        .ThenInclude(ro => ro.PolicyRiskMotors)
                .Include(p => p.Contract)
                    .ThenInclude(c => c.Customer)
                .Include(p => p.Contract)
                    .ThenInclude(c => c.Insurer)
                .Include(p => p.Lob));

        return await MapPoliciesToClaimLookupDtosAsync(policies, incidentDate);
    }

        protected virtual async Task<List<PolicyClaimLookupDto>> MapPoliciesToClaimLookupDtosAsync(List<iOne.Policies.Policy> policies, DateTime? incidentDate)
    {
        var dtos = new List<PolicyClaimLookupDto>();

            // Preload product names
            var allProductIds = policies
                .SelectMany(p => p.PolicyVersions.SelectMany(v => v.PolicyProducts.Select(pp => pp.ProductId)))
                .Distinct()
                .ToList();

            var productNameById = new Dictionary<Guid, string>();
            if (allProductIds.Count > 0)
            {
                var productQuery = await ProProductRepository.GetQueryableAsync();
                var products = await AsyncExecuter.ToListAsync(
                    productQuery
                        .Where(p => allProductIds.Contains(p.Id))
                        .Select(p => new { p.Id, p.Name })
                );
                productNameById = products.ToDictionary(p => p.Id, p => p.Name ?? string.Empty);
            }

            // Preload payment status from policy_amount.payment_status by PolicyVersionId
            var versionIds = policies
                .SelectMany(p => p.PolicyVersions.Where(v => !v.IsDeleted))
                .Select(v => v.Id)
                .Distinct()
                .ToList();

            var paymentStatusByVersionId = new Dictionary<Guid, string>();
            if (versionIds.Count > 0)
            {
                var amountQuery = await PolicyAmountRepository.GetQueryableAsync();
                var amounts = await AsyncExecuter.ToListAsync(
                    amountQuery
                        .Where(a => !a.IsDeleted && versionIds.Contains(a.PolicyVersionId))
                        .Select(a => new { a.PolicyVersionId, a.PaymentStatus })
                );

                foreach (var a in amounts)
                {
                    paymentStatusByVersionId[a.PolicyVersionId] = string.IsNullOrWhiteSpace(a.PaymentStatus)
                        ? "new"
                        : a.PaymentStatus!;
                }
            }

        foreach (var policy in policies)
        {
            var versionQuery = policy.PolicyVersions.Where(v => !v.IsDeleted);
            if (incidentDate.HasValue)
            {
                versionQuery = versionQuery.Where(v => v.EffectDate <= incidentDate.Value && v.ExpireDate >= incidentDate.Value);
            }

            var selectedVersion = versionQuery
                .OrderByDescending(v => v.Version)
                .FirstOrDefault();

            if (selectedVersion == null)
            {
                continue;
            }

            var certificate = selectedVersion.PolicyCertificates.FirstOrDefault(c => !c.IsDeleted);
            var certificateNo = certificate?.CertificateNo;
            var certificateUrl = certificate?.Url;

            var policyProducts = selectedVersion.PolicyProducts
                .Where(pp => !pp.IsDeleted)
                .ToList();

            var riskMotor = selectedVersion.PolicyRiskObjects
                .Where(ro => !ro.IsDeleted)
                .SelectMany(ro => ro.PolicyRiskMotors.Where(rm => !rm.IsDeleted))
                .FirstOrDefault();

            var carPlate = riskMotor?.CarPlate;

            var ownerName = policy.Contract?.Customer?.Name ?? string.Empty;
            var lobName = policy.Lob?.Name ?? string.Empty;
            var contractId = policy.Contract?.Code ?? string.Empty;
            var insurerId = policy.Contract?.InsurerId;
            var insurerName = policy.Contract?.Insurer?.Name;

            var policyStatusText = policy.Status.ToString();
            var paymentStatus = paymentStatusByVersionId.TryGetValue(selectedVersion.Id, out var ps)
                ? ps
                : "new";

            var addedProductRow = false;
            foreach (var pp in policyProducts)
            {
                var productName = productNameById.TryGetValue(pp.ProductId, out var name) ? name : string.Empty;
                if (string.IsNullOrWhiteSpace(productName))
                {
                    continue;
                }

                addedProductRow = true;
                dtos.Add(new PolicyClaimLookupDto
                {
                    PolicyId = policy.Id,
                    LobName = lobName,
                    ContractId = contractId,
                    CertificateNo = certificateNo,
                    Products = new List<string> { productName },
                    ProductId = pp.ProductId,
                    CarPlate = carPlate,
                    OwnerName = ownerName,
                    EffectDate = selectedVersion.EffectDate,
                    ExpireDate = selectedVersion.ExpireDate,
                    Status = policyStatusText,
                    PaymentStatus = paymentStatus,
                    CertificateUrl = certificateUrl,
                    InsurerId = insurerId,
                    InsurerName = insurerName
                });
            }

            if (!addedProductRow)
            {
                dtos.Add(new PolicyClaimLookupDto
                {
                    PolicyId = policy.Id,
                    LobName = lobName,
                    ContractId = contractId,
                    CertificateNo = certificateNo,
                    Products = new List<string>(),
                    ProductId = null,
                    CarPlate = carPlate,
                    OwnerName = ownerName,
                    EffectDate = selectedVersion.EffectDate,
                    ExpireDate = selectedVersion.ExpireDate,
                    Status = policyStatusText,
                    PaymentStatus = paymentStatus,
                    CertificateUrl = certificateUrl,
                    InsurerId = insurerId,
                    InsurerName = insurerName
                });
            }
        }

        return dtos;
    }

    // Keep same normalization behavior as Claim module (for future exact-match tightening)
    protected virtual string NormalizeCarInfo(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return Regex.Replace(value, @"[^A-Za-z0-9]", "").ToUpperInvariant();
    }
}

