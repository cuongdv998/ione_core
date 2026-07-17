using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace iOne.Claim.Claims;

public interface IClaimSentMessageAppService : IApplicationService
{
    Task<PagedResultDto<ClaimSentMessageDto>> GetListAsync(GetClaimSentMessagesInput input);
}

