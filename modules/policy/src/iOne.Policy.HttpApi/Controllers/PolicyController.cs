using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Policy.Permissions;
using iOne.Policy.Policies;
using iOne.Policy.PolicyContracts;

namespace iOne.Policy.Controllers;

[RemoteService(Name = PolicyRemoteServiceConsts.RemoteServiceName)]
[Area(PolicyRemoteServiceConsts.ModuleName)]
[Route("api/policy/policies")]
public class PolicyController : AbpControllerBase
{
    protected IPolicyAppService AppService { get; }
    protected IPolicyClaimLookupAppService ClaimLookupAppService { get; }
    protected IPolicyWorkflowStatusAppService PolicyWorkflowStatusAppService { get; }

    public PolicyController(
        IPolicyAppService appService,
        IPolicyClaimLookupAppService claimLookupAppService,
        IPolicyWorkflowStatusAppService policyWorkflowStatusAppService)
    {
        AppService = appService;
        ClaimLookupAppService = claimLookupAppService;
        PolicyWorkflowStatusAppService = policyWorkflowStatusAppService;
    }

    [HttpGet]
    [Authorize(PolicyPermissions.View)]
    public virtual Task<PagedResultDto<PolicyDto>> GetListAsync(GetPoliciesInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(PolicyPermissions.View)]
    public virtual Task<PolicyDto> GetAsync(Guid id, [FromQuery] Guid? versionId = null)
    {
        return AppService.GetAsync(id, versionId);
    }

    /// <summary>
    /// Gets work task history for a policy version (work_task where business_name='policyVersion' and business_key=policyVersionId), ordered by CreationTime descending.
    /// </summary>
    [HttpGet("work-task-history/{policyVersionId}")]
    [Authorize(PolicyPermissions.View)]
    public virtual Task<List<PolicyWorkTaskHistoryItemDto>> GetWorkTaskHistoryAsync(Guid policyVersionId)
    {
        return AppService.GetWorkTaskHistoryAsync(policyVersionId);
    }

    // Claim create: lookup policies by GCN or vehicle info (login only)
    [HttpGet("claim-lookup")]
    [Authorize]
    public virtual Task<List<PolicyClaimLookupDto>> GetClaimLookupAsync(GetPolicyClaimLookupInput input)
    {
        return ClaimLookupAppService.GetClaimLookupAsync(input);
    }

    [HttpPost]
    [Authorize(PolicyPermissions.Create)]
    public virtual Task<PolicyDto> CreateAsync(CreatePolicyDto input)
    {
        return AppService.CreateAsync(input);
    }

    /// <summary>
    /// Creates a draft policy and then submits it for approval.
    /// If submit fails, created draft policy is still kept.
    /// </summary>
    [HttpPost("create-and-submit")]
    [Authorize(PolicyPermissions.Create)]
    public virtual Task<CreateAndSubmitPolicyResultDto> CreateAndSubmitForApprovalAsync(CreatePolicyDto input)
    {
        return AppService.CreateAndSubmitForApprovalAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(PolicyPermissions.Edit)]
    public virtual Task<PolicyDto> UpdateAsync(Guid id, UpdatePolicyDetailDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpPost("{id}/endorsement")]
    [Authorize(PolicyPermissions.Edit)]
    public virtual Task<PolicyDto> EndorsementAsync(Guid id, UpdatePolicyDetailDto input)
    {
        return AppService.EndorsementAsync(id, input);
    }

    /// <summary>
    /// Extracts attribute parameter values from a Policy-like payload.
    /// Returns a list containing a single map (code -> value).
    /// </summary>
    [HttpPost("extract-attribute-parameters")]
    [Authorize]
    public virtual async Task<List<object>> ExtractAttributeParametersAsync(
        ExtractPolicyAttributeParametersInputDto input)
    {
        // NOTE:
        // - The actual business return type is List<Dictionary<string,string>>
        // - We expose List<object> here to keep ABP Angular proxy generation stable
        //   (Dictionary<> return types can generate invalid TS like Record<[string,string]>).
        var dicts = await AppService.ExtractAttributeParametersAsync(input);
        var result = new List<object>(dicts.Count);
        foreach (var d in dicts)
        {
            result.Add(d);
        }
        return result;
    }

    /// <summary>
    /// Gets attribute required spec for the product in the request (same body as extract-attribute-parameters).
    /// Returns for each attribute: Code, ParameterName (from DataPath), Name, IsRequired (from ProProductAttribute).
    /// </summary>
    [HttpPost("get-attribute-required-spec")]
    [Authorize]
    public virtual async Task<GetAttributeRequiredSpecResultDto> GetAttributeRequiredSpecAsync(
        ExtractPolicyAttributeParametersInputDto input)
    {
        return await AppService.GetAttributeRequiredSpecAsync(input);
    }

    /// <summary>
    /// Loại xe (xe máy): lọc InsurerDictionary business_name = RES_MORTOR_TYPE, insurer_id, active, hiệu lực.
    /// </summary>
    [HttpGet("motorbike-vehicle-type-options")]
    [Authorize(PolicyPermissions.View)]
    public virtual Task<List<MotorbikeInsurerVehicleTypeOptionDto>> GetMotorbikeVehicleTypeOptionsAsync(
        [FromQuery] Guid insurerId)
    {
        return AppService.GetMotorbikeVehicleTypeOptionsAsync(insurerId);
    }

    [HttpDelete("{id}")]
    [Authorize(PolicyPermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }

    /// <summary>
    /// Gets policies for contract termination modal (active policies with refund amounts).
    /// </summary>
    [HttpGet("contract-termination-policies")]
    [Authorize(PolicyPermissions.View)]
    public virtual Task<List<ContractTerminationPolicyDto>> GetContractTerminationPoliciesAsync([FromQuery] Guid contractId)
    {
        return AppService.GetContractTerminationPoliciesAsync(contractId);
    }

    /// <summary>
    /// Calculates refund amounts for policy termination using the product's rule script.
    /// </summary>
    [HttpPost("calculate-refund-amount")]
    [Authorize(PolicyPermissions.Edit)]
    public virtual Task<CalculateRefundAmountResultDto> CalculateRefundAmountAsync(CalculateRefundAmountInput input)
    {
        return AppService.CalculateRefundAmountAsync(input);
    }

    /// <summary>
    /// Calculates refund amounts for policy termination across multiple products in a single request.
    /// </summary>
    [HttpPost("calculate-refund-amount-batch")]
    [Authorize(PolicyPermissions.Edit)]
    public virtual Task<CalculateRefundAmountResultDto> CalculateRefundAmountBatchAsync(
        CalculateRefundAmountBatchInput input)
    {
        return AppService.CalculateRefundAmountBatchAsync(input);
    }

    /// <summary>
    /// Distributes a total refund amount across coverages by premium ratio.
    /// </summary>
    [HttpPost("distribute-refund")]
    [Authorize(PolicyPermissions.Edit)]
    public virtual Task<CalculateRefundAmountResultDto> DistributeRefundToCoveragesAsync(DistributeRefundInput input)
    {
        return AppService.DistributeRefundToCoveragesAsync(input);
    }

    /// <summary>
    /// Terminates a policy, saving refund amounts and updating status.
    /// </summary>
    [HttpPost("{id}/terminate")]
    [Authorize(PolicyPermissions.Edit)]
    public virtual Task TerminatePolicyAsync(Guid id, TerminatePolicyInput input)
    {
        return AppService.TerminatePolicyAsync(id, input);
    }

    /// <summary>
    /// Cancels a draft policy by setting status to Cancelled.
    /// </summary>
    [HttpPost("{id}/cancel")]
    [Authorize(PolicyPermissions.Edit)]
    public virtual Task CancelPolicyAsync(Guid id)
    {
        return AppService.CancelPolicyAsync(id);
    }

    /// <summary>
    /// Submits a draft policy for approval by triggering the Elsa create-policy workflow.
    /// </summary>
    [HttpPost("{id}/submit-for-approval")]
    [Authorize(PolicyPermissions.Edit)]
    public virtual Task SubmitForApprovalAsync(Guid id, [FromBody] SubmitForApprovalInput? input = null)
    {
        return AppService.SubmitForApprovalAsync(id, input);
    }

    /// <summary>
    /// Updates the termination status of a policy version (approve or reject).
    /// </summary>
    [HttpPost("update-termination-status")]
    [Authorize(PolicyPermissions.Edit)]
    public virtual Task UpdateTerminationStatusAsync(UpdateTerminationStatusInput input)
    {
        return PolicyWorkflowStatusAppService.UpdateTerminationStatusAsync(input);
    }

    /// <summary>
    /// Renews an existing policy by cloning it into a new Draft policy.
    /// EffectiveDate = today, ExpireDate = today + 1 year, IssueDate = today.
    /// Does NOT clone any documents and creates a new contract.
    /// </summary>
    [HttpPost("{id}/renew")]
    [Authorize(PolicyPermissions.Create)]
    public virtual Task<PolicyDto> RenewPolicyAsync(Guid id)
    {
        return AppService.RenewPolicyAsync(id);
    }

    /// <summary>
    /// Tải file Excel mẫu để import đơn bảo hiểm.
    /// Cột cố định giữ nguyên, cột sản phẩm tạo động theo danh sách ProProduct active.
    /// </summary>
    [HttpGet("import-template")]
    [Authorize(PolicyPermissions.View)]
    public virtual async Task<IActionResult> ExportPolicyImportTemplateAsync([FromQuery] Guid? contractId = null, [FromQuery] Guid? insurerId = null, [FromQuery] Guid? productTypeId = null)
    {
        var fileBytes = await AppService.ExportPolicyImportTemplateAsync(contractId, insurerId, productTypeId);
        var fileName = $"Policy_Import_Template_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    /// <summary>
    /// Import đơn bảo hiểm từ file Excel vào hợp đồng.
    /// Mỗi dòng Excel = 1 đơn bảo hiểm mới.
    /// Hợp đồng tra cứu theo cột Số HĐ trên từng dòng; cột Đối tác BH gốc phải trùng insurer của HĐ đó.
    /// Nếu có contractId (ngữ cảnh import), HĐ tra được từ Số HĐ phải khớp contractId.
    /// Tham số query insurerId giữ để tương thích API, không dùng để kiểm tra so khớp đối tác.
    /// </summary>
    [HttpPost("import-excel")]
    [Authorize(PolicyPermissions.Create)]
    public virtual async Task<ImportPolicyExcelResultDto> ImportPolicyExcelAsync(
        [FromQuery] Guid? contractId,
        [FromQuery] Guid? insurerId,
        [FromQuery] Guid? productTypeId,
        IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new UserFriendlyException("File is required");

        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) &&
            !file.FileName.EndsWith(".xls", StringComparison.OrdinalIgnoreCase))
            throw new UserFriendlyException("Only Excel files (.xlsx, .xls) are allowed");

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var fileBytes = memoryStream.ToArray();

        return await AppService.ImportPolicyExcelAsync(contractId, insurerId, fileBytes, productTypeId);
    }

    /// <summary>
    /// Upload ảnh OCR (đăng ký hoặc đăng kiểm), trả về biển số xe, số khung, số máy.
    /// Hiện tại response cố định để tích hợp; sau có thể thay bằng OCR thật.
    /// </summary>
    [HttpPost("ocr-image")]
    [Authorize(PolicyPermissions.View)]
    public virtual Task<OcrImageResultDto> UploadOcrImageAsync(
        IFormFile file,
        [FromQuery] string type)
    {
        if (file == null || file.Length == 0)
            throw new UserFriendlyException("File is required");

        // type: "registration" (ảnh đăng ký) hoặc "inspection" (ảnh đăng kiểm)
        // Response cố định theo yêu cầu
        var result = new OcrImageResultDto
        {
            VehiclePlate = "30H12345",
            ChassisNumber = "1HGCM82633A472915",
            EngineNumber = "4G63T-9K28475"
        };
        return Task.FromResult(result);
    }
}
