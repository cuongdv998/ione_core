using System;
using System.Threading;
using System.Threading.Tasks;

namespace iOne.WebviewAuth;

public interface IWebviewAccessTokenIssuer
{
    Task<WebviewAuthResultDto> IssueAsync(Guid identityUserId, CancellationToken cancellationToken = default);
}
