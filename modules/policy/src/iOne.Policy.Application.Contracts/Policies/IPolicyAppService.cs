using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using iOne.Policy.Payments;
using iOne.Policy.PolicyContracts;
using Volo.Abp.Application.Services;

namespace iOne.Policy.Policies;

public interface IPolicyAppService : ICrudAppService<
    PolicyDto,
    Guid,
    GetPoliciesInput,
    CreatePolicyDto,
    UpdatePolicyDetailDto>
{
    /// <summary>
    /// Gets a policy by id. When versionId is provided, detail is returned for that version; otherwise the current (LastVersionId) version is used.
    /// </summary>
    Task<PolicyDto> GetAsync(Guid id, Guid? versionId = null);

    Task<List<Dictionary<string, string>>> ExtractAttributeParametersAsync(ExtractPolicyAttributeParametersInputDto input);

    /// <summary>
    /// Gets attribute required spec for form generation: parses DataPath to parameter name and returns IsRequired from ProProductAttribute.
    /// </summary>
    Task<GetAttributeRequiredSpecResultDto> GetAttributeRequiredSpecAsync(ExtractPolicyAttributeParametersInputDto input);

    /// <summary>
    /// Calculates refund amounts for policy termination using the product's rule script.
    /// </summary>
    Task<CalculateRefundAmountResultDto> CalculateRefundAmountAsync(CalculateRefundAmountInput input);

    /// <summary>
    /// Calculates refund amounts for policy termination across multiple products in a single request.
    /// Returns the summed total refund and all per-coverage refund amounts.
    /// </summary>
    Task<CalculateRefundAmountResultDto> CalculateRefundAmountBatchAsync(CalculateRefundAmountBatchInput input);

    /// <summary>
    /// Distributes a total refund amount across coverages by premium ratio (same logic as used in batch calculation).
    /// </summary>
    Task<CalculateRefundAmountResultDto> DistributeRefundToCoveragesAsync(DistributeRefundInput input);

    /// <summary>
    /// Terminates a policy by triggering the Elsa termination workflow (no PolicyAmount or status update here).
    /// </summary>
    Task TerminatePolicyAsync(Guid id, TerminatePolicyInput input);

    /// <summary>
    /// Creates a PolicyAmount record for policy termination (fee item TERMINATE_REFUND_AMOUNT).
    /// Called by the termination workflow; does not update policy status.
    /// </summary>
    Task CreateTerminatePolicyAmountAsync(CreateTerminatePolicyAmountInput input);

    /// <summary>
    /// Gets policies for contract termination modal (active policies with refund amounts).
    /// </summary>
    Task<List<ContractTerminationPolicyDto>> GetContractTerminationPoliciesAsync(Guid contractId);

    /// <summary>
    /// Cancels a draft policy by setting status to Cancelled.
    /// </summary>
    Task CancelPolicyAsync(Guid id);

    /// <summary>
    /// Submits a draft policy for approval by triggering the Elsa create-policy workflow.
    /// </summary>
    /// <param name="id">Policy id.</param>
    /// <param name="input">Optional; ApproverId (HrEmployee id) or empty/null for auto-determine.</param>
    Task SubmitForApprovalAsync(Guid id, SubmitForApprovalInput? input = null);

    /// <summary>
    /// Creates a draft policy and then submits it for approval.
    /// If submit fails, created draft policy is still kept.
    /// </summary>
    Task<CreateAndSubmitPolicyResultDto> CreateAndSubmitForApprovalAsync(CreatePolicyDto input);

    /// <summary>
    /// Renews an existing policy by cloning it into a new Draft policy.
    /// EffectiveDate = today, ExpireDate = today + 1 year, IssueDate = today.
    /// Does NOT clone any documents and creates a new contract.
    /// </summary>
    Task<PolicyDto> RenewPolicyAsync(Guid id);

    /// <summary>
    /// Lấy cấu hình thanh toán cho đơn bảo hiểm: payment methods, amount to pay, payment types.
    /// <paramref name="isPaymentOnline"/>: tính tổng phải thu gồm policy_amount thuộc phiên bản Active và Draft.
    /// </summary>
    Task<PaymentConfigDto> GetPaymentConfigAsync(Guid policyId, bool isPaymentOnline = false);

    /// <summary>
    /// Tạo đề nghị thanh toán cho đơn bảo hiểm.
    /// </summary>
    Task<AccountPaymentRequestDto> CreatePaymentRequestAsync(Guid policyId, CreatePaymentRequestInput input);

    /// <summary>
    /// Tạo đề nghị thanh toán từ IPN, không yêu cầu xác thực người dùng.
    /// </summary>
    Task<AccountPaymentRequestDto> CreatePaymentRequestIpnAsync(Guid policyId, CreatePaymentRequestInput input);

    /// <summary>
    /// Đánh dấu <c>policy_amount.payment_status = paid</c> cho mọi dòng phí có <paramref name="policyId"/> (luồng callback thanh toán đối tác).
    /// </summary>
    Task MarkPolicyAmountsAsPaidAsync(Guid policyId);

    /// <summary>
    /// Xuất file mẫu Excel với dữ liệu các đơn bảo hiểm còn nợ thanh toán.
    /// <paramref name="lobId"/> tùy chọn: khi có, khớp LOB ngầm trên màn tìm kiếm đơn (DEFAULT_LOB / DEFAULT_LOB_MOTORBIKE).
    /// Phạm vi xem đơn luôn áp dụng như danh sách đơn (seller/implementer/đối tác/phòng ban).
    /// </summary>
    Task<byte[]> ExportPaymentTemplateAsync(Guid? lobId = null, bool isPaymentOnline = false);

    /// <summary>
    /// Import cập nhật thanh toán từ file Excel.
    /// Cột: STT, Mã đơn, Tên khách hàng, Số tiền cần thanh toán, Số tiền thanh toán, Hình thức thanh toán, Ngày thanh toán.
    /// Chỉ xử lý các dòng có đủ giá trị; bỏ qua dòng thiếu dữ liệu.
    /// </summary>
    Task<ImportPaymentExcelResultDto> ImportPaymentExcelAsync(byte[] fileBytes);

    /// <summary>
    /// Xuất file Excel mẫu để import đơn bảo hiểm.
    /// Nếu contractId có: load hợp đồng, điền sẵn theo HĐ.
    /// Nếu không có contractId nhưng có insurerId: dùng đối tác BH + LOB mặc định mã CAR để lấy sản phẩm và điền Đối tác BH gốc, Nghiệp vụ BH vào dòng 2.
    /// </summary>
    Task<byte[]> ExportPolicyImportTemplateAsync(Guid? contractId = null, Guid? insurerId = null, Guid? productTypeId = null);

    /// <summary>
    /// Import đơn bảo hiểm từ file Excel.
    /// contractId optional: có thì gắn đơn vào hợp đồng đó; không có thì mỗi dòng tạo hợp đồng mới (tự sinh) rồi tạo đơn qua CreateAsync.
    /// insurerId optional: đối tác BH gốc mong đợi; nếu có (từ contract hoặc từ UI), mỗi dòng Excel phải có Đối tác BH gốc trùng với insurerId, nếu không sẽ báo lỗi.
    /// productTypeId optional: nếu có, chỉ nhận diện cột sản phẩm thuộc loại sản phẩm đó (khớp file mẫu đã lọc theo loại).
    /// </summary>
    Task<ImportPolicyExcelResultDto> ImportPolicyExcelAsync(Guid? contractId, Guid? insurerId, byte[] fileBytes, Guid? productTypeId = null);

    /// <summary>
    /// Endorses a policy with the provided input. Similar to update, performs upsert operations on policy data.
    /// </summary>
    Task<PolicyDto> EndorsementAsync(Guid id, UpdatePolicyDetailDto input);

    /// <summary>
    /// Gets work task history for a policy version (work_task where business_name='policyVersion' and business_key=policyVersionId), ordered by CreationTime descending.
    /// </summary>
    Task<List<PolicyWorkTaskHistoryItemDto>> GetWorkTaskHistoryAsync(Guid policyVersionId);

    Task<UpdatePrevEffectDateResultDto> UpdatePrevEffectDateAsync(UpdatePrevEffectDateInputDto input);

    /// <summary>
    /// Danh sách loại xe cho cấp đơn xe máy: InsurerDictionary (business_name = RES_MORTOR_TYPE), theo đối tác BH và hiệu lực.
    /// </summary>
    Task<List<MotorbikeInsurerVehicleTypeOptionDto>> GetMotorbikeVehicleTypeOptionsAsync(Guid insurerId);
}
