using System;
using System.Collections.Generic;
using System.Linq;using System.Text.RegularExpressions;
using System.Threading.Tasks;
using iOne.Claim.Claims;
using iOne.ClaimFolders;
using iOne.Claims;
using iOne.ClaimIncidentRiskMotors;
using iOne.ClaimIncidents;
using iOne.HrEmployees;
using iOne.Policies;
using iOne.ResPartners;
using iOne.Claim.Permissions;
using iOne.ClaimFolderExposureEstimates;
using iOne.ClaimFolderExposures;
using iOne.ClaimFolderIncidentObjects;
using iOne.ProCoverages;
using iOne.ResFeeItems;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using ClaimEntity = iOne.Claims.Claim;
using iOne.Workflow;

namespace iOne.Claim.Claims;

[Authorize(ClaimPermissions.Default)]
public class ClaimFolderAppService : ApplicationService, IClaimFolderAppService
{
    protected IRepository<ClaimFolder, Guid> ClaimFolderRepository { get; }
    protected IRepository<ClaimEntity, Guid> ClaimRepository { get; }
    protected IRepository<ClaimIncident, Guid> ClaimIncidentRepository { get; }
    protected IRepository<ClaimIncidentRiskMotor, Guid> ClaimIncidentRiskMotorRepository { get; }
    protected IRepository<Policy, Guid> PolicyRepository { get; }
    protected IRepository<PolicyVersion, Guid> PolicyVersionRepository { get; }
    protected IRepository<PolicyCertificate, Guid> PolicyCertificateRepository { get; }
    protected IRepository<PolicyRiskMotor, Guid> PolicyRiskMotorRepository { get; }
    protected IRepository<PolicyRiskObject, Guid> PolicyRiskObjectRepository { get; }
    protected IRepository<HrEmployee, Guid> EmployeeRepository { get; }
    protected IRepository<ResPartner, Guid> PartnerRepository { get; }
    protected IRepository<ClaimFolderExposureEstimate, Guid> ClaimFolderExposureEstimateRepository { get; }
    protected IRepository<ClaimFolderIncidentObject, Guid> ClaimFolderIncidentObjectRepository { get; }
    protected IRepository<ClaimFolderExposure, Guid> ClaimFolderExposureRepository { get; }
    protected IRepository<PolicyProduct, Guid> PolicyProductRepository { get; }
    protected IRepository<PolicyCoverage, Guid> PolicyCoverageRepository { get; }
    protected IRepository<ProCoverage, Guid> ProCoverageRepository { get; }
    protected IRepository<ResFeeItem, Guid> FeeItemRepository { get; }
    private readonly IElsaWorkflowService _elsaWorkflowService;

    public ClaimFolderAppService(
        IRepository<ClaimFolder, Guid> claimFolderRepository,
        IRepository<ClaimEntity, Guid> claimRepository,
        IRepository<ClaimIncident, Guid> claimIncidentRepository,
        IRepository<ClaimIncidentRiskMotor, Guid> claimIncidentRiskMotorRepository,
        IRepository<Policy, Guid> policyRepository,
        IRepository<PolicyVersion, Guid> policyVersionRepository,
        IRepository<PolicyCertificate, Guid> policyCertificateRepository,
        IRepository<PolicyRiskMotor, Guid> policyRiskMotorRepository,
        IRepository<PolicyRiskObject, Guid> policyRiskObjectRepository,
        IRepository<HrEmployee, Guid> employeeRepository,
        IRepository<ResPartner, Guid> partnerRepository,
        IRepository<ClaimFolderExposureEstimate, Guid> claimFolderExposureEstimateRepository,
        IRepository<ClaimFolderIncidentObject, Guid> claimFolderIncidentObjectRepository,
        IRepository<ClaimFolderExposure, Guid> claimFolderExposureRepository,
        IRepository<PolicyProduct, Guid> policyProductRepository,
        IRepository<PolicyCoverage, Guid> policyCoverageRepository,
        IRepository<ProCoverage, Guid> proCoverageRepository,
        IRepository<ResFeeItem, Guid> feeItemRepository,
        IElsaWorkflowService elsaWorkflowService)
    {
        ClaimFolderRepository = claimFolderRepository;
        ClaimRepository = claimRepository;
        ClaimIncidentRepository = claimIncidentRepository;
        ClaimIncidentRiskMotorRepository = claimIncidentRiskMotorRepository;
        PolicyRepository = policyRepository;
        PolicyVersionRepository = policyVersionRepository;
        PolicyCertificateRepository = policyCertificateRepository;
        PolicyRiskMotorRepository = policyRiskMotorRepository;
        PolicyRiskObjectRepository = policyRiskObjectRepository;
        EmployeeRepository = employeeRepository;
        PartnerRepository = partnerRepository;
        ClaimFolderExposureEstimateRepository = claimFolderExposureEstimateRepository;
        ClaimFolderIncidentObjectRepository = claimFolderIncidentObjectRepository;
        ClaimFolderExposureRepository = claimFolderExposureRepository;
        PolicyProductRepository = policyProductRepository;
        PolicyCoverageRepository = policyCoverageRepository;
        ProCoverageRepository = proCoverageRepository;
        FeeItemRepository = feeItemRepository;
        _elsaWorkflowService = elsaWorkflowService;
    }

    public virtual async Task<PagedResultDto<ClaimFolderListDto>> GetListAsync(GetClaimFoldersInput input)
    {
        var query = await ClaimFolderRepository.GetQueryableAsync();

        query = query
            .Include(f => f.Claim)
            .Include(f => f.Insurer)
            .Include(f => f.OpenEmployee)
            .Where(f => f.ClaimId == input.ClaimId);

        var totalCount = await AsyncExecuter.CountAsync(query);

        var sorting = string.IsNullOrWhiteSpace(input.Sorting) ? "OpenDate desc" : input.Sorting;
        query = sorting.ToLowerInvariant() switch
        {
            "folderno" => query.OrderBy(x => x.FolderNo),
            "opendate" => query.OrderBy(x => x.OpenDate),
            _ => query.OrderByDescending(x => x.OpenDate)
        };

        query = query.Skip(input.SkipCount).Take(input.MaxResultCount > 0 ? input.MaxResultCount : 10);

        var items = await AsyncExecuter.ToListAsync(query);

        var dtos = items.Select(f => new ClaimFolderListDto
        {
            Id = f.Id,
            FolderNo = f.FolderNo,
            FolderName = f.FolderName,
            InsurerName = f.Insurer?.Name,
            CustomerName = f.Claim?.ContactName,
            CarPlate = null, // sẽ bổ sung sau nếu cần join claim incident risk motor
            PolicyNo = f.PolicyNo,
            AdjustorName = f.OpenEmployee?.FullName,
            Priority = f.Priority,
            ClaimAmount = null,
            Status = f.Status,
            OpenDate = f.OpenDate
        }).ToList();

        return new PagedResultDto<ClaimFolderListDto>(totalCount, dtos);
    }

    public virtual async Task<ClaimFolderDto> GetAsync(Guid id)
    {
        var entity = await ClaimFolderRepository.GetAsync(id);
        var dto = ObjectMapper.Map<ClaimFolder, ClaimFolderDto>(entity);

        if (entity.InsurerId.HasValue)
        {
            var insurer = await PartnerRepository.FirstOrDefaultAsync(x => x.Id == entity.InsurerId.Value);
            dto.InsurerName = insurer?.Name;
        }

        if (entity.OpenEmployeeId.HasValue)
        {
            var emp = await EmployeeRepository.FirstOrDefaultAsync(x => x.Id == entity.OpenEmployeeId.Value);
            dto.OpenEmployeeName = emp?.FullName;
        }

        return dto;
    }

    public virtual async Task<ClaimFolderDto> CreateAsync(CreateClaimFolderDto input)
    {
        var claim = await ClaimRepository.FirstOrDefaultAsync(c => c.Id == input.ClaimId);
        if (claim == null)
        {
            throw new UserFriendlyException(L["Claim:ClaimNotFound"]);
        }

        // Đối tượng tổn thất (claim_incident): IncidentId trên entity = Id claim
        var claimIncident = await ClaimIncidentRepository.FirstOrDefaultAsync(ci => ci.IncidentId == claim.Id);
        ClaimIncidentRiskMotor? riskMotor = null;
        if (claimIncident != null)
        {
            riskMotor = await ClaimIncidentRiskMotorRepository.FirstOrDefaultAsync(rm => rm.IncidentObjectId == claimIncident.Id);
        }

        var incidentDate = claimIncident?.IncidentDate ?? claim.NotifyDate;
        var matchingPolicies = await FindMatchingPoliciesAsync(
            claim.CertificateNo,
            NormalizeCarInfo(riskMotor?.CarPlate),
            NormalizeCarInfo(riskMotor?.Vin),
            NormalizeCarInfo(riskMotor?.EngineNumber),
            incidentDate);
        var firstPolicy = matchingPolicies.FirstOrDefault();
        var policyForProduct = firstPolicy;
        if (input.ProductId.HasValue && matchingPolicies.Count > 0)
        {
            policyForProduct = FindPolicyContainingProduct(matchingPolicies, input.ProductId.Value, incidentDate)
                ?? firstPolicy;
        }

        var resolvedIncidentId = input.IncidentId ?? claimIncident?.Id;
        var resolvedProductId = input.ProductId ?? GetFirstProductIdForIncidentDate(firstPolicy, incidentDate);
        var resolvedPolicyNo = input.PolicyNo ?? policyForProduct?.PolicyNo;
        var resolvedInsurerPolicyNo = input.InsurerPolicyNo ?? policyForProduct?.InsurerPolicyNo;

        // Lấy ClaimTypeId ưu tiên từ input, nếu không có thì lấy từ Claim
        var claimTypeId = input.ClaimTypeId ?? claim.ClaimTypeId;
        if (!claimTypeId.HasValue)
        {
            throw new UserFriendlyException(L["Claim:ClaimTypeRequired"]);
        }

        var id = GuidGenerator.Create();
        var status = ClaimFolderStatus.New;
        var stage = "init";

        var hasAdjustLocation = string.IsNullOrWhiteSpace(input.HasAdjustLocation) ? "N" : input.HasAdjustLocation!;
        TimeSpan ts = DateTime.Now.TimeOfDay;
        var tick = ts.Ticks;
        var entity = new ClaimFolder(
            id,
            folderNo: $"CLF-{claim.Code}-{tick}",
            claimId: input.ClaimId,
            claimTypeId: claimTypeId.Value,
            openDate: Clock.Now,
            status: status,
            stage: stage,
            insurerId: input.InsurerId,
            incidentId: resolvedIncidentId,
            productId: resolvedProductId,
            folderName: input.FolderName,
            policyNo: resolvedPolicyNo,
            insurerPolicyNo: resolvedInsurerPolicyNo,
            description: input.Description,
            openEmployeeId: claim.OpenEmployeeId,
            priority: input.Priority,
            hasAdjustLocation: hasAdjustLocation);

        await ClaimFolderRepository.InsertAsync(entity, autoSave: true);

        var selectedObjectTypeIds = input.IncidentObjectIds?
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList() ?? new List<Guid>();
        var useSelectedObjectTypes = selectedObjectTypeIds.Count > 0;

        if (useSelectedObjectTypes)
        {
            if (policyForProduct == null || !resolvedProductId.HasValue)
            {
                throw new UserFriendlyException(L["Claim:PolicyRequiredForFolderObjects"]);
            }

            var policyVersionId = await GetActivePolicyVersionIdAsync(policyForProduct.Id, incidentDate);
            if (!policyVersionId.HasValue || policyVersionId.Value == Guid.Empty)
            {
                throw new UserFriendlyException(L["Claim:PolicyVersionNotFoundForFolder"]);
            }

            var policyProductId = await GetPolicyProductIdAsync(policyVersionId.Value, resolvedProductId.Value);
            if (!policyProductId.HasValue || policyProductId.Value == Guid.Empty)
            {
                throw new UserFriendlyException(L["Claim:PolicyProductNotFoundForFolder"]);
            }

            var allowedObjectTypeIds = await GetObjectTypeIdsForPolicyProductAsync(policyProductId.Value);
            foreach (var objectTypeId in selectedObjectTypeIds)
            {
                if (!allowedObjectTypeIds.Contains(objectTypeId))
                {
                    throw new UserFriendlyException(L["Claim:InvalidObjectTypeForFolder"]);
                }
            }

            foreach (var objectTypeId in selectedObjectTypeIds)
            {
                var incidentObject = CreateClaimFolderIncidentObject(
                    entity.Id,
                    objectTypeId,
                    claimIncident,
                    riskMotor);

                await ClaimFolderIncidentObjectRepository.InsertAsync(incidentObject, autoSave: true);

                var coverageCode = await ResolveCoverageCodeForObjectTypeAsync(policyProductId.Value, objectTypeId);
                if (string.IsNullOrWhiteSpace(coverageCode))
                {
                    throw new UserFriendlyException(L["Claim:NoCoverageForObjectType"]);
                }

                var exposure = new ClaimFolderExposure(
                    GuidGenerator.Create(),
                    entity.Id,
                    incidentObject.Id,
                    coverageCode);
                exposure.UpdateCoverageParentCode(coverageCode);
                exposure.UpdateInsurerCoverageCode(coverageCode);
                await ClaimFolderExposureRepository.InsertAsync(exposure, autoSave: true);
            }
        }
        else if (claimIncident != null)
        {
            var driverIdNo = NormalizeClaimFolderIncidentObjectIdNo(riskMotor?.DriverIdNo);
            var incidentObject = riskMotor != null
                ? new ClaimFolderIncidentObject(
                    GuidGenerator.Create(),
                    entity.Id,
                    claimIncident.ObjectTypeId,
                    carPlate: riskMotor.CarPlate,
                    carEngineNumber: riskMotor.EngineNumber,
                    carVin: riskMotor.Vin,
                    name: riskMotor.DriverName,
                    idNo: driverIdNo)
                : new ClaimFolderIncidentObject(
                    GuidGenerator.Create(),
                    entity.Id,
                    claimIncident.ObjectTypeId);

            await ClaimFolderIncidentObjectRepository.InsertAsync(incidentObject, autoSave: true);
        }

        // Nếu có ước bồi thường ban đầu thì khởi tạo bản ghi ClaimFolderExposureEstimate
        if (input.EstimateAmount.HasValue && input.EstimateAmount.Value > 0)
        {
            // Tạm thời chọn bất kỳ FeeItem đang có trong hệ thống làm khoản phí cho ước bồi thường.
            // Nếu sau này có quy tắc riêng (ví dụ cấu hình mã khoản phí), có thể thay đổi logic chọn FeeItemId tại đây.
            var defaultFeeItem = await FeeItemRepository.FirstOrDefaultAsync();
            if (defaultFeeItem != null)
            {
                var exposureEstimate = new ClaimFolderExposureEstimate(
                    GuidGenerator.Create(),
                    defaultFeeItem.Id,
                    input.EstimateAmount.Value,
                    ClaimFolderExposureEstimateStatus.Active,
                    entity.Id);

                await ClaimFolderExposureEstimateRepository.InsertAsync(exposureEstimate, autoSave: true);
            }
        }

        // Chỉ khởi tạo workflow với trường hợp ProcessClaimType != Insurer
        if (claim.ProcessClaimType != ProcessClaimType.Insurer && !string.IsNullOrEmpty(input.AssigneeId))
        {
            await _elsaWorkflowService.InitCreateClaimFolderWorkflowAsync(
                entity.Id,
                input.AssigneeId,
                input.AssigneeOrganizationId,
                input.AssessmentStartDate);
        }

        return await GetAsync(entity.Id);
    }

    /// <summary>
    /// Chọn policy version active theo ngày tổn thất (khớp API object-types-by-policy-product).
    /// </summary>
    protected virtual async Task<Guid?> GetActivePolicyVersionIdAsync(Guid policyId, DateTime incidentDate)
    {
        var pvQuery = await PolicyVersionRepository.GetQueryableAsync();
        var versionQuery = pvQuery.Where(pv => pv.PolicyId == policyId && !pv.IsDeleted);
        versionQuery = versionQuery.Where(v =>
            v.Status == "active"
            && v.EffectDate <= incidentDate
            && v.ExpireDate >= incidentDate);

        return await AsyncExecuter.FirstOrDefaultAsync(
            versionQuery.OrderByDescending(v => v.Version).Select(v => v.Id));
    }

    protected virtual async Task<Guid?> GetPolicyProductIdAsync(Guid policyVersionId, Guid productId)
    {
        var ppQuery = await PolicyProductRepository.GetQueryableAsync();
        return await AsyncExecuter.FirstOrDefaultAsync(
            ppQuery
                .Where(pp =>
                    pp.PolicyVersionId == policyVersionId && pp.ProductId == productId && !pp.IsDeleted)
                .Select(pp => pp.Id));
    }

    /// <summary>
    /// Object type ids có coverage trên policy product (khớp API object-types-by-policy-product).
    /// </summary>
    protected virtual async Task<HashSet<Guid>> GetObjectTypeIdsForPolicyProductAsync(Guid policyProductId)
    {
        var pcQuery = await PolicyCoverageRepository.GetQueryableAsync();
        var coverageIds = await AsyncExecuter.ToListAsync(
            pcQuery
                .Where(pc => pc.PolicyProductId == policyProductId && !pc.IsDeleted)
                .Select(pc => pc.CoverageId));

        if (coverageIds.Count == 0)
        {
            return new HashSet<Guid>();
        }

        var covQuery = await ProCoverageRepository.GetQueryableAsync();
        var objectTypeIds = await AsyncExecuter.ToListAsync(
            covQuery
                .Where(c =>
                    coverageIds.Contains(c.Id)
                    && c.ObjectTypeId != null
                    && c.ObjectTypeId != Guid.Empty)
                .Select(c => c.ObjectTypeId!.Value));

        return objectTypeIds.ToHashSet();
    }

    /// <summary>
    /// Nhiều coverage cùng object type: chọn mã coverage (ProCoverage.Code) nhỏ nhất theo thứ tự từ điển.
    /// </summary>
    protected virtual async Task<string?> ResolveCoverageCodeForObjectTypeAsync(Guid policyProductId, Guid objectTypeId)
    {
        var pcQuery = await PolicyCoverageRepository.GetQueryableAsync();
        var covQuery = await ProCoverageRepository.GetQueryableAsync();

        return await AsyncExecuter.FirstOrDefaultAsync(
            from pc in pcQuery
            join c in covQuery on pc.CoverageId equals c.Id
            where pc.PolicyProductId == policyProductId
                && !pc.IsDeleted
                && c.ObjectTypeId == objectTypeId
            orderby c.Code
            select c.Code);
    }

    protected virtual ClaimFolderIncidentObject CreateClaimFolderIncidentObject(
        Guid claimFolderId,
        Guid objectTypeId,
        ClaimIncident? claimIncident,
        ClaimIncidentRiskMotor? riskMotor)
    {
        if (claimIncident != null
            && claimIncident.ObjectTypeId == objectTypeId
            && riskMotor != null)
        {
            var driverIdNo = NormalizeClaimFolderIncidentObjectIdNo(riskMotor.DriverIdNo);
            return new ClaimFolderIncidentObject(
                GuidGenerator.Create(),
                claimFolderId,
                objectTypeId,
                carPlate: riskMotor.CarPlate,
                carEngineNumber: riskMotor.EngineNumber,
                carVin: riskMotor.Vin,
                name: riskMotor.DriverName,
                idNo: driverIdNo);
        }

        if (claimIncident != null && claimIncident.ObjectTypeId == objectTypeId)
        {
            return new ClaimFolderIncidentObject(
                GuidGenerator.Create(),
                claimFolderId,
                objectTypeId);
        }

        return new ClaimFolderIncidentObject(
            GuidGenerator.Create(),
            claimFolderId,
            objectTypeId);
    }

    /// <summary>
    /// Khớp đơn bảo hiểm theo GCN / thông tin xe và ngày tổn thất (cùng quy tắc khi tạo claim).
    /// </summary>
    protected virtual async Task<List<Policy>> FindMatchingPoliciesAsync(
        string? certificateNo,
        string normalizedCarPlate,
        string normalizedVin,
        string normalizedEngineNumber,
        DateTime incidentDate)
    {
        var policyQuery = await PolicyRepository.GetQueryableAsync();
        var versionQuery = await PolicyVersionRepository.GetQueryableAsync();
        var certificateQuery = await PolicyCertificateRepository.GetQueryableAsync();
        var riskMotorQuery = await PolicyRiskMotorRepository.GetQueryableAsync();

        var matchingPolicyIds = new HashSet<Guid>();

        if (!string.IsNullOrWhiteSpace(certificateNo))
        {
            var certificatePolicyIds = await AsyncExecuter.ToListAsync(
                from p in policyQuery
                join v in versionQuery on p.Id equals v.PolicyId
                join c in certificateQuery on v.Id equals c.PolicyVersionId
                where c.CertificateNo == certificateNo
                    && v.Status == "active"
                    && v.EffectDate <= incidentDate
                    && v.ExpireDate >= incidentDate
                select p.Id
            );
            foreach (var id in certificatePolicyIds)
            {
                matchingPolicyIds.Add(id);
            }
        }

        if (!string.IsNullOrWhiteSpace(normalizedCarPlate) ||
            !string.IsNullOrWhiteSpace(normalizedVin) ||
            !string.IsNullOrWhiteSpace(normalizedEngineNumber))
        {
            var allRiskMotors = await AsyncExecuter.ToListAsync(
                from rm in riskMotorQuery
                where rm.PolicyRiskObjectId != null
                select rm
            );

            var allRiskObjects = await AsyncExecuter.ToListAsync(
                from ro in await PolicyRiskObjectRepository.GetQueryableAsync()
                select ro
            );

            var riskObjectDict = allRiskObjects.ToDictionary(ro => ro.Id, ro => ro);

            foreach (var riskMotor in allRiskMotors)
            {
                if (!riskMotor.PolicyRiskObjectId.HasValue) continue;
                if (!riskObjectDict.TryGetValue(riskMotor.PolicyRiskObjectId.Value, out var riskObject)) continue;

                var version = await PolicyVersionRepository.FirstOrDefaultAsync(v => v.Id == riskObject.PolicyVersionId);
                if (version == null) continue;
                if (version.Status != "active") continue;
                if (version.EffectDate > incidentDate || version.ExpireDate < incidentDate) continue;

                var matches = false;

                if (!string.IsNullOrWhiteSpace(normalizedCarPlate))
                {
                    var normalizedValue = NormalizeCarInfo(riskMotor.CarPlate ?? "");
                    if (normalizedValue == normalizedCarPlate)
                    {
                        matches = true;
                    }
                }

                if (!matches && !string.IsNullOrWhiteSpace(normalizedVin))
                {
                    var normalizedValue = NormalizeCarInfo(riskMotor.CarVin ?? "");
                    if (normalizedValue == normalizedVin)
                    {
                        matches = true;
                    }
                }

                if (!matches && !string.IsNullOrWhiteSpace(normalizedEngineNumber))
                {
                    var normalizedValue = NormalizeCarInfo(riskMotor.CarEngineNumber ?? "");
                    if (normalizedValue == normalizedEngineNumber)
                    {
                        matches = true;
                    }
                }

                if (matches)
                {
                    matchingPolicyIds.Add(riskObject.PolicyId);
                }
            }
        }

        if (matchingPolicyIds.Count == 0)
        {
            return new List<Policy>();
        }

        var policies = await AsyncExecuter.ToListAsync(
            policyQuery
                .Where(p => matchingPolicyIds.Contains(p.Id))
                .Include(p => p.PolicyVersions)
                    .ThenInclude(v => v.PolicyCertificates)
                .Include(p => p.PolicyVersions)
                    .ThenInclude(v => v.PolicyProducts)
                .Include(p => p.PolicyVersions)
                    .ThenInclude(v => v.PolicyRiskObjects)
                        .ThenInclude(ro => ro.PolicyRiskMotors)
                .Include(p => p.PolicyVersions)
                    .ThenInclude(v => v.PolicyRiskObjects)
                        .ThenInclude(ro => ro.ObjectType)
                .Include(p => p.Contract)
                    .ThenInclude(c => c!.Customer)
                .Include(p => p.Contract)
                    .ThenInclude(c => c!.Insurer)
                .Include(p => p.Lob));

        return policies;
    }

    protected virtual Policy? FindPolicyContainingProduct(
        List<Policy> policies,
        Guid productId,
        DateTime incidentDate)
    {
        foreach (var policy in policies)
        {
            if (policy.PolicyVersions == null || policy.PolicyVersions.Count == 0)
            {
                continue;
            }

            var version = policy.PolicyVersions
                .Where(v =>
                    v.Status == "active"
                    && v.EffectDate <= incidentDate
                    && v.ExpireDate >= incidentDate
                    && !v.IsDeleted)
                .OrderByDescending(v => v.Version)
                .FirstOrDefault();

            if (version?.PolicyProducts == null)
            {
                continue;
            }

            if (version.PolicyProducts.Any(pp => !pp.IsDeleted && pp.ProductId == productId))
            {
                return policy;
            }
        }

        return null;
    }

    protected virtual Guid? GetFirstProductIdForIncidentDate(Policy? policy, DateTime incidentDate)
    {
        if (policy?.PolicyVersions == null || policy.PolicyVersions.Count == 0)
        {
            return null;
        }

        var version = policy.PolicyVersions
            .Where(v =>
                v.Status == "active"
                && v.EffectDate <= incidentDate
                && v.ExpireDate >= incidentDate
                && !v.IsDeleted)
            .OrderByDescending(v => v.Version)
            .FirstOrDefault();

        if (version?.PolicyProducts == null)
        {
            return null;
        }

        var product = version.PolicyProducts.FirstOrDefault(pp => !pp.IsDeleted);
        return product?.ProductId;
    }

    protected virtual string NormalizeCarInfo(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return Regex.Replace(value, @"[^A-Za-z0-9]", "").ToUpperInvariant();
    }

    /// <summary>
    /// ClaimFolderIncidentObject.IdNo tối đa 15 ký tự; ClaimIncidentRiskMotor.DriverIdNo có thể dài hơn.
    /// </summary>
    private static string? NormalizeClaimFolderIncidentObjectIdNo(string? driverIdNo)
    {
        if (string.IsNullOrWhiteSpace(driverIdNo))
        {
            return null;
        }

        var trimmed = driverIdNo.Trim();
        return trimmed.Length <= 15 ? trimmed : trimmed[..15];
    }
}

