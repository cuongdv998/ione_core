using System.Threading.Tasks;
using iOne.Payment.Payments;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace iOne.Payment.Controllers;

[Route("api/motor_bike/payment")]
public class MotorbikePaymentController : AbpControllerBase
{
    private readonly IPaymentAppService _paymentAppService;

    public MotorbikePaymentController(IPaymentAppService paymentAppService)
    {
        _paymentAppService = paymentAppService;
    }

    [HttpGet("payment-inquiry")]
    public async Task<MotorbikePaymentInquiryResultDto> PaymentInquiryAsync()
    {
        return await _paymentAppService.ProcessPaymentInquiryAsync();
    }
}
