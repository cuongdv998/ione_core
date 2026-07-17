using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Payment.VnPay;

public interface IVnPayAppService : IApplicationService
{
    Task<string> CreatePaymentUrlAsync(CreateVnPayRequestDto input);
    Task<VnPayReturnDto> HandleReturnAsync(IDictionary<string, string> queryParameters);
    Task<VnPayReturnDto> HandleIpnAsync(IDictionary<string, string> queryParameters);
    Task<VnPayQueryDrResponseDto> QueryTransactionAsync(VnPayQueryDrRequestDto input);
}
