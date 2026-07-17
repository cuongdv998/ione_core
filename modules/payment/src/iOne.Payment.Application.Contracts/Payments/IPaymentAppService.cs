using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Payment.Payments;

public interface IPaymentAppService : IApplicationService
{
    Task<MotorbikePaymentInquiryResultDto> ProcessPaymentInquiryAsync();
    Task ProcessPaymentInquiryByIdAsync(Guid paymentId);
}
