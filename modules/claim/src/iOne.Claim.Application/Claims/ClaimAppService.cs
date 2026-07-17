using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using iOne.Claim.Claims;
using iOne.Claim.Localization;
using iOne.ClaimIncidentRiskMotors;
using iOne.ClaimIncidents;
using ClaimEntity = iOne.Claims.Claim; // Alias to avoid namespace conflict
using iOne.Claims;
using iOne.ClaimStages;
using iOne.HrDepartments;
using iOne.HrEmployees;
using iOne.Policies;
using iOne.ProLineOfBusinesses;
using iOne.ProProducts;
using iOne.ResObjectTypes;
using iOne.ResClaimStages;
using iOne.ResClaimTypes;
using iOne.ResPartners;
using iOne.ResPartnerTypes;
using iOne.ResProvinces;
using iOne.ResReasonGroups;
using iOne.ResReasons;
using iOne.ResSequences;
using iOne.ResWards;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using iOne.AdminConfigs;
using Volo.Abp.Users;
using Volo.Abp.Domain.Entities;
using iOne.Workflow;
using iOne.WorkTasks;
using Microsoft.Extensions.Configuration;
using iOne.ClaimFolders;
using iOne.ClaimFolderExposureEstimates;

namespace iOne.Claim.Claims;

public class ClaimAppService : ApplicationService, IClaimAppService
{
    protected IRepository<ClaimEntity, Guid> ClaimRepository { get; }
    protected IRepository<ClaimIncident, Guid> ClaimIncidentRepository { get; }
    protected IRepository<ClaimIncidentRiskMotor, Guid> ClaimIncidentRiskMotorRepository { get; }
    protected IRepository<ProLineOfBusiness, Guid> LobRepository { get; }
    protected IRepository<ResPartner, Guid> PartnerRepository { get; }
    protected IRepository<ResPartnerType, Guid> PartnerTypeRepository { get; }
    protected IRepository<HrEmployee, Guid> EmployeeRepository { get; }
    protected IRepository<HrDepartment, Guid> DepartmentRepository { get; }
    protected IRepository<Policy, Guid> PolicyRepository { get; }
    protected IRepository<PolicyVersion, Guid> PolicyVersionRepository { get; }
    protected IRepository<PolicyCertificate, Guid> PolicyCertificateRepository { get; }
    protected IRepository<PolicyRiskMotor, Guid> PolicyRiskMotorRepository { get; }
    protected IRepository<PolicyRiskObject, Guid> PolicyRiskObjectRepository { get; }
    protected IRepository<ResReason, Guid> ResReasonRepository { get; }
    protected IRepository<ResReasonGroup, Guid> ResReasonGroupRepository { get; }
    protected IRepository<ResProvince, Guid> ResProvinceRepository { get; }
    protected IRepository<ResWard, Guid> ResWardRepository { get; }
    protected IRepository<ResObjectType, Guid> ResObjectTypeRepository { get; }
    protected IRepository<AdminConfig, Guid> ResAdminConfigRepository { get; }
    protected IResSequenceRepository ResSequenceRepository { get; }
    protected ClaimManager ClaimManager { get; }
    protected ICurrentUser CurrentUser { get; }
    private readonly IElsaWorkflowService _elsaWorkflowService;
    protected IRepository<ClaimStage, Guid> ClaimStageRepository { get; }
    protected IRepository<ResClaimStage, Guid> ResClaimStageRepository { get; }
    protected IResClaimTypeRepository ResClaimTypeRepository { get; }
    protected IConfiguration Configuration { get; }
    protected IRepository<ClaimFolder, Guid> ClaimFolderRepository { get; }
    protected IRepository<ClaimFolderExposureEstimate, Guid> ClaimFolderExposureEstimateRepository { get; }
    protected IRepository<WorkTask, Guid> WorkTaskRepository { get; }

    public ClaimAppService(
        IRepository<ClaimEntity, Guid> claimRepository,
        IRepository<ClaimIncident, Guid> claimIncidentRepository,
        IRepository<ClaimIncidentRiskMotor, Guid> claimIncidentRiskMotorRepository,
        IRepository<ProLineOfBusiness, Guid> lobRepository,
        IRepository<ResPartner, Guid> partnerRepository,
        IRepository<ResPartnerType, Guid> partnerTypeRepository,
        IRepository<HrEmployee, Guid> employeeRepository,
        IRepository<HrDepartment, Guid> departmentRepository,
        IRepository<Policy, Guid> policyRepository,
        IRepository<PolicyVersion, Guid> policyVersionRepository,
        IRepository<PolicyCertificate, Guid> policyCertificateRepository,
        IRepository<PolicyRiskMotor, Guid> policyRiskMotorRepository,
        IRepository<PolicyRiskObject, Guid> policyRiskObjectRepository,
        IRepository<ResReason, Guid> resReasonRepository,
        IRepository<ResReasonGroup, Guid> resReasonGroupRepository,
        IRepository<ResProvince, Guid> resProvinceRepository,
        IRepository<ResWard, Guid> resWardRepository,
        IRepository<ResObjectType, Guid> resObjectTypeRepository,
        IRepository<AdminConfig, Guid> resAdminConfigRepository,
        IResSequenceRepository resSequenceRepository,
        ClaimManager claimManager,
        ICurrentUser currentUser,
        IElsaWorkflowService elsaWorkflowService,
        IRepository<ClaimStage, Guid> claimStageRepository,
        IRepository<ResClaimStage, Guid> resClaimStageRepository,
        IResClaimTypeRepository resClaimTypeRepository,
        IConfiguration configuration,
        IRepository<ClaimFolder, Guid> claimFolderRepository,
        IRepository<ClaimFolderExposureEstimate, Guid> claimFolderExposureEstimateRepository,
        IRepository<WorkTask, Guid> workTaskRepository)
    {
        ClaimRepository = claimRepository;
        ClaimIncidentRepository = claimIncidentRepository;
        ClaimIncidentRiskMotorRepository = claimIncidentRiskMotorRepository;
        LobRepository = lobRepository;
        PartnerRepository = partnerRepository;
        PartnerTypeRepository = partnerTypeRepository;
        EmployeeRepository = employeeRepository;
        DepartmentRepository = departmentRepository;
        PolicyRepository = policyRepository;
        PolicyVersionRepository = policyVersionRepository;
        PolicyCertificateRepository = policyCertificateRepository;
        PolicyRiskMotorRepository = policyRiskMotorRepository;
        PolicyRiskObjectRepository = policyRiskObjectRepository;
        ResReasonRepository = resReasonRepository;
        ResReasonGroupRepository = resReasonGroupRepository;
        ResProvinceRepository = resProvinceRepository;
        ResWardRepository = resWardRepository;
        ResObjectTypeRepository = resObjectTypeRepository;
        ResAdminConfigRepository = resAdminConfigRepository;
        ResSequenceRepository = resSequenceRepository;
        ClaimManager = claimManager;
        CurrentUser = currentUser;
        _elsaWorkflowService = elsaWorkflowService;
        ClaimStageRepository = claimStageRepository;
        ResClaimStageRepository = resClaimStageRepository;
        ResClaimTypeRepository = resClaimTypeRepository;
        Configuration = configuration;
        ClaimFolderRepository = claimFolderRepository;
        ClaimFolderExposureEstimateRepository = claimFolderExposureEstimateRepository;
        WorkTaskRepository = workTaskRepository;
        LocalizationResource = typeof(ClaimResource);
    }

    public virtual async Task<PagedResultDto<ClaimDto>> GetListAsync(GetClaimsInput input)
    {
        var query = await CreateFilteredQueryAsync(input);

        // Get total count before pagination
        var totalCount = await AsyncExecuter.CountAsync(query);

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(input.Sorting))
        {
            query = ApplySorting(query, input.Sorting);
        }
        else
        {
            // Default sorting: open_date desc
            query = query.OrderByDescending(x => x.OpenDate);
        }

        // Apply paging
        query = ApplyPaging(query, input);

        // Execute query
        var entities = await AsyncExecuter.ToListAsync(query);

        // Map to DTOs
        var dtos = ObjectMapper.Map<List<ClaimEntity>, List<ClaimDto>>(entities);

        // Set navigation property names and CarPlate
        // Load all ClaimIncidents and ClaimIncidentRiskMotors at once to avoid N+1 queries
        var claimIds = entities.Select(x => x.Id).ToList();
        var claimIncidents = await AsyncExecuter.ToListAsync(
            (await ClaimIncidentRepository.GetQueryableAsync())
            .Where(x => claimIds.Contains(x.IncidentId))
        );
        var incidentIds = claimIncidents.Select(x => x.Id).ToList();
        var riskMotors = await AsyncExecuter.ToListAsync(
            (await ClaimIncidentRiskMotorRepository.GetQueryableAsync())
            .Where(x => incidentIds.Contains(x.IncidentObjectId ?? Guid.Empty))
        );
        var riskMotorDict = riskMotors.ToDictionary(x => x.IncidentObjectId ?? Guid.Empty, x => x);
        var incidentDict = claimIncidents.ToDictionary(x => x.IncidentId, x => x);

        for (int i = 0; i < entities.Count; i++)
        {
            var entity = entities[i];
            var dto = dtos[i];

            // Set LobName
            if (entity.Lob != null)
            {
                dto.LobName = entity.Lob.Name;
            }

            // Set InsurerName
            if (entity.InsurerId.HasValue && entity.Insurer != null)
            {
                dto.InsurerName = entity.Insurer.Name;
                dto.InsurerCode = entity.Insurer.Code;
            }

            // Set OpenEmployeeName
            if (entity.OpenEmployee != null)
            {
                dto.OpenEmployeeName = entity.OpenEmployee.FullName;
            }

            // Set CarPlate from ClaimIncidentRiskMotor
            if (incidentDict.TryGetValue(entity.Id, out var claimIncident))
            {
                if (riskMotorDict.TryGetValue(claimIncident.Id, out var riskMotor))
                {
                    dto.CarPlate = riskMotor.CarPlate;
                }
            }
        }

        return new PagedResultDto<ClaimDto>(totalCount, dtos);
    }

    public virtual async Task<ClaimDetailDto> GetAsync(Guid id)
    {
        // Load main claim with navigation properties
        var claimQuery = await ClaimRepository.GetQueryableAsync();
        var claim = await claimQuery
            .Include(x => x.Lob)
            .Include(x => x.Insurer)
            .Include(x => x.OpenEmployee)
            .Include(x => x.ProcessDepartment)
            .Include(x => x.ProcessEmployee)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (claim == null)
        {
            throw new EntityNotFoundException(typeof(ClaimEntity), id);
        }

        // Load incident & risk motor
        var incidentQuery = await ClaimIncidentRepository.GetQueryableAsync();
        var incident = await incidentQuery.FirstOrDefaultAsync(x => x.IncidentId == claim.Id);

        ClaimIncidentRiskMotor? riskMotor = null;
        if (incident != null)
        {
            var riskMotorQuery = await ClaimIncidentRiskMotorRepository.GetQueryableAsync();
            riskMotor = await riskMotorQuery.FirstOrDefaultAsync(x => x.IncidentObjectId == incident.Id);
        }

        var dto = new ClaimDetailDto
        {
            Id = claim.Id,
            Code = claim.Code,
            Status = claim.Status,
            OpenDate = claim.OpenDate,
            NotifyDate = claim.NotifyDate,
            LobId = claim.LobId,
            LobName = claim.Lob?.Name,
            InsurerId = claim.InsurerId,
            InsurerName = claim.Insurer?.Name,
            SnapshotLink = claim.SnapshotLink,

            ProcessClaimType = claim.ProcessClaimType,
            ProcessDeptId = claim.ProcessDeptId,
            ProcessEmpId = claim.ProcessEmpId,
            ProcessDeptName = claim.ProcessDepartment?.Name,
            ProcessEmpName = claim.ProcessEmployee?.FullName,
            ProcessEmpPhone = claim.ProcessEmployee?.Phone,
            OpenEmployeePhone = claim.OpenEmployee?.Phone,
            CertificateNo = claim.CertificateNo,

            NotifierName = claim.NotifierName,
            NotifierPhone = claim.NotifierPhone,
            NotifierEmail = claim.NotifierEmail,
            NotifierInRelationship = claim.NotifierInRelationship,

            ContactName = claim.ContactName,
            ContactPhone = claim.ContactPhone,
            ContactEmail = claim.ContactEmail,
            ContactInRelationship = claim.ContactInRelationship,

            Priority = (int)claim.Priority,

            Note = incident?.Note
        };

        if (incident != null)
        {
            dto.AssessmentPartnerId = incident.AssessmentPartnerId;
            dto.AssessmentDate = incident.AssessmentDate;
            dto.IncidentDate = incident.IncidentDate;
            dto.IncidentProvinceId = incident.IncidentProvinceId ?? Guid.Empty;
            dto.IncidentWardId = incident.IncidentWardId ?? Guid.Empty;
            dto.IncidentAddress = incident.IncidentAddress;
            dto.OnLocation = incident.OnLocation;
            dto.IncidentCauseId = incident.IncidentCauseId;
            dto.IncidentDescription = incident.IncidentDescription;
            dto.IncidentResult = incident.IncidentResult ?? string.Empty;

            var cause = await ResReasonRepository.FindAsync(incident.IncidentCauseId);
            dto.IncidentCauseName = cause?.Name;

            if (incident.AssessmentPartnerId.HasValue)
            {
                var partner = await PartnerRepository.FindAsync(incident.AssessmentPartnerId.Value);
                dto.AssessmentPartnerName = partner?.Name;
            }
        }

        if (riskMotor != null)
        {
            dto.CarPlate = riskMotor.CarPlate;
            dto.Vin = riskMotor.Vin;
            dto.EngineNumber = riskMotor.EngineNumber;
            dto.PersonOnCar = riskMotor.PersonsOnCar;

            dto.DriverName = riskMotor.DriverName;
            dto.DriverPhone = riskMotor.DriverPhone;
            dto.DriverIdNo = riskMotor.DriverIdNo;
            dto.DriverLicenseNo = riskMotor.DriverLicenseNo;
            dto.DriverLicenseLevel = riskMotor.DriverLicenseLevel;
            dto.DriverLicenseEffectDate = riskMotor.DriverLicenseEffectDate;
            dto.DriverLicenseExpireDate = riskMotor.DriverLicenseExpireDate;
            dto.DriverSex = riskMotor.DriverSex;

            dto.DriverRegistryNo = riskMotor.CarRegistryNo;
            dto.DriverRegistryEffectDate = riskMotor.CarRegistryEffectDate;
            dto.DriverRegistryExpireDate = riskMotor.CarRegistryExpireDate;
        }

        dto.TotalEstimatedLossAmount = await GetTotalActiveExposureEstimateForClaimExcludingCancelledFoldersAsync(claim.Id);

        return dto;
    }

    private async Task<decimal> GetTotalActiveExposureEstimateForClaimExcludingCancelledFoldersAsync(Guid claimId)
    {
        var folderQuery = await ClaimFolderRepository.GetQueryableAsync();
        var estimateQuery = await ClaimFolderExposureEstimateRepository.GetQueryableAsync();

        return await AsyncExecuter.SumAsync(
            from est in estimateQuery
            join folder in folderQuery on est.ClaimFolderId equals folder.Id
            where folder.ClaimId == claimId
                && folder.Status != ClaimFolderStatus.Cancelled
                && !folder.IsDeleted
                && est.Status == ClaimFolderExposureEstimateStatus.Active
                && !est.IsDeleted
            select est.Amount);
    }

    public virtual async Task<string> GetSnapshotLinkAsync(Guid id)
    {
        // Lấy claim hiện tại
        var claim = await ClaimRepository.FindAsync(id);
        if (claim == null)
        {
            throw new EntityNotFoundException(typeof(ClaimEntity), id);
        }

        // Nếu đã có snapshot link thì trả luôn
        if (!string.IsNullOrWhiteSpace(claim.SnapshotLink))
        {
            return claim.SnapshotLink;
        }

        // Nếu chưa có thì sinh từ claim Id (mã hóa Id để URL có thể giải mã lại)
        var snapshotCode = EncryptClaimIdToSnapshotCode(claim.Id);
        var snapshotLink = GenerateSnapshotLink(snapshotCode);

        // Cập nhật lại Claim thông qua ClaimManager để đảm bảo domain rule
        await ClaimManager.UpdateAsync(
            claim,
            claim.LobId,
            claim.ProcessClaimType,
            claim.Status,
            claim.OpenDate,
            claim.NotifyDate,
            claim.NotifierName,
            claim.NotifierPhone,
            claim.ContactName,
            claim.ContactPhone,
            claim.Priority,
            claim.OpenEmployeeId,
            claim.ClaimTypeId,
            claim.InsurerId,
            claim.InsurerIncidentCode,
            claim.CloseDate,
            claim.CancelDate,
            claim.CloseEmployeeId,
            claim.CancelEmployeeId,
            claim.CancelReasonId,
            claim.CancelNote,
            claim.NotifierEmail,
            claim.NotifierInRelationship,
            claim.ContactEmail,
            claim.ContactInRelationship,
            snapshotLink,
            claim.ProcessDeptId,
            claim.ProcessEmpId,
            claim.CertificateNo);

        await CurrentUnitOfWork.SaveChangesAsync();

        return snapshotLink;
    }

    public virtual async Task<CreateClaimResponseDto> CreateAsync(CreateClaimDto input)
    {
        // 1. Validation
        await ValidateCreateClaimInputAsync(input);

        // 2. Normalize vehicle info
        var normalizedCarPlate = NormalizeCarInfo(input.CarPlate);
        var normalizedVin = NormalizeCarInfo(input.Vin);
        var normalizedEngineNumber = NormalizeCarInfo(input.EngineNumber);

        // 3. Find policies
        var policies = await FindMatchingPoliciesAsync(
            input.CertificateNo,
            normalizedCarPlate,
            normalizedVin,
            normalizedEngineNumber,
            input.IncidentDate);

        // 4. Determine insurer (from policy or input)
        var insurerId = await DetermineInsurerIdAsync(policies, input.InsurerId);

        // 5. Get current user employee
        var currentEmployee = await GetCurrentEmployeeAsync();

        // 6. Get LOB from policy (or default CAR if no policy)
        var defaultLob = await GetLobFromPoliciesOrDefaultAsync(policies);

        // 7. Generate claim code
        var claimCode = await GenerateClaimCodeAsync();

        // 8. Generate claim Id first, then snapshot link from Id (mã hóa Id để có thể giải mã khi mở URL)
        var claimId = GuidGenerator.Create();
        var snapshotCode = EncryptClaimIdToSnapshotCode(claimId);
        var snapshotLink = GenerateSnapshotLink(snapshotCode);

        // 9. Determine status based on action
        var status = input.Action.ToLowerInvariant() == "assign" 
            ? ClaimStatus.PendingReceive 
            : ClaimStatus.Draft;

        // 10. Set priority (default = 2 = Medium)
        var priority = input.Priority.HasValue && input.Priority.Value >= 1 && input.Priority.Value <= 3
            ? (ClaimPriority)(input.Priority.Value)
            : ClaimPriority.Medium;

        // 11. Get ObjectType from policy risk objects (or default CAR)
        var objectType = await GetObjectTypeFromPoliciesOrDefaultAsync(policies);

        // 12. Create Claim entity (claimId đã sinh ở bước 8)
        var notifyDate = Clock.Now; // Auto from server
        var openDate = Clock.Now;

        // Lấy ClaimType cho xe cơ giới (vehicle_claim)
        var vehicleClaimType = await ResClaimTypeRepository.FirstOrDefaultAsync(x => x.Code == "vehicle_claim");
        if (vehicleClaimType == null)
        {
            throw new UserFriendlyException("Không tìm thấy cấu hình loại yêu cầu bồi thường với code = VEHICLE_CLAIM.");
        }

        var claim = new ClaimEntity(
            claimId,
            defaultLob.Id,
            claimCode,
            input.ProcessClaimType,
            status,
            openDate,
            notifyDate,
            input.NotifierName,
            input.NotifierPhone,
            input.ContactName,
            input.ContactPhone,
            priority,
            currentEmployee.Id,
            claimTypeId: vehicleClaimType.Id,
            insurerId: insurerId,
            notifierEmail: input.NotifierEmail,
            notifierInRelationship: input.NotifierInRelationship,
            contactEmail: input.ContactEmail,
            contactInRelationship: input.ContactInRelationship,
            snapshotLink: snapshotLink,
            processDeptId: input.ProcessDeptId,
            processEmpId: input.ProcessEmpId,
            certificateNo: input.CertificateNo);

        await ClaimManager.CreateAsync(claim);
        await ClaimRepository.InsertAsync(claim);

        // 13. Create ClaimIncident entity
        var claimIncidentId = GuidGenerator.Create();
        var claimIncident = new ClaimIncident(
            claimIncidentId,
            claimId,
            objectType.Id,
            input.OnLocation.ToUpperInvariant(),
            input.IncidentCauseId,
            assessmentPartnerId: input.AssessmentPartnerId,
            assessmentDate: input.AssessmentDate,
            incidentDate: input.IncidentDate,
            incidentProvinceId: input.IncidentProvinceId,
            incidentWardId: input.IncidentWardId,
            incidentAddress: input.IncidentAddress,
            incidentDescription: input.IncidentDescription,
            incidentResult: input.IncidentResult,
            note: input.Note);

        await ClaimIncidentRepository.InsertAsync(claimIncident);

        // 14. Create ClaimIncidentRiskMotor entity
        var riskMotorId = GuidGenerator.Create();
        var riskMotor = new ClaimIncidentRiskMotor(
            riskMotorId,
            claimIncidentId,
            normalizedCarPlate,
            normalizedVin,
            normalizedEngineNumber,
            input.PersonOnCar,
            input.DriverName,
            input.DriverPhone,
            input.DriverIdNo,
            input.DriverLicenseNo,
            input.DriverLicenseLevel,
            input.DriverLicenseEffectDate,
            input.DriverLicenseExpireDate,
            input.DriverSex?.ToUpperInvariant(),
            input.DriverRegistryNo, // Maps to carRegistryNo
            input.DriverRegistryEffectDate, // Maps to carRegistryEffectDate
            input.DriverRegistryExpireDate);

        await ClaimIncidentRiskMotorRepository.InsertAsync(riskMotor);

        // 15. Handle Elsa workflow if action = assign
        if (input.Action.ToLowerInvariant() == "assign")
        {
            // TODO: Implement Elsa workflow call
            // await TriggerElsaWorkflowAsync(claimId);
        }

        // 16. Send notifications (TODO: Implement Zalo/Email notifications)
        // await SendNotificationsAsync(claim, snapshotLink);

        await CurrentUnitOfWork.SaveChangesAsync();

        // 17. Map policies to DTOs
        var policyDtos = await MapPoliciesToDtosAsync(policies);

        return new CreateClaimResponseDto
        {
            ClaimId = claimId,
            Code = claimCode,
            Status = status.ToString(),
            SnapshotLink = snapshotLink,
            Policies = policyDtos
        };
    }

    public virtual async Task<ClaimDetailDto> UpdateAsync(Guid id, UpdateClaimDto input)
    {
        // 1. Load claim and ensure it exists
        var claim = await ClaimRepository.GetAsync(id);

        // 2. Chỉ nháp mới được sửa
        if (claim.Status != ClaimStatus.Draft)
        {
            throw new UserFriendlyException(L["OnlyDraftCanBeEdited"].Value);
        }

        // 3. Validation (same rules as create, except action)
        await ValidateUpdateClaimInputAsync(input);

        // 4. Normalize vehicle info
        var normalizedCarPlate = NormalizeCarInfo(input.CarPlate);
        var normalizedVin = NormalizeCarInfo(input.Vin);
        var normalizedEngineNumber = NormalizeCarInfo(input.EngineNumber);

        // 5. Find policies (for LOB from policy, same as Create)
        var policies = await FindMatchingPoliciesAsync(
            input.CertificateNo,
            normalizedCarPlate,
            normalizedVin,
            normalizedEngineNumber,
            input.IncidentDate);

        // 6. Get current user employee
        var currentEmployee = await GetCurrentEmployeeAsync();

        // 7. Get LOB from policy (or default CAR if no policy)
        var defaultLob = await GetLobFromPoliciesOrDefaultAsync(policies);

        // 8. Priority
        var priority = input.Priority.HasValue && input.Priority.Value >= 1 && input.Priority.Value <= 3
            ? (ClaimPriority)(input.Priority.Value)
            : ClaimPriority.Medium;

        var insurerId = input.InsurerId != Guid.Empty ? input.InsurerId : (Guid?)null;

        // 9. Update Claim via ClaimManager
        await ClaimManager.UpdateAsync(
            claim,
            defaultLob.Id,
            input.ProcessClaimType,
            ClaimStatus.Draft,
            claim.OpenDate,
            claim.NotifyDate,
            input.NotifierName,
            input.NotifierPhone,
            input.ContactName,
            input.ContactPhone,
            priority,
            currentEmployee.Id,
            claim.ClaimTypeId,
            insurerId,
            claim.InsurerIncidentCode,
            claim.CloseDate,
            claim.CancelDate,
            claim.CloseEmployeeId,
            claim.CancelEmployeeId,
            claim.CancelReasonId,
            claim.CancelNote,
            input.NotifierEmail,
            input.NotifierInRelationship,
            input.ContactEmail,
            input.ContactInRelationship,
            claim.SnapshotLink,
            input.ProcessDeptId,
            input.ProcessEmpId,
            input.CertificateNo);

        await ClaimRepository.UpdateAsync(claim);

        // 10. Load incident (IncidentId = claim.Id)
        var incidentQuery = await ClaimIncidentRepository.GetQueryableAsync();
        var incident = await incidentQuery.FirstOrDefaultAsync(x => x.IncidentId == id);
        if (incident != null)
        {
            incident.UpdateOnLocation(input.OnLocation.ToUpperInvariant());
            incident.UpdateIncidentCauseId(input.IncidentCauseId);
            incident.UpdateAssessmentPartnerId(input.AssessmentPartnerId);
            incident.UpdateAssessmentDate(input.AssessmentDate);
            incident.UpdateIncidentDate(input.IncidentDate);
            incident.UpdateIncidentProvinceId(input.IncidentProvinceId);
            incident.UpdateIncidentWardId(input.IncidentWardId);
            incident.UpdateIncidentAddress(input.IncidentAddress);
            incident.UpdateIncidentDescription(input.IncidentDescription);
            incident.UpdateIncidentResult(input.IncidentResult);
            incident.UpdateNote(input.Note);
            await ClaimIncidentRepository.UpdateAsync(incident);

            // 11. Load and update risk motor
            var riskMotorQuery = await ClaimIncidentRiskMotorRepository.GetQueryableAsync();
            var riskMotor = await riskMotorQuery.FirstOrDefaultAsync(x => x.IncidentObjectId == incident.Id);
            if (riskMotor != null)
            {
                riskMotor.UpdateCarPlate(normalizedCarPlate);
                riskMotor.UpdateVin(normalizedVin);
                riskMotor.UpdateEngineNumber(normalizedEngineNumber);
                riskMotor.UpdatePersonsOnCar(input.PersonOnCar);
                riskMotor.UpdateDriverName(input.DriverName);
                riskMotor.UpdateDriverPhone(input.DriverPhone);
                riskMotor.UpdateDriverIdNo(input.DriverIdNo);
                riskMotor.UpdateDriverLicenseNo(input.DriverLicenseNo);
                riskMotor.UpdateDriverLicenseLevel(input.DriverLicenseLevel);
                riskMotor.UpdateDriverLicenseEffectDate(input.DriverLicenseEffectDate);
                riskMotor.UpdateDriverLicenseExpireDate(input.DriverLicenseExpireDate);
                riskMotor.UpdateDriverSex(input.DriverSex?.ToUpperInvariant());
                riskMotor.UpdateCarRegistryNo(input.DriverRegistryNo);
                riskMotor.UpdateCarRegistryEffectDate(input.DriverRegistryEffectDate);
                riskMotor.UpdateCarRegistryExpireDate(input.DriverRegistryExpireDate);
                await ClaimIncidentRiskMotorRepository.UpdateAsync(riskMotor);
            }
        }

        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    /// <summary>
    /// Hủy yêu cầu bồi thường. Chỉ cho phép khi trạng thái là Nháp (Draft). Chuyển status sang Đã hủy (Called).
    /// </summary>
    public virtual async Task<ClaimDetailDto> CancelAsync(Guid id)
    {
        var claim = await ClaimRepository.GetAsync(id);

        if (claim.Status != ClaimStatus.Draft && claim.Status != ClaimStatus.PendingReceive)
        {
            throw new UserFriendlyException(L["OnlyDraftCanBeCancelled"].Value);
        }

        var currentEmployee = await GetCurrentEmployeeAsync();
        var cancelDate = Clock.Now;

        await ClaimManager.UpdateAsync(
            claim,
            claim.LobId,
            claim.ProcessClaimType,
            ClaimStatus.Called,
            claim.OpenDate,
            claim.NotifyDate,
            claim.NotifierName,
            claim.NotifierPhone,
            claim.ContactName,
            claim.ContactPhone,
            claim.Priority,
            claim.OpenEmployeeId,
            claim.ClaimTypeId,
            claim.InsurerId,
            claim.InsurerIncidentCode,
            claim.CloseDate,
            cancelDate,
            claim.CloseEmployeeId,
            currentEmployee.Id,
            claim.CancelReasonId,
            claim.CancelNote,
            claim.NotifierEmail,
            claim.NotifierInRelationship,
            claim.ContactEmail,
            claim.ContactInRelationship,
            claim.SnapshotLink,
            claim.ProcessDeptId,
            claim.ProcessEmpId,
            claim.CertificateNo);

        await ClaimRepository.UpdateAsync(claim);
        await CurrentUnitOfWork.SaveChangesAsync();

        var workTasks = await WorkTaskRepository.GetListAsync(wt =>
            wt.BusinessCode == "CLAIM_ASSIGN" &&
            wt.BusinessKey == claim.Id &&
            (wt.Status == WorkTaskStatus.New || wt.Status == WorkTaskStatus.InProgress));

        foreach (var wt in workTasks)
        {
            wt.UpdateStatus(WorkTaskStatus.Cancelled);
            await WorkTaskRepository.UpdateAsync(wt);
        }

        if (workTasks.Count > 0)
        {
            await CurrentUnitOfWork.SaveChangesAsync();
        }

        return await GetAsync(id);
    }

    public virtual async Task SubmitForAssessmentAsync(Guid id)
    {
        var claim = await ClaimRepository.GetAsync(id);

        if (!claim.ProcessDeptId.HasValue || !claim.ProcessEmpId.HasValue)
        {
            throw new UserFriendlyException("Bạn bắt buộc phải chọn đơn vị và người giám định!");
        }

        // Khi gửi giám định, nếu claim đang ở trạng thái Nháp thì chuyển sang Chờ xử lý (PendingReceive)
        if (claim.Status == ClaimStatus.Draft)
        {
            await ClaimManager.UpdateAsync(
                claim,
                claim.LobId,
                claim.ProcessClaimType,
                ClaimStatus.PendingReceive,
                claim.OpenDate,
                claim.NotifyDate,
                claim.NotifierName,
                claim.NotifierPhone,
                claim.ContactName,
                claim.ContactPhone,
                claim.Priority,
                claim.OpenEmployeeId,
                claim.ClaimTypeId,
                claim.InsurerId,
                claim.InsurerIncidentCode,
                claim.CloseDate,
                claim.CancelDate,
                claim.CloseEmployeeId,
                claim.CancelEmployeeId,
                claim.CancelReasonId,
                claim.CancelNote,
                claim.NotifierEmail,
                claim.NotifierInRelationship,
                claim.ContactEmail,
                claim.ContactInRelationship,
                claim.SnapshotLink,
                claim.ProcessDeptId,
                claim.ProcessEmpId,
                claim.CertificateNo);

            await ClaimRepository.UpdateAsync(claim);

            // Thêm bản ghi ClaimStage đầu tiên cho yêu cầu bồi thường
            // Lấy ClaimType theo code = "res_claim_type"
            var resClaimType = await ResClaimTypeRepository.FirstOrDefaultAsync(x => x.Code == "vehicle_claim");
            if (resClaimType == null)
            {
                throw new UserFriendlyException("Không tìm thấy cấu hình loại yêu cầu bồi thường với code = vehicle_claim.");
            }

            // Lấy ResClaimStage có code = "NEW" theo ClaimTypeId
            var resClaimStageQuery = await ResClaimStageRepository.GetQueryableAsync();
            var resClaimStage = await resClaimStageQuery
                .FirstOrDefaultAsync(x => x.ClaimTypeId == resClaimType.Id && x.Code == "NEW");

            if (resClaimStage == null)
            {
                throw new UserFriendlyException("Không tìm thấy cấu hình giai đoạn NEW cho loại yêu cầu bồi thường tương ứng.");
            }

            var now = Clock.Now;

            var claimStage = new ClaimStage(
                GuidGenerator.Create(),
                claim.Id,
                resClaimStage.Id,
                now,
                ClaimStageStatus.New,
                claimFolderId: null,
                partnerId: null,
                startDate: now,
                endDate: null);

            await ClaimStageRepository.InsertAsync(claimStage);

            await CurrentUnitOfWork.SaveChangesAsync();
        }

        // Chỉ khởi tạo workflow với trường hợp ProcessClaimType != Insurer
        if (claim.ProcessClaimType != ProcessClaimType.Insurer)
        {
            var assigneeId = claim.ProcessEmpId.Value.ToString();
            await _elsaWorkflowService.InitCreateClaimWorkflowAsync(id, assigneeId);
        }
    }

    // Helper methods for CreateAsync
    protected virtual async Task ValidateCreateClaimInputAsync(CreateClaimDto input)
    {
        // Validate certificate_no OR (car_plate OR vin OR engine_number)
        var hasCertificate = !string.IsNullOrWhiteSpace(input.CertificateNo);
        var hasVehicleInfo = !string.IsNullOrWhiteSpace(input.CarPlate) ||
                            !string.IsNullOrWhiteSpace(input.Vin) ||
                            !string.IsNullOrWhiteSpace(input.EngineNumber);

        if (!hasCertificate && !hasVehicleInfo)
        {
            throw new UserFriendlyException(L["CertificateOrVehicleInfoRequired"].Value);
        }

        // Validate action = assign requires process_dept_id and process_emp_id
        if (input.Action.ToLowerInvariant() == "assign")
        {
            if (!input.ProcessDeptId.HasValue || !input.ProcessEmpId.HasValue)
            {
                throw new UserFriendlyException(L["ProcessDeptAndEmpRequiredForAssign"].Value);
            }
        }

        // Validate driver license dates
        if (input.DriverLicenseEffectDate.HasValue && input.DriverLicenseEffectDate.Value > Clock.Now)
        {
            throw new UserFriendlyException(L["DriverLicenseEffectDateMustBePastOrToday"].Value);
        }

        if (input.DriverRegistryEffectDate.HasValue && input.DriverRegistryEffectDate.Value > Clock.Now)
        {
            throw new UserFriendlyException(L["DriverRegistryEffectDateMustBePastOrToday"].Value);
        }

        // Validate on_location
        var upperOnLocation = input.OnLocation.ToUpperInvariant();
        if (upperOnLocation != "Y" && upperOnLocation != "N")
        {
            throw new UserFriendlyException(L["OnLocationMustBeYOrN"].Value);
        }

        // Validate driver_sex
        if (!string.IsNullOrWhiteSpace(input.DriverSex))
        {
            var upperSex = input.DriverSex.ToUpperInvariant();
            if (upperSex != "M" && upperSex != "F")
            {
                throw new UserFriendlyException(L["DriverSexMustBeMOrF"].Value);
            }
        }
    }

    protected virtual async Task ValidateUpdateClaimInputAsync(UpdateClaimDto input)
    {
        // Same validation as Create (except action/assign)
        var hasCertificate = !string.IsNullOrWhiteSpace(input.CertificateNo);
        var hasVehicleInfo = !string.IsNullOrWhiteSpace(input.CarPlate) ||
                            !string.IsNullOrWhiteSpace(input.Vin) ||
                            !string.IsNullOrWhiteSpace(input.EngineNumber);

        if (!hasCertificate && !hasVehicleInfo)
        {
            throw new UserFriendlyException(L["CertificateOrVehicleInfoRequired"].Value);
        }

        if (input.DriverLicenseEffectDate.HasValue && input.DriverLicenseEffectDate.Value > Clock.Now)
        {
            throw new UserFriendlyException(L["DriverLicenseEffectDateMustBePastOrToday"].Value);
        }

        if (input.DriverRegistryEffectDate.HasValue && input.DriverRegistryEffectDate.Value > Clock.Now)
        {
            throw new UserFriendlyException(L["DriverRegistryEffectDateMustBePastOrToday"].Value);
        }

        var upperOnLocation = input.OnLocation.ToUpperInvariant();
        if (upperOnLocation != "Y" && upperOnLocation != "N")
        {
            throw new UserFriendlyException(L["OnLocationMustBeYOrN"].Value);
        }

        if (!string.IsNullOrWhiteSpace(input.DriverSex))
        {
            var upperSex = input.DriverSex.ToUpperInvariant();
            if (upperSex != "M" && upperSex != "F")
            {
                throw new UserFriendlyException(L["DriverSexMustBeMOrF"].Value);
            }
        }

        await Task.CompletedTask;
    }

    protected virtual async Task<List<Policy>> FindMatchingPoliciesAsync(
        string? certificateNo,
        string? normalizedCarPlate,
        string? normalizedVin,
        string? normalizedEngineNumber,
        DateTime incidentDate)
    {
        var policyQuery = await PolicyRepository.GetQueryableAsync();
        var versionQuery = await PolicyVersionRepository.GetQueryableAsync();
        var certificateQuery = await PolicyCertificateRepository.GetQueryableAsync();
        var riskMotorQuery = await PolicyRiskMotorRepository.GetQueryableAsync();

        var matchingPolicyIds = new HashSet<Guid>();

        // Find policies by certificate number
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

        // Find policies by vehicle info
        if (!string.IsNullOrWhiteSpace(normalizedCarPlate) ||
            !string.IsNullOrWhiteSpace(normalizedVin) ||
            !string.IsNullOrWhiteSpace(normalizedEngineNumber))
        {
            // Load all risk motors with their versions
            var allRiskMotors = await AsyncExecuter.ToListAsync(
                from rm in riskMotorQuery
                where rm.PolicyRiskObjectId != null
                select rm
            );

            // Load all risk objects
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

                bool matches = false;

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

        // Load policies with navigation properties
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
                    .ThenInclude(c => c.Customer)
                .Include(p => p.Contract)
                    .ThenInclude(c => c.Insurer)
                .Include(p => p.Lob)
        );

        return policies;
    }

    protected virtual async Task<Guid?> DetermineInsurerIdAsync(List<Policy> policies, Guid inputInsurerId)
    {
        // Try to get insurer from first policy's contract
        if (policies.Count > 0)
        {
            var firstPolicy = policies[0];
            if (firstPolicy.ContractId.HasValue && firstPolicy.Contract != null)
            {
                // PolicyContract has InsurerId
                var contract = firstPolicy.Contract;
                if (contract.InsurerId.HasValue)
                {
                    return contract.InsurerId.Value;
                }
            }
        }

        // Fallback to input insurer ID
        return inputInsurerId != Guid.Empty ? inputInsurerId : null;
    }

    protected virtual async Task<HrEmployee> GetCurrentEmployeeAsync()
    {
        if (CurrentUser.Id == null)
        {
            throw new UserFriendlyException(L["UserNotAuthenticated"].Value);
        }

        var employee = await EmployeeRepository.FirstOrDefaultAsync(e => e.UserId == CurrentUser.Id.Value);
        if (employee == null)
        {
            throw new UserFriendlyException(L["EmployeeNotFoundForUser"].Value);
        }

        return employee;
    }

    /// <summary>
    /// Gets LOB from the first policy in the list; if none, falls back to default CAR LOB.
    /// </summary>
    protected virtual async Task<ProLineOfBusiness> GetLobFromPoliciesOrDefaultAsync(List<Policy> policies)
    {
        if (policies != null && policies.Count > 0)
        {
            var firstPolicy = policies[0];
            if (firstPolicy.Lob != null)
            {
                return firstPolicy.Lob;
            }
            if (firstPolicy.LobId != Guid.Empty)
            {
                var lob = await LobRepository.FirstOrDefaultAsync(l => l.Id == firstPolicy.LobId);
                if (lob != null)
                {
                    return lob;
                }
            }
        }

        return await GetDefaultLobAsync();
    }

    protected virtual async Task<ProLineOfBusiness> GetDefaultLobAsync()
    {
        var lob = await LobRepository.FirstOrDefaultAsync(l => l.Code == "CAR");
        if (lob == null)
        {
            throw new UserFriendlyException(L["DefaultLobNotFound"].Value);
        }

        return lob;
    }

    protected virtual async Task<string> GenerateClaimCodeAsync()
    {
        return await GetNextSequenceCodeAsync("CLAIM_CODE");
    }

    /// <summary>
    /// Mã hóa claim Id thành chuỗi snapshot (Base64Url) để đưa vào URL; có thể giải mã bằng <see cref="TryDecryptSnapshotCodeToClaimId"/>.
    /// </summary>
    protected virtual string EncryptClaimIdToSnapshotCode(Guid claimId)
    {
        var plainBytes = claimId.ToByteArray();
        var cipherBytes = AesEncrypt(plainBytes);
        return Base64UrlEncode(cipherBytes);
    }

    /// <summary>
    /// Giải mã chuỗi snapshot (từ URL) về claim Id. Trả về true nếu giải mã thành công.
    /// </summary>
    public virtual bool TryDecryptSnapshotCodeToClaimId(string snapshotCode, out Guid? claimId)
    {
        claimId = null;
        if (string.IsNullOrWhiteSpace(snapshotCode))
        {
            return false;
        }

        try
        {
            var cipherBytes = Base64UrlDecode(snapshotCode.Trim());
            if (cipherBytes == null || cipherBytes.Length == 0)
            {
                return false;
            }

            var plainBytes = AesDecrypt(cipherBytes);
            if (plainBytes == null || plainBytes.Length != 16)
            {
                return false;
            }

            claimId = new Guid(plainBytes);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public virtual Task<Guid?> GetClaimIdFromSnapshotCodeAsync(string snapshotCode)
    {
        TryDecryptSnapshotCodeToClaimId(snapshotCode, out var claimId);
        return Task.FromResult(claimId);
    }

    // AES-256: key 32 bytes, IV 16 bytes (block size)
    private static readonly byte[] SnapshotAesKey = Encoding.UTF8.GetBytes("iOne.Claim.Snapshot.Key.32Bytes!");
    private static readonly byte[] SnapshotAesIv = Encoding.UTF8.GetBytes("iOne.Snapshot.IV"); // 16 chars

    private static byte[] AesEncrypt(byte[] plainBytes)
    {
        using var aes = Aes.Create();
        aes.Key = SnapshotAesKey;
        aes.IV = SnapshotAesIv;
        using var encryptor = aes.CreateEncryptor();
        return encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
    }

    private static byte[] AesDecrypt(byte[] cipherBytes)
    {
        using var aes = Aes.Create();
        aes.Key = SnapshotAesKey;
        aes.IV = SnapshotAesIv;
        using var decryptor = aes.CreateDecryptor();
        return decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        var base64 = Convert.ToBase64String(bytes);
        return base64.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }

    private static byte[]? Base64UrlDecode(string base64Url)
    {
        if (string.IsNullOrEmpty(base64Url))
        {
            return null;
        }

        var base64 = base64Url.Replace('-', '+').Replace('_', '/');
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }

        try
        {
            return Convert.FromBase64String(base64);
        }
        catch
        {
            return null;
        }
    }

    protected virtual async Task<string> GenerateSnapshotCodeAsync()
    {
        return await GetNextSequenceCodeAsync("CLAIM_SNAPSHOT_CODE");
    }

    protected virtual string GenerateSnapshotLink(string snapshotCode)
    {
        // Lấy base URL từ cấu hình, ưu tiên cấu hình riêng cho Claim
        var baseUrl = Configuration["Claim:SnapshotBaseUrl"]
                      ?? Configuration["App:ExternalUrl"]
                      ?? "https://ione.vnexco.com";

        baseUrl = baseUrl.TrimEnd('/');

        return $"{baseUrl}/firstNotifyOfLoss/incident/infor/{snapshotCode}";
    }

    /// <summary>
    /// Gets ObjectType from the first policy risk object in the list; if none, falls back to default CAR.
    /// </summary>
    protected virtual async Task<ResObjectType> GetObjectTypeFromPoliciesOrDefaultAsync(List<Policy> policies)
    {
        if (policies != null)
        {
            foreach (var policy in policies)
            {
                if (policy.PolicyVersions == null) continue;
                foreach (var version in policy.PolicyVersions)
                {
                    if (version.PolicyRiskObjects == null) continue;
                    var firstRiskObject = version.PolicyRiskObjects.FirstOrDefault();
                    if (firstRiskObject?.ObjectType != null)
                    {
                        return firstRiskObject.ObjectType;
                    }
                    if (firstRiskObject != null && firstRiskObject.ObjectTypeId != Guid.Empty)
                    {
                        var objectType = await ResObjectTypeRepository.FirstOrDefaultAsync(ot => ot.Id == firstRiskObject.ObjectTypeId);
                        if (objectType != null)
                        {
                            return objectType;
                        }
                    }
                }
            }
        }

        return await GetVehicleObjectTypeAsync();
    }

    protected virtual async Task<ResObjectType> GetVehicleObjectTypeAsync()
    {
        var objectType = await ResObjectTypeRepository.FirstOrDefaultAsync(ot => ot.Code == "CAR");
        if (objectType == null)
        {
            throw new UserFriendlyException(L["VehicleObjectTypeNotFound"].Value);
        }

        return objectType;
    }

    protected virtual async Task<List<PolicyDto>> MapPoliciesToDtosAsync(List<Policy> policies)
    {
        var policyDtos = new List<PolicyDto>();

        // Load all product IDs
        var allProductIds = policies
            .SelectMany(p => p.PolicyVersions
                .SelectMany(v => v.PolicyProducts.Select(pp => pp.ProductId)))
            .Distinct()
            .ToList();

        // Load products
        var productNameById = new Dictionary<Guid, string>();
        if (allProductIds.Count > 0)
        {
            var productRepository = LazyServiceProvider.LazyGetRequiredService<IRepository<ProProduct, Guid>>();
            var productQuery = await productRepository.GetQueryableAsync();
            var products = await AsyncExecuter.ToListAsync(
                productQuery
                    .Where(p => allProductIds.Contains(p.Id))
                    .Select(p => new { p.Id, p.Name })
            );
            productNameById = products.ToDictionary(p => p.Id, p => p.Name ?? "");
        }

        foreach (var policy in policies)
        {
            // Get latest active version
            var latestVersion = policy.PolicyVersions
                .Where(v => v.Status == "active")
                .OrderByDescending(v => v.Version)
                .FirstOrDefault();

            if (latestVersion == null) continue;

            // Get certificates
            var certificates = latestVersion.PolicyCertificates.ToList();
            var certificateNo = certificates.FirstOrDefault()?.CertificateNo;
            var certificateUrl = certificates.FirstOrDefault()?.Url;

            // Get products
            var products = latestVersion.PolicyProducts
                .Select(pp => productNameById.TryGetValue(pp.ProductId, out var name) ? name : "")
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Distinct()
                .ToList();

            // Get vehicle info
            var riskMotor = latestVersion.PolicyRiskObjects
                .SelectMany(ro => ro.PolicyRiskMotors)
                .FirstOrDefault();

            var carPlate = riskMotor?.CarPlate;
            var ownerName = policy.Contract?.Customer?.Name ?? "";

            // Get LOB name and insurer
            var lobName = policy.Lob?.Name ?? "";
            var insurerId = policy.Contract?.InsurerId;
            var insurerName = policy.Contract?.Insurer?.Name;

            // Create DTO for each product (split products into separate rows)
            if (products.Count > 0)
            {
                foreach (var product in products)
                {
                    policyDtos.Add(new PolicyDto
                    {
                        PolicyId = policy.Id,
                        LobName = lobName,
                        ContractId = policy.Contract?.Code ?? "",
                        CertificateNo = certificateNo,
                        Products = new List<string> { product },
                        CarPlate = carPlate,
                        OwnerName = ownerName,
                        EffectDate = latestVersion.EffectDate,
                        ExpireDate = latestVersion.ExpireDate,
                        Status = latestVersion.Status,
                        CertificateUrl = certificateUrl,
                        InsurerId = insurerId,
                        InsurerName = insurerName
                    });
                }
            }
            else
            {
                // If no products, still create one DTO
                policyDtos.Add(new PolicyDto
                {
                    PolicyId = policy.Id,
                    LobName = lobName,
                    ContractId = policy.Contract?.Code ?? "",
                    CertificateNo = certificateNo,
                    Products = new List<string>(),
                    CarPlate = carPlate,
                    OwnerName = ownerName,
                    EffectDate = latestVersion.EffectDate,
                    ExpireDate = latestVersion.ExpireDate,
                    Status = latestVersion.Status,
                    CertificateUrl = certificateUrl,
                    InsurerId = insurerId,
                    InsurerName = insurerName
                });
            }
        }

        return policyDtos;
    }

    protected virtual async Task<string> GetNextSequenceCodeAsync(string code, Dictionary<string, string>? parameters = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new BusinessException("Master:ResSequence:CodeOrIdRequired");
        }

        var query = await ResSequenceRepository.GetQueryableAsync();
        var sequence = await query.FirstOrDefaultAsync(x => x.Code == code);
        if (sequence == null)
        {
            throw new BusinessException("Master:ResSequence:NotFound").WithData("Code", code);
        }

        if (sequence.Status != ResSequenceStatus.Active)
        {
            throw new BusinessException("Master:ResSequence:NotActive").WithData("Code", code);
        }

        // Reload to get latest NumberNext (important for concurrent requests)
        var freshSequence = await ResSequenceRepository.GetAsync(sequence.Id);
        if (freshSequence.Status != ResSequenceStatus.Active)
        {
            throw new BusinessException("Master:ResSequence:NotActive").WithData("Code", code);
        }

        var currentNumber = freshSequence.NumberNext;
        var shouldReset = false;

        if (freshSequence.UseDateRange == ResSequenceUseDateRange.Yes && freshSequence.DateRangeType.HasValue)
        {
            shouldReset = ShouldResetSequence(freshSequence);
            if (shouldReset)
            {
                currentNumber = 0;
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

    protected virtual bool ShouldResetSequence(ResSequence sequence)
    {
        if (sequence.UseDateRange != ResSequenceUseDateRange.Yes || !sequence.DateRangeType.HasValue)
        {
            return false;
        }

        var now = Clock.Now;
        var lastModified = sequence.LastModificationTime ?? sequence.CreationTime;

        return sequence.DateRangeType.Value switch
        {
            ResSequenceDateRangeType.Week => GetWeekOfYear(now) != GetWeekOfYear(lastModified) || now.Year != lastModified.Year,
            ResSequenceDateRangeType.Month => now.Year != lastModified.Year || now.Month != lastModified.Month,
            ResSequenceDateRangeType.Quarter => ((now.Month - 1) / 3 + 1) != ((lastModified.Month - 1) / 3 + 1) || now.Year != lastModified.Year,
            ResSequenceDateRangeType.Half => (now.Month <= 6 ? 1 : 2) != (lastModified.Month <= 6 ? 1 : 2) || now.Year != lastModified.Year,
            ResSequenceDateRangeType.Year => now.Year != lastModified.Year,
            _ => false
        };
    }

    protected virtual int GetWeekOfYear(DateTime date)
    {
        var calendar = CultureInfo.CurrentCulture.Calendar;
        return calendar.GetWeekOfYear(date, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
    }

    protected virtual string GenerateSequenceCode(ResSequence sequence, long number, Dictionary<string, string>? parameters = null)
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

    protected virtual string ProcessTemplate(string template, Dictionary<string, string>? parameters = null)
    {
        if (string.IsNullOrWhiteSpace(template))
        {
            return string.Empty;
        }

        if (parameters == null || parameters.Count == 0)
        {
            return template;
        }

        var result = template;
        foreach (var param in parameters)
        {
            result = result.Replace($"{{{param.Key}}}", param.Value);
        }

        return result;
    }

    protected virtual async Task<IQueryable<ClaimEntity>> CreateFilteredQueryAsync(GetClaimsInput input)
    {
        var query = await ClaimRepository.GetQueryableAsync();

        // Include navigation properties to avoid N+1 queries
        query = query
            .Include(x => x.Lob)
            .Include(x => x.Insurer)
            .Include(x => x.OpenEmployee);

        // Filter by LobId
        if (input.LobId.HasValue)
        {
            query = query.Where(x => x.LobId == input.LobId.Value);
        }

        // Filter by InsurerId
        if (input.InsurerId.HasValue)
        {
            query = query.Where(x => x.InsurerId == input.InsurerId.Value);
        }

        // Filter by ProcessClaimType
        if (input.ProcessClaimType.HasValue)
        {
            query = query.Where(x => x.ProcessClaimType == input.ProcessClaimType.Value);
        }

        // Filter by ProcessDeptId (filter by OpenEmployee's department and child departments)
        if (input.ProcessDeptId.HasValue)
        {
            var departmentIds = await GetDepartmentAndChildIdsAsync(input.ProcessDeptId.Value);
            query = query.Where(x => departmentIds.Contains(x.OpenEmployee.DepartmentId));
        }

        // Filter by NotifierPhone
        if (!string.IsNullOrWhiteSpace(input.NotifierPhone))
        {
            query = query.Where(x => EF.Functions.ILike(x.NotifierPhone, $"%{input.NotifierPhone}%"));
        }

        // Filter by OpenEmployeeId (claims opened by this employee)
        if (input.OpenEmployeeId.HasValue)
        {
            query = query.Where(x => x.OpenEmployeeId == input.OpenEmployeeId.Value);
        }

        // Filter by OpenDateFrom
        if (input.OpenDateFrom.HasValue)
        {
            query = query.Where(x => x.OpenDate >= input.OpenDateFrom.Value.Date);
        }

        // Filter by OpenDateTo
        if (input.OpenDateTo.HasValue)
        {
            query = query.Where(x => x.OpenDate <= input.OpenDateTo.Value.Date.AddDays(1).AddTicks(-1));
        }

        // Filter by Status
        if (input.Status.HasValue)
        {
            query = query.Where(x => x.Status == input.Status.Value);
        }

        // Filter by Code
        if (!string.IsNullOrWhiteSpace(input.Code))
        {
            query = query.Where(x => EF.Functions.ILike(x.Code, $"%{input.Code}%"));
        }

        // Filter by CarPlate, Vin, EngineNumber (need to join with ClaimIncidentRiskMotor)
        // Note: EF Core cannot translate NormalizeCarInfo, so we load and filter in memory
        if (!string.IsNullOrWhiteSpace(input.CarPlate) || !string.IsNullOrWhiteSpace(input.Vin) || !string.IsNullOrWhiteSpace(input.EngineNumber))
        {
            var claimIncidentQuery = await ClaimIncidentRepository.GetQueryableAsync();
            var riskMotorQuery = await ClaimIncidentRiskMotorRepository.GetQueryableAsync();

            // Join ClaimIncident with ClaimIncidentRiskMotor
            var claimRiskMotorData = await AsyncExecuter.ToListAsync(
                from ci in claimIncidentQuery
                join rm in riskMotorQuery on ci.Id equals rm.IncidentObjectId into riskMotors
                from rm in riskMotors.DefaultIfEmpty()
                select new { ClaimId = ci.IncidentId, RiskMotor = rm }
            );

            var matchingClaimIds = new List<Guid>();

            foreach (var item in claimRiskMotorData)
            {
                bool matches = true;

                if (!string.IsNullOrWhiteSpace(input.CarPlate))
                {
                    var normalizedInput = NormalizeCarInfo(input.CarPlate);
                    var normalizedValue = item.RiskMotor != null && !string.IsNullOrEmpty(item.RiskMotor.CarPlate) 
                        ? NormalizeCarInfo(item.RiskMotor.CarPlate) 
                        : string.Empty;
                    if (normalizedValue != normalizedInput)
                    {
                        matches = false;
                    }
                }

                if (matches && !string.IsNullOrWhiteSpace(input.Vin))
                {
                    var normalizedInput = NormalizeCarInfo(input.Vin);
                    var normalizedValue = item.RiskMotor != null && !string.IsNullOrEmpty(item.RiskMotor.Vin) 
                        ? NormalizeCarInfo(item.RiskMotor.Vin) 
                        : string.Empty;
                    if (normalizedValue != normalizedInput)
                    {
                        matches = false;
                    }
                }

                if (matches && !string.IsNullOrWhiteSpace(input.EngineNumber))
                {
                    var normalizedInput = NormalizeCarInfo(input.EngineNumber);
                    var normalizedValue = item.RiskMotor != null && !string.IsNullOrEmpty(item.RiskMotor.EngineNumber) 
                        ? NormalizeCarInfo(item.RiskMotor.EngineNumber) 
                        : string.Empty;
                    if (normalizedValue != normalizedInput)
                    {
                        matches = false;
                    }
                }

                if (matches)
                {
                    matchingClaimIds.Add(item.ClaimId);
                }
            }

            if (matchingClaimIds.Count > 0)
            {
                query = query.Where(x => matchingClaimIds.Contains(x.Id));
            }
            else
            {
                // No matches found, return empty result
                query = query.Where(x => false);
            }
        }

        return query;
    }

    protected virtual IQueryable<ClaimEntity> ApplySorting(IQueryable<ClaimEntity> query, string sorting)
    {
        if (string.IsNullOrWhiteSpace(sorting))
        {
            return query.OrderByDescending(x => x.OpenDate);
        }

        var sortParts = sorting.Split(' ');
        var sortField = sortParts[0];
        var sortOrder = sortParts.Length > 1 ? sortParts[1].ToLowerInvariant() : "desc";

        return sortField.ToLowerInvariant() switch
        {
            "code" => sortOrder == "asc" ? query.OrderBy(x => x.Code) : query.OrderByDescending(x => x.Code),
            "opendate" => sortOrder == "asc" ? query.OrderBy(x => x.OpenDate) : query.OrderByDescending(x => x.OpenDate),
            "notifydate" => sortOrder == "asc" ? query.OrderBy(x => x.NotifyDate) : query.OrderByDescending(x => x.NotifyDate),
            "status" => sortOrder == "asc" ? query.OrderBy(x => x.Status) : query.OrderByDescending(x => x.Status),
            _ => query.OrderByDescending(x => x.OpenDate)
        };
    }

    protected virtual IQueryable<ClaimEntity> ApplyPaging(IQueryable<ClaimEntity> query, GetClaimsInput input)
    {
        if (input.SkipCount > 0)
        {
            query = query.Skip(input.SkipCount);
        }

        if (input.MaxResultCount > 0)
        {
            query = query.Take(input.MaxResultCount);
        }

        return query;
    }

    protected virtual string NormalizeCarInfo(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        // Remove special characters, keep only letters and numbers, convert to uppercase
        return Regex.Replace(value, @"[^A-Za-z0-9]", "").ToUpperInvariant();
    }

    protected virtual async Task<List<Guid>> GetDepartmentAndChildIdsAsync(Guid departmentId)
    {
        var departmentIds = new List<Guid> { departmentId };
        var allDepartments = await DepartmentRepository.GetListAsync();
        
        var childDepartments = allDepartments.Where(x => x.ParentId == departmentId).ToList();
        foreach (var child in childDepartments)
        {
            departmentIds.Add(child.Id);
            var grandChildren = await GetDepartmentAndChildIdsAsync(child.Id);
            departmentIds.AddRange(grandChildren);
        }

        return departmentIds.Distinct().ToList();
    }
}
