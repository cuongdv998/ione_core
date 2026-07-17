using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Policy.Payments;
using iOne.Policy.Permissions;
using iOne.Policy.Policies;

namespace iOne.Policy.Controllers;

[RemoteService(Name = PolicyRemoteServiceConsts.RemoteServiceName)]
[Area(PolicyRemoteServiceConsts.ModuleName)]
[Route("api/payment")]
public class PaymentController : AbpControllerBase
{
    protected IPolicyAppService PolicyAppService { get; }

    public PaymentController(IPolicyAppService policyAppService)
    {
        PolicyAppService = policyAppService;
    }

    /// <summary>
    /// Lấy cấu hình thanh toán: payment methods, amount to pay, payment types.
    /// </summary>
    [HttpGet("config")]
    [Authorize(PolicyPermissions.View)]
    public virtual Task<PaymentConfigDto> GetConfigAsync([FromQuery] Guid policyId, [FromQuery] bool isPaymentOnline = false)
    {
        return PolicyAppService.GetPaymentConfigAsync(policyId, isPaymentOnline);
    }

    /// <summary>
    /// Tạo đề nghị thanh toán cho đơn bảo hiểm.
    /// </summary>
    [HttpPost("create")]
    [Authorize(PolicyPermissions.Edit)]
    public virtual Task<AccountPaymentRequestDto> CreateAsync([FromQuery] Guid policyId, [FromBody] CreatePaymentRequestInput input)
    {
        return PolicyAppService.CreatePaymentRequestAsync(policyId, input);
    }

    /// <summary>
    /// Xuất file mẫu Excel với dữ liệu các đơn bảo hiểm còn nợ thanh toán.
    /// </summary>
    [HttpGet("export-template")]
    [Authorize(PolicyPermissions.View)]
    public virtual async Task<IActionResult> ExportTemplateAsync([FromQuery] Guid? lobId = null, [FromQuery] bool isPaymentOnline = false)
    {
        var fileBytes = await PolicyAppService.ExportPaymentTemplateAsync(lobId, isPaymentOnline);
        var fileName = $"Payment_Update_Template_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    /// <summary>
    /// Import cập nhật thanh toán từ file Excel.
    /// </summary>
    [HttpPost("import-excel")]
    [Authorize(PolicyPermissions.Edit)]
    public virtual async Task<ImportPaymentExcelResultDto> ImportExcelAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new UserFriendlyException("File is required");

        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) &&
            !file.FileName.EndsWith(".xls", StringComparison.OrdinalIgnoreCase))
            throw new UserFriendlyException("Only Excel files (.xlsx, .xls) are allowed");

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var fileBytes = memoryStream.ToArray();

        return await PolicyAppService.ImportPaymentExcelAsync(fileBytes);
    }
}
