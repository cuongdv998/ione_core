using System.Threading.Tasks;
using iOne.Payment.Payments;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace iOne.Payment.Controllers;

[Route("api/payment")]
public class PaymentController : AbpControllerBase
{
    private readonly IPaymentAppService _paymentAppService;

    public PaymentController(IPaymentAppService paymentAppService)
    {
        _paymentAppService = paymentAppService;
    }

    [HttpPost("payment-inquiry-by-id")]
    public async Task<IActionResult> PaymentInquiryById([FromBody] PaymentInquiryByIdInput input)
    {
        await _paymentAppService.ProcessPaymentInquiryByIdAsync(input.PaymentId);
        return Ok();
    }
}
