using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace iOne.Claim.Claims;

public interface IClaimFolderAppService : IApplicationService
{
    Task<PagedResultDto<ClaimFolderListDto>> GetListAsync(GetClaimFoldersInput input);

    Task<ClaimFolderDto> CreateAsync(CreateClaimFolderDto input);

    Task<ClaimFolderDto> GetAsync(Guid id);
}

