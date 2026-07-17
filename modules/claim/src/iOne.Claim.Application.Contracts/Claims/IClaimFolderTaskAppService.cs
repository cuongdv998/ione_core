using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace iOne.Claim.Claims;

public interface IClaimFolderTaskAppService : IApplicationService
{
    Task<PagedResultDto<ClaimTaskDto>> GetListAsync(GetClaimTasksInput input);
}
