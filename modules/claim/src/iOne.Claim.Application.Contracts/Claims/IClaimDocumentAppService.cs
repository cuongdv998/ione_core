using System.Threading.Tasks;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace iOne.Claim.Claims;

public interface IClaimDocumentAppService : IApplicationService
{
    Task<PagedResultDto<ClaimDocumentDto>> GetListAsync(GetClaimDocumentsInput input);

    Task<List<ClaimDocumentDto>> GetOnsiteImagesAsync(GetClaimOnsiteImagesInput input);
}
