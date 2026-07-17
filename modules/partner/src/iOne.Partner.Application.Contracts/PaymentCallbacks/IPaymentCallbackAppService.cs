using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Partner.PaymentCallbacks;

public interface IPaymentCallbackAppService : IApplicationService
{
    Task<PaymentCallbackResponseDto> ProcessCallbackAsync(PaymentCallbackRequestDto input);
}
