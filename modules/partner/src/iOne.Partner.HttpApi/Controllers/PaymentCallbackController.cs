using System.Threading.Tasks;
using iOne.Partner.PaymentCallbacks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;

namespace iOne.Partner.Controllers;

[RemoteService(Name = "Partner")]
[Area("partner")]
[Route("partner/v1/policy/payment")]
public class PaymentCallbackController : AbpControllerBase, IPaymentCallbackAppService
{
    private readonly IPaymentCallbackAppService _paymentCallbackAppService;

    public PaymentCallbackController(IPaymentCallbackAppService paymentCallbackAppService)
    {
        _paymentCallbackAppService = paymentCallbackAppService;
    }

    [HttpPost("callback")]
    [Authorize(AuthenticationSchemes = "ApiKey")]
    public virtual Task<PaymentCallbackResponseDto> ProcessCallbackAsync(PaymentCallbackRequestDto input)
    {
        return _paymentCallbackAppService.ProcessCallbackAsync(input);
    }
}
