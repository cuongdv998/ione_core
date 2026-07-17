using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using iOne.HrEmployees;
using iOne.Policies;
using iOne.Policy.Policies;
using iOne.PolicyContracts;
using iOne.Policy.Localization;
using iOne.Policy.Permissions;
using iOne.ProLineOfBusinesses;
using iOne.ResCustomers;
using iOne.ResDocuments;
using iOne.ResPartners;
using iOne.ResPartnerTypes;
using iOne.ResSequences;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Identity;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace iOne.Policy.PolicyContracts;

[RemoteService(Name = PolicyRemoteServiceConsts.RemoteServiceName)]
[Authorize(PolicyContractPermissions.Default)]
public class PolicyContractAppService : CrudAppService<
    iOne.PolicyContracts.PolicyContract,
    PolicyContractDto,
    Guid,
    GetPolicyContractsInput,
    CreatePolicyContractDto,
    UpdatePolicyContractDto>, IPolicyContractAppService
{
    private const string InsurerPartnerTypeCode = "INSURER";
    private const string CarLobCode = "CAR";
    private const string ContractCodeSequenceCode = "CONTRACT_CODE_SEQ";

    public PolicyContractAppService(
        IRepository<iOne.PolicyContracts.PolicyContract, Guid> repository,
        PolicyContractManager manager,
        IPolicyContractRepository policyContractRepository,
        ICurrentUser currentUser,
        IRepository<HrEmployee, Guid> employeeRepository,
        IRepository<iOne.Policies.Policy, Guid> policyRepository,
        IResSequenceRepository resSequenceRepository,
        IResDocumentRepository resDocumentRepository,
        IResCustomerRepository resCustomerRepository,
        IProLineOfBusinessRepository proLineOfBusinessRepository,
        IRepository<ResPartner, Guid> resPartnerRepository,
        IRepository<ResPartnerType, Guid> resPartnerTypeRepository,
        IPolicyContractDocumentRepository policyContractDocumentRepository,
        PolicyContractDocumentManager policyContractDocumentManager,
        IPolicyAppService policyAppService,
        IRepository<IdentityUser, Guid> userRepository,
        IRepository<PolicyVersion, Guid> policyVersionRepository)
        : base(repository)
    {
        Manager = manager;
        PolicyContractRepository = policyContractRepository;
        CurrentUser = currentUser;
        EmployeeRepository = employeeRepository;
        PolicyRepository = policyRepository;
        ResSequenceRepository = resSequenceRepository;
        ResDocumentRepository = resDocumentRepository;
        ResCustomerRepository = resCustomerRepository;
        ProLineOfBusinessRepository = proLineOfBusinessRepository;
        ResPartnerRepository = resPartnerRepository;
        ResPartnerTypeRepository = resPartnerTypeRepository;
        PolicyContractDocumentRepository = policyContractDocumentRepository;
        PolicyContractDocumentManager = policyContractDocumentManager;
        PolicyAppService = policyAppService;
        UserRepository = userRepository;
        PolicyVersionRepository = policyVersionRepository;
        LocalizationResource = typeof(PolicyResource);
        GetPolicyName = PolicyContractPermissions.View;
        GetListPolicyName = PolicyContractPermissions.View;
        CreatePolicyName = PolicyContractPermissions.Create;
        UpdatePolicyName = PolicyContractPermissions.Edit;
        DeletePolicyName = PolicyContractPermissions.Delete;
    }

    protected PolicyContractManager Manager { get; }
    protected IPolicyContractRepository PolicyContractRepository { get; }
    protected ICurrentUser CurrentUser { get; }
    protected IRepository<HrEmployee, Guid> EmployeeRepository { get; }
    protected IRepository<iOne.Policies.Policy, Guid> PolicyRepository { get; }
    protected IResSequenceRepository ResSequenceRepository { get; }
    protected IResDocumentRepository ResDocumentRepository { get; }
    protected IResCustomerRepository ResCustomerRepository { get; }
    protected IProLineOfBusinessRepository ProLineOfBusinessRepository { get; }
    protected IRepository<ResPartner, Guid> ResPartnerRepository { get; }
    protected IRepository<ResPartnerType, Guid> ResPartnerTypeRepository { get; }
    protected IPolicyContractDocumentRepository PolicyContractDocumentRepository { get; }
    protected PolicyContractDocumentManager PolicyContractDocumentManager { get; }
    protected IPolicyAppService PolicyAppService { get; }
    protected IRepository<IdentityUser, Guid> UserRepository { get; }
    protected IRepository<PolicyVersion, Guid> PolicyVersionRepository { get; }

    public virtual async Task<PolicyContractDto> CreateWithResultAsync(CreatePolicyContractDto input)
    {
        var dto = await CreateContractInternalAsync(input);
        return dto;
    }

    public override async Task<PolicyContractDto> CreateAsync(CreatePolicyContractDto input)
    {
        return await CreateContractInternalAsync(input);
    }

    public override async Task<PolicyContractDto> GetAsync(Guid id)
    {
        var query = await Repository.GetQueryableAsync();
        var entity = await AsyncExecuter.FirstOrDefaultAsync(
            query.Include(x => x.Documents).Where(x => x.Id == id));
        if (entity == null)
        {
            throw new Volo.Abp.Domain.Entities.EntityNotFoundException(typeof(iOne.PolicyContracts.PolicyContract), id);
        }
        var dto = ObjectMapper.Map<iOne.PolicyContracts.PolicyContract, PolicyContractDto>(entity);
        var firstDoc = entity.Documents?.FirstOrDefault(d => d.DocumentId.HasValue);
        if (firstDoc != null)
        {
            dto.DocumentId = firstDoc.DocumentId;
        }
        return dto;
    }

    private async Task<PolicyContractDto> CreateContractInternalAsync(CreatePolicyContractDto input)
    {
        var now = Clock.Now;
        var effectDate = input.EffectDate ?? now.Date;
        var expireDate = input.ExpireDate ?? effectDate.AddMonths(12);
        var quantity = input.Quantity > 0 ? input.Quantity : 1m;
        var isReciveInvoice = (input.IsReciveInvoice ?? "N").Trim().ToUpperInvariant() == "Y" ? "Y" : "N";

        if (effectDate.Date < now.Date)
        {
            throw new UserFriendlyException(L["PolicyContract:EffectDateMustBeTodayOrFuture"].Value);
        }

        if (expireDate < effectDate)
        {
            throw new UserFriendlyException(L["PolicyContract:ExpireDateMustBeAfterEffectDate"].Value);
        }

        if (quantity <= 0)
        {
            throw new UserFriendlyException(L["PolicyContract:QuantityMustBePositive"].Value);
        }

        var insurerId = input.InsurerId;
        var insurer = await ResPartnerRepository.GetQueryableAsync().Result.AsNoTracking()
            .Include(p => p.PartnerType)
            .FirstOrDefaultAsync(p => p.Id == insurerId);
        if (insurer == null)
        {
            throw new UserFriendlyException(L["PolicyContract:InsurerNotFound"].Value);
        }
        if (insurer.PartnerType?.Code != InsurerPartnerTypeCode)
        {
            throw new UserFriendlyException(L["PolicyContract:InsurerMustBeInsurerType"].Value);
        }

        var insurerContractCode = string.IsNullOrWhiteSpace(input.InsurerContractCode) ? null : input.InsurerContractCode!.Trim();
        if (!string.IsNullOrEmpty(insurerContractCode) && await PolicyContractRepository.IsInsurerContractCodeExistsAsync(insurerId, insurerContractCode))
        {
            throw new UserFriendlyException(L["PolicyContract:InsurerContractCodeDuplicate", insurerContractCode].Value);
        }

        Guid? lobId = input.LobId;
        if (!lobId.HasValue)
        {
            var carLob = await ProLineOfBusinessRepository.FindByCodeAsync(CarLobCode);
            if (carLob != null)
            {
                lobId = carLob.Id;
            }
        }

        var customer = await ResCustomerRepository.FindAsync(input.CustomerId);
        if (customer == null)
        {
            throw new UserFriendlyException(L["PolicyContract:CustomerNotFound"].Value);
        }

        ResCustomer? payerCustomer = null;
        if (input.PayerId.HasValue)
        {
            payerCustomer = await ResCustomerRepository.FindAsync(input.PayerId.Value);
            if (payerCustomer == null)
            {
                throw new UserFriendlyException(L["PolicyContract:PayerNotFound"].Value);
            }
        }

        var payerName = input.PayerName?.Trim();
        var payerEmail = input.PayerEmail?.Trim();
        var payerPhone = input.PayerPhone?.Trim();
        var payerAddress = input.PayerAddress?.Trim();
        var payerFullAddress = input.PayerFullAddress?.Trim();
        if (payerCustomer != null)
        {
            payerName = payerCustomer.Name;
            payerEmail = payerCustomer.Email;
            payerPhone = payerCustomer.Phone;
            payerAddress = payerCustomer.Address;
            payerFullAddress = payerCustomer.FullAddress;
        }
        else
        {
            if (string.IsNullOrEmpty(payerName)) payerName = customer.Name;
            if (string.IsNullOrEmpty(payerEmail)) payerEmail = customer.Email;
            if (string.IsNullOrEmpty(payerPhone)) payerPhone = customer.Phone;
            if (string.IsNullOrEmpty(payerAddress)) payerAddress = customer.Address;
            if (string.IsNullOrEmpty(payerFullAddress)) payerFullAddress = customer.FullAddress;
        }

        Guid? employeeId = input.EmployeeId;
        if (!employeeId.HasValue && CurrentUser.Id.HasValue)
        {
            var emp = await AsyncExecuter.FirstOrDefaultAsync(
                (await EmployeeRepository.GetQueryableAsync()).Where(e => e.UserId == CurrentUser.Id));
            if (emp != null)
            {
                employeeId = emp.Id;
            }
        }

        if (input.DocumentId.HasValue)
        {
            var docExists = await ResDocumentRepository.AnyAsync(x => x.Id == input.DocumentId.Value);
            if (!docExists)
            {
                throw new UserFriendlyException(L["PolicyContract:DocumentNotFound"].Value);
            }
        }

        var code = input.Code?.Trim();
        if (string.IsNullOrWhiteSpace(code))
        {
            code = await GetNextSequenceCodeAsync(ContractCodeSequenceCode);
        }
        else if (await PolicyContractRepository.IsCodeExistsAsync(code))
        {
            throw new UserFriendlyException(L["PolicyContract:CodeExists", code].Value);
        }

        var entity = new iOne.PolicyContracts.PolicyContract(
            GuidGenerator.Create(),
            code,
            input.Name.Trim(),
            input.Type,
            input.CustomerId,
            effectDate,
            quantity,
            input.Status,
            insurerId,
            insurerContractCode,
            lobId,
            payerName,
            payerEmail,
            payerPhone,
            input.PayerProvinceId,
            input.PayerWardId,
            payerAddress,
            payerFullAddress,
            input.PayerTaxCode?.Trim(),
            input.Description?.Trim(),
            expireDate,
            0m,
            employeeId,
            input.CancellationDate,
            input.TerminationDate,
            input.CancellationReasonId,
            input.TerminationReasonId,
            input.CancellationDescription?.Trim(),
            input.TerminationDescription?.Trim(),
            isReciveInvoice,
            input.QuotationId
        );

        await Manager.CreateAsync(entity);

        if (input.DocumentId.HasValue)
        {
            var policyContractDoc = new PolicyContractDocument(
                GuidGenerator.Create(),
                entity.Id,
                input.DocumentId.Value);
            await PolicyContractDocumentRepository.InsertAsync(policyContractDoc);
        }

        // Map and return like ResCarBrandAppService.CreateAsync: do NOT call CurrentUnitOfWork.CompleteAsync()
        // so the framework completes UoW after the response is sent; otherwise serialization can run after
        // UoW is disposed and lead to 500.
        var dto = ObjectMapper.Map<iOne.PolicyContracts.PolicyContract, PolicyContractDto>(entity);
        dto.DocumentId = input.DocumentId;

        return dto;
    }

    private async Task<string> GetNextSequenceCodeAsync(string code, Dictionary<string, string>? parameters = null)
    {
        var query = await ResSequenceRepository.GetQueryableAsync();
        var sequence = await query.FirstOrDefaultAsync(x => x.Code == code);
        if (sequence == null)
        {
            throw new UserFriendlyException(L["Policy:ResSequence:NotFound"].Value);
        }
        if (sequence.Status != ResSequenceStatus.Active)
        {
            throw new UserFriendlyException(L["Policy:ResSequence:NotActive"].Value);
        }

        var freshSequence = await ResSequenceRepository.GetAsync(sequence.Id);
        if (freshSequence.Status != ResSequenceStatus.Active)
        {
            throw new UserFriendlyException(L["Policy:ResSequence:NotActive"].Value);
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
        var nextNumber = shouldReset ? freshSequence.NumberIncrement : currentNumber + freshSequence.NumberIncrement;
        freshSequence.UpdateNumberNext(nextNumber);
        await ResSequenceRepository.UpdateAsync(freshSequence);
        await CurrentUnitOfWork.SaveChangesAsync();
        return generatedCode;
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
                return GetWeekOfYear(now) != GetWeekOfYear(lastModified) || now.Year != lastModified.Year;
            case ResSequenceDateRangeType.Month:
                return now.Year != lastModified.Year || now.Month != lastModified.Month;
            case ResSequenceDateRangeType.Quarter:
                return ((now.Month - 1) / 3 + 1) != ((lastModified.Month - 1) / 3 + 1) || now.Year != lastModified.Year;
            case ResSequenceDateRangeType.Half:
                return (now.Month <= 6 ? 1 : 2) != (lastModified.Month <= 6 ? 1 : 2) || now.Year != lastModified.Year;
            case ResSequenceDateRangeType.Year:
                return now.Year != lastModified.Year;
            default:
                return false;
        }
    }

    private static int GetWeekOfYear(DateTime date)
    {
        var calendar = CultureInfo.CurrentCulture.Calendar;
        return calendar.GetWeekOfYear(date, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
    }

    private string GenerateSequenceCode(ResSequence sequence, long number, Dictionary<string, string>? parameters = null)
    {
        var sb = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(sequence.Prefix))
        {
            sb.Append(ProcessTemplate(sequence.Prefix, parameters));
        }
        var numberStr = number.ToString();
        if (sequence.Padding.HasValue && sequence.Padding.Value > 0)
        {
            numberStr = numberStr.PadLeft(sequence.Padding.Value, '0');
        }
        sb.Append(numberStr);
        if (!string.IsNullOrWhiteSpace(sequence.Suffix))
        {
            sb.Append(ProcessTemplate(sequence.Suffix, parameters));
        }
        return sb.ToString();
    }

    private string ProcessTemplate(string template, Dictionary<string, string>? parameters = null)
    {
        if (string.IsNullOrWhiteSpace(template)) return template ?? string.Empty;
        var result = template;
        var now = Clock.Now;
        result = Regex.Replace(result, @"\$date\(([^)]+)\)", match =>
        {
            try { return now.ToString(match.Groups[1].Value); } catch { return match.Value; }
        });
        result = Regex.Replace(result, @"\$\{date\}\(([^)]+)\)", match =>
        {
            try { return now.ToString(match.Groups[1].Value); } catch { return match.Value; }
        });
        if (parameters != null && parameters.Count > 0)
        {
            result = Regex.Replace(result, @"\$\{([^}]+)\}", match =>
            {
                var paramName = match.Groups[1].Value;
                if (paramName == "date") return match.Value;
                return parameters.TryGetValue(paramName, out var v) ? (v ?? string.Empty) : match.Value;
            });
        }
        return result;
    }

    public override async Task<PolicyContractDto> UpdateAsync(Guid id, UpdatePolicyContractDto input)
    {
        var entity = await Repository.GetAsync(id);

        // Validate ExpireDate > EffectDate
        if (input.ExpireDate.HasValue && input.ExpireDate.Value < input.EffectDate)
        {
            throw new UserFriendlyException(L["PolicyContract:ExpireDateMustBeAfterEffectDate"].Value);
        }

        // Update properties (Code is immutable, so we don't update it)
        entity.UpdateCode(entity.Code); // Keep existing Code
        entity.UpdateName(input.Name);
        entity.UpdateType(input.Type);
        entity.UpdateStatus(input.Status);
        entity.UpdateExpireDate(input.ExpireDate);
        entity.UpdateEffectDate(input.EffectDate);

        if (input.InsurerId.HasValue)
        {
            entity.UpdateInsurerId(input.InsurerId);
        }
        if (input.InsurerContractCode != null)
        {
            entity.UpdateInsurerContractCode(input.InsurerContractCode);
        }
        if (input.LobId.HasValue)
        {
            entity.UpdateLobId(input.LobId.Value);
        }
        entity.UpdateCustomerId(input.CustomerId);
        // Only update payer when at least one payer field is sent (frontend may omit when not edited)
        if (input.PayerName != null || input.PayerEmail != null || input.PayerPhone != null
            || input.PayerProvinceId.HasValue || input.PayerWardId.HasValue
            || input.PayerAddress != null || input.PayerFullAddress != null)
        {
            entity.UpdatePayerInfo(
                input.PayerName,
                input.PayerEmail,
                input.PayerPhone,
                input.PayerProvinceId,
                input.PayerWardId,
                input.PayerAddress,
                input.PayerFullAddress);
        }
        if (input.PayerTaxCode != null)
        {
            entity.UpdatePayerTin(input.PayerTaxCode);
        }
        if (input.Description != null)
        {
            entity.UpdateDescription(input.Description);
        }
        var currentQty = entity.CurrentQuantity ?? 0;
        if (input.Quantity < currentQty)
        {
            throw new UserFriendlyException(L["PolicyContract:QuantityMustNotBeLessThanCurrentQuantity"].Value);
        }
        entity.UpdateQuantity(input.Quantity);
        if (input.CurrentQuantity.HasValue)
        {
            entity.UpdateCurrentQuantity(input.CurrentQuantity);
        }
        if (input.EmployeeId.HasValue)
        {
            entity.UpdateEmployeeId(input.EmployeeId);
        }
        entity.UpdateIsReciveInvoice(input.IsReciveInvoice ?? "N");

        // Update document: if provided, replace; if not provided, remove existing
        var existingDocs = await PolicyContractDocumentRepository.GetListAsync(x => x.PolicyContractId == id);
        if (input.DocumentId.HasValue)
        {
            var docExists = await ResDocumentRepository.AnyAsync(x => x.Id == input.DocumentId.Value);
            if (!docExists)
            {
                throw new UserFriendlyException(L["PolicyContract:DocumentNotFound"].Value);
            }
            foreach (var doc in existingDocs)
            {
                await PolicyContractDocumentRepository.DeleteAsync(doc);
            }
            var policyContractDoc = new PolicyContractDocument(
                GuidGenerator.Create(),
                id,
                input.DocumentId.Value);
            await PolicyContractDocumentRepository.InsertAsync(policyContractDoc);
        }
        else
        {
            // When documentId is not passed, remove existing document records
            foreach (var doc in existingDocs)
            {
                await PolicyContractDocumentRepository.DeleteAsync(doc);
            }
        }

        await Manager.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<iOne.PolicyContracts.PolicyContract, PolicyContractDto>(entity);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CancelAsync(id);
    }

    public virtual async Task TerminateContractAsync(TerminateContractInput input)
    {
        if (input == null || input.Policies == null || input.Policies.Count == 0)
        {
            throw new UserFriendlyException(L["PolicyContract:TerminateContractNoPolicies"].Value);
        }

        var entity = await Repository.GetAsync(input.ContractId);

        foreach (var item in input.Policies)
        {
            var terminateInput = new TerminatePolicyInput
            {
                TerminationReasonId = input.TerminationReasonId ?? Guid.Empty,
                TerminationReasonDescription = input.TerminationReasonDescription,
                TerminationDate = input.TerminationDate,
                TotalRefundAmount = item.ActualRefundAmount
            };
            await PolicyAppService.TerminatePolicyAsync(item.PolicyId, terminateInput);
        }

        entity.UpdateStatus(PolicyContractStatus.Terminated);
        entity.UpdateTerminationDate(input.TerminationDate);
        if (input.TerminationReasonId.HasValue)
        {
            entity.UpdateTerminationReasonId(input.TerminationReasonId.Value);
        }
        await Manager.UpdateAsync(entity);
        await CurrentUnitOfWork!.SaveChangesAsync();
    }

    public virtual async Task CancelAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);
        var policyQuery = await PolicyRepository.GetQueryableAsync();
        var versionQuery = await PolicyVersionRepository.GetQueryableAsync();

        // Check 1: Policy header has status Active (approved)
        var hasActiveApprovedPolicy = await AsyncExecuter.AnyAsync(
            policyQuery.Where(p => p.ContractId == id
                && !p.IsDeleted
                && p.Status == PolicyStatus.Active));
        if (hasActiveApprovedPolicy)
        {
            throw new UserFriendlyException(L["PolicyContract:CannotCancelContract"].Value);
        }

        // Check 2: Any policy under this contract has a version that is approved (active)
        var contractPolicyIds = policyQuery
            .Where(p => p.ContractId == id && !p.IsDeleted)
            .Select(p => p.Id);
        var hasActiveVersion = await AsyncExecuter.AnyAsync(
            versionQuery.Where(v => contractPolicyIds.Contains(v.PolicyId)
                && string.Equals(v.Status, "active", StringComparison.OrdinalIgnoreCase)));
        if (hasActiveVersion)
        {
            throw new UserFriendlyException(L["PolicyContract:CannotCancelContract"].Value);
        }

        // Cancel all cancellable policies (e.g. draft policies) under this contract
        var cancellablePolicyIds = await AsyncExecuter.ToListAsync(
            policyQuery
                .Where(p => p.ContractId == id
                    && !p.IsDeleted
                    && p.Status == PolicyStatus.Draft)
                .Select(p => p.Id));

        foreach (var policyId in cancellablePolicyIds)
        {
            await PolicyAppService.CancelPolicyAsync(policyId);
        }

        entity.UpdateStatus(PolicyContractStatus.Cancelled);
        await Manager.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    public virtual async Task<PagedResultDto<PolicyContractSearchResultDto>> SearchAsync(PolicyContractSearchInput input)
    {
        var query = await CreateSearchQueryAsync(input);
        var totalCount = await AsyncExecuter.CountAsync(query);
        query = ApplySortingForSearch(query, input);
        var items = await AsyncExecuter.ToListAsync(
            query.Skip(input.SkipCount).Take(input.MaxResultCount));
        var dtos = items.Select(MapToSearchResultDto).ToList();
        await PopulateCreatorNamesAsync(items, dtos);
        return new PagedResultDto<PolicyContractSearchResultDto>(totalCount, dtos);
    }

    private async Task PopulateCreatorNamesAsync(
        List<iOne.PolicyContracts.PolicyContract> items,
        List<PolicyContractSearchResultDto> dtos)
    {
        var creatorIds = items
            .Select(x => x.CreatorId)
            .Where(id => id.HasValue && id.Value != Guid.Empty)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();
        if (creatorIds.Count == 0) return;

        var userQuery = await UserRepository.GetQueryableAsync();
        var users = await AsyncExecuter.ToListAsync(
            userQuery
                .Where(u => creatorIds.Contains(u.Id))
                .Select(u => new { u.Id, u.UserName, u.Name, u.Surname }));

        var creatorNameById = users
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
                });

        for (var i = 0; i < items.Count; i++)
        {
            var entity = items[i];
            var dto = dtos[i];
            if (entity.CreatorId.HasValue &&
                creatorNameById.TryGetValue(entity.CreatorId.Value, out var creatorName) &&
                !string.IsNullOrWhiteSpace(creatorName))
            {
                dto.CreatorName = creatorName;
            }
        }
    }

    public virtual async Task<byte[]> ExportAsync(PolicyContractSearchInput input)
    {
        var query = await CreateSearchQueryAsync(input);
        query = ApplySortingForSearch(query, input);
        const int exportLimit = 10000;
        var items = await AsyncExecuter.ToListAsync(query.Take(exportLimit));
        return BuildCsvContent(items);
    }

    public virtual async Task<byte[]> ExportExcelAsync(PolicyContractSearchInput input)
    {
        var query = await CreateSearchQueryAsync(input);
        query = ApplySortingForSearch(query, input);
        var items = await AsyncExecuter.ToListAsync(query);
        var dtos = items.Select(MapToSearchResultDto).ToList();
        await PopulateCreatorNamesAsync(items, dtos);

        var lobIds = dtos.Where(d => d.LobId.HasValue).Select(d => d.LobId!.Value).Distinct().ToList();
        var insurerIds = dtos.Where(d => d.InsurerId.HasValue).Select(d => d.InsurerId!.Value).Distinct().ToList();
        var customerIds = dtos.Select(d => d.CustomerId).Distinct().ToList();

        var lobNameById = new Dictionary<Guid, string>();
        if (lobIds.Count > 0)
        {
            var lobs = await ProLineOfBusinessRepository.GetListAsync(x => lobIds.Contains(x.Id));
            lobNameById = lobs.ToDictionary(x => x.Id, x => x.Name ?? "");
        }
        var insurerNameById = new Dictionary<Guid, string>();
        if (insurerIds.Count > 0)
        {
            var partners = await ResPartnerRepository.GetListAsync(x => insurerIds.Contains(x.Id));
            insurerNameById = partners.ToDictionary(x => x.Id, x => x.Name ?? "");
        }
        var customerNameById = new Dictionary<Guid, string>();
        if (customerIds.Count > 0)
        {
            var customers = await ResCustomerRepository.GetListAsync(x => customerIds.Contains(x.Id));
            customerNameById = customers.ToDictionary(x => x.Id, x => x.Name ?? "");
        }

        return BuildExcelContent(dtos, lobNameById, insurerNameById, customerNameById);
    }

    protected virtual byte[] BuildExcelContent(
        List<PolicyContractSearchResultDto> dtos,
        Dictionary<Guid, string> lobNameById,
        Dictionary<Guid, string> insurerNameById,
        Dictionary<Guid, string> customerNameById)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Danh sách hợp đồng");

        var headers = new[]
        {
            "Số HĐ",
            "Tên hợp đồng",
            "Nghiệp vụ BH",
            "Loại HĐ",
            "BH gốc",
            "Khách hàng",
            "Số lượng",
            "SL hiện tại",
            "Ngày hiệu lực",
            "Ngày hết hạn",
            "Trạng thái",
            "Ngày tạo",
            "Người tạo"
        };

        for (var col = 0; col < headers.Length; col++)
        {
            worksheet.Cell(1, col + 1).Value = headers[col];
        }
        worksheet.Row(1).Style.Font.Bold = true;
        worksheet.Row(1).Style.Fill.BackgroundColor = XLColor.LightGray;

        for (var i = 0; i < dtos.Count; i++)
        {
            var d = dtos[i];
            var lobName = d.LobId.HasValue && lobNameById.TryGetValue(d.LobId.Value, out var ln) ? ln : "";
            var insurerName = d.InsurerId.HasValue && insurerNameById.TryGetValue(d.InsurerId.Value, out var iname) ? iname : "";
            var customerName = customerNameById.TryGetValue(d.CustomerId, out var cname) ? cname : "";
            var typeStr = FormatTypeForExport(d.Type);
            var statusStr = FormatStatusForExport(d.Status);

            worksheet.Cell(i + 2, 1).Value = d.Code ?? "";
            worksheet.Cell(i + 2, 2).Value = d.Name ?? "";
            worksheet.Cell(i + 2, 3).Value = lobName;
            worksheet.Cell(i + 2, 4).Value = typeStr;
            worksheet.Cell(i + 2, 5).Value = insurerName;
            worksheet.Cell(i + 2, 6).Value = customerName;
            worksheet.Cell(i + 2, 7).Value = d.Quantity;
            worksheet.Cell(i + 2, 8).Value = d.CurrentQuantity ?? 0;
            worksheet.Cell(i + 2, 9).Value = d.EffectDate ?? "";
            worksheet.Cell(i + 2, 10).Value = d.ExpireDate ?? "";
            worksheet.Cell(i + 2, 11).Value = statusStr;
            worksheet.Cell(i + 2, 12).Value = d.CreationTime ?? "";
            worksheet.Cell(i + 2, 13).Value = d.CreatorName ?? "";
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new System.IO.MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static string FormatTypeForExport(string? type)
    {
        if (string.IsNullOrEmpty(type)) return "";
        var lower = type.ToLowerInvariant();
        return lower == "individual" ? "Cá nhân" : lower == "group" ? "Tập thể" : type;
    }

    private static string FormatStatusForExport(string? status)
    {
        if (string.IsNullOrEmpty(status)) return "";
        var lower = status.ToLowerInvariant();
        return lower switch
        {
            "quotation" => "Báo giá",
            "draft" => "Nháp",
            "active" => "Đang hiệu lực",
            "expired" => "Hết hạn",
            "terminated" => "Đã chấm dứt",
            "cancelled" => "Đã hủy",
            _ => status
        };
    }

    protected virtual async Task<IQueryable<iOne.PolicyContracts.PolicyContract>> CreateSearchQueryAsync(PolicyContractSearchInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();
        query = query
            .Include(c => c.Policies)
            .ThenInclude(p => p.PolicyCertificates)
            .Include(c => c.Employee);

        query = await ApplyPermissionFilterAsync(query);
        query = ApplySearchFilters(query, input);
        return query;
    }

    protected virtual async Task<IQueryable<iOne.PolicyContracts.PolicyContract>> ApplyPermissionFilterAsync(
        IQueryable<iOne.PolicyContracts.PolicyContract> query)
    {
        if (CurrentUser.Id == null)
        {
            return query.Where(_ => false);
        }
        var currentUserId = CurrentUser.Id.Value;
        var currentEmployee = await AsyncExecuter.FirstOrDefaultAsync(
            (await EmployeeRepository.GetQueryableAsync()).Where(e => e.UserId == currentUserId));

        if (currentEmployee == null)
        {
            return query.Where(c => c.CreatorId == currentUserId);
        }

        if (currentEmployee.PartnerId != null)
        {
            if (currentEmployee.IsManager == true)
            {
                return query;
            }
            return query.Where(c => c.EmployeeId == currentEmployee.Id);
        }

        return query.Where(c =>
            c.CreatorId == currentUserId
            || c.EmployeeId == currentEmployee.Id
            || (c.Employee != null && c.Employee.DepartmentId == currentEmployee.DepartmentId));
    }

    protected virtual IQueryable<iOne.PolicyContracts.PolicyContract> ApplySearchFilters(
        IQueryable<iOne.PolicyContracts.PolicyContract> query,
        PolicyContractSearchInput input)
    {
        if (input.CustomerId.HasValue)
        {
            query = query.Where(c => c.CustomerId == input.CustomerId.Value);
        }
        if (!string.IsNullOrWhiteSpace(input.Code))
        {
            var code = input.Code.Trim();
            query = query.Where(c => EF.Functions.ILike(c.Code, $"%{code}%"));
        }
        if (input.Type.HasValue)
        {
            query = query.Where(c => c.Type == input.Type.Value);
        }
        if (input.Status.HasValue)
        {
            query = query.Where(c => c.Status == input.Status.Value);
        }
        if (!string.IsNullOrWhiteSpace(input.CertificateCode))
        {
            var cert = input.CertificateCode.Trim();
            query = query.Where(c => c.Policies.Any(p =>
                p.PolicyCertificates.Any(pc => EF.Functions.ILike(pc.CertificateNo ?? "", $"%{cert}%"))));
        }
        if (!string.IsNullOrWhiteSpace(input.PolicyNo))
        {
            var policyNo = input.PolicyNo.Trim();
            query = query.Where(c => c.Policies.Any(p =>
                EF.Functions.ILike(p.PolicyNo, $"%{policyNo}%")
                || (p.InsurerPolicyNo != null && EF.Functions.ILike(p.InsurerPolicyNo, $"%{policyNo}%"))));
        }
        if (input.SellerId.HasValue)
        {
            query = query.Where(c => c.EmployeeId == input.SellerId.Value);
        }
        if (input.EffectDateFrom.HasValue)
        {
            query = query.Where(c => c.EffectDate >= input.EffectDateFrom.Value);
        }
        if (input.EffectDateTo.HasValue)
        {
            var to = input.EffectDateTo.Value.Date.AddDays(1);
            query = query.Where(c => c.EffectDate < to);
        }
        if (input.ExpireDateFrom.HasValue)
        {
            query = query.Where(c => c.ExpireDate >= input.ExpireDateFrom.Value);
        }
        if (input.ExpireDateTo.HasValue)
        {
            var to = input.ExpireDateTo.Value.Date.AddDays(1);
            query = query.Where(c => c.ExpireDate != null && c.ExpireDate < to);
        }
        if (input.InsurerId.HasValue)
        {
            query = query.Where(c => c.InsurerId == input.InsurerId.Value);
        }
        return query;
    }

    protected static IQueryable<iOne.PolicyContracts.PolicyContract> ApplySortingForSearch(
        IQueryable<iOne.PolicyContracts.PolicyContract> query,
        PolicyContractSearchInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Sorting))
        {
            return query.OrderByDescending(x => x.CreationTime);
        }
        return query.OrderByDescending(x => x.CreationTime);
    }

    protected static PolicyContractSearchResultDto MapToSearchResultDto(iOne.PolicyContracts.PolicyContract c)
    {
        const string dateFormat = "dd/MM/yyyy";
        const string creationFormat = "HH:mm dd/MM/yyyy";
        return new PolicyContractSearchResultDto
        {
            Id = c.Id,
            Code = c.Code,
            Name = c.Name,
            LobId = c.LobId,
            Type = c.Type.ToString().ToLowerInvariant(),
            InsurerId = c.InsurerId,
            CustomerId = c.CustomerId,
            Quantity = c.Quantity,
            CurrentQuantity = c.CurrentQuantity,
            EffectDate = c.EffectDate.ToString(dateFormat),
            ExpireDate = c.ExpireDate?.ToString(dateFormat),
            Status = c.Status.ToString().ToLowerInvariant(),
            CreationTime = c.CreationTime.ToString(creationFormat),
            CreatorId = c.CreatorId
        };
    }

    protected static byte[] BuildCsvContent(List<iOne.PolicyContracts.PolicyContract> items)
    {
        const string dateFormat = "dd/MM/yyyy";
        const string creationFormat = "HH:mm dd/MM/yyyy";
        var sb = new StringBuilder();
        sb.AppendLine("Code,Name,LobId,Type,InsurerId,CustomerId,Quantity,CurrentQuantity,EffectDate,ExpireDate,Status,CreationTime,CreatorId");
        foreach (var c in items)
        {
            sb.AppendLine(string.Join(",",
                EscapeCsv(c.Code),
                EscapeCsv(c.Name),
                c.LobId?.ToString() ?? "",
                EscapeCsv(c.Type.ToString().ToLowerInvariant()),
                c.InsurerId?.ToString() ?? "",
                c.CustomerId.ToString(),
                c.Quantity.ToString(CultureInfo.InvariantCulture),
                c.CurrentQuantity?.ToString(CultureInfo.InvariantCulture) ?? "",
                c.EffectDate.ToString(dateFormat),
                c.ExpireDate?.ToString(dateFormat) ?? "",
                EscapeCsv(c.Status.ToString().ToLowerInvariant()),
                c.CreationTime.ToString(creationFormat),
                c.CreatorId?.ToString() ?? ""));
        }
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }
        return value;
    }

    protected override IQueryable<iOne.PolicyContracts.PolicyContract> ApplySorting(IQueryable<iOne.PolicyContracts.PolicyContract> query, GetPolicyContractsInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Sorting))
        {
            return query.OrderByDescending(x => x.CreationTime);
        }
        return base.ApplySorting(query, input);
    }

    protected override async Task<IQueryable<iOne.PolicyContracts.PolicyContract>> CreateFilteredQueryAsync(GetPolicyContractsInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.Code))
        {
            query = query.Where(x => EF.Functions.ILike(x.Code, $"%{input.Code}%"));
        }

        if (!string.IsNullOrWhiteSpace(input.Name))
        {
            query = query.Where(x => EF.Functions.ILike(x.Name, $"%{input.Name}%"));
        }

        if (input.InsurerId.HasValue)
        {
            query = query.Where(x => x.InsurerId == input.InsurerId.Value);
        }

        if (input.CustomerId.HasValue)
        {
            query = query.Where(x => x.CustomerId == input.CustomerId.Value);
        }

        if (input.LobId.HasValue)
        {
            query = query.Where(x => x.LobId == input.LobId.Value);
        }

        if (input.EmployeeId.HasValue)
        {
            query = query.Where(x => x.EmployeeId == input.EmployeeId.Value);
        }

        if (input.Type.HasValue)
        {
            query = query.Where(x => x.Type == input.Type.Value);
        }

        if (input.Status.HasValue)
        {
            query = query.Where(x => x.Status == input.Status.Value);
        }

        if (input.Statuses != null && input.Statuses.Length > 0)
        {
            query = query.Where(x => input.Statuses.Contains(x.Status));
        }

        return query;
    }
}
