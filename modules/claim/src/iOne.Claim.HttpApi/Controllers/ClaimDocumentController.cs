using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Claim.Claims;
using Volo.Abp;

namespace iOne.Claim.Controllers;

[RemoteService(Name = ClaimRemoteServiceConsts.RemoteServiceName)]
[Area(ClaimRemoteServiceConsts.ModuleName)]
[Route("api/claim/claim-documents")]
public class ClaimDocumentController : AbpControllerBase
{
    protected IClaimDocumentAppService AppService { get; }

    public ClaimDocumentController(IClaimDocumentAppService appService)
    {
        AppService = appService;
    }

    [HttpGet("list")]
    public virtual Task<PagedResultDto<ClaimDocumentDto>> GetListAsync([FromQuery] GetClaimDocumentsInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("onsite-images")]
    public virtual Task<List<ClaimDocumentDto>> GetOnsiteImagesAsync([FromQuery] GetClaimOnsiteImagesInput input)
    {
        return AppService.GetOnsiteImagesAsync(input);
    }
}
