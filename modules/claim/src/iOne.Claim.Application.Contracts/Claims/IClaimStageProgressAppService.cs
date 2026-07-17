using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace iOne.Claim.Claims;

public interface IClaimStageProgressAppService : IApplicationService
{
    Task<PagedResultDto<ClaimStageProgressDto>> GetListAsync(GetClaimStageProgressInput input);
}

