using System.Threading;
using System.Threading.Tasks;

namespace iOne.WebviewAuth;

public interface IPartnerWebviewTokenValidator
{
    Task<PartnerWebviewTokenValidationResult> ValidateAsync(string partnerCode, string partnerToken, CancellationToken cancellationToken = default);
    Task<object> SendOrderResultAsync(SendOrderResultInputDto input, CancellationToken cancellationToken = default);
}
