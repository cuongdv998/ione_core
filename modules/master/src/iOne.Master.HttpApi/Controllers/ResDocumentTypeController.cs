using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Master;
using iOne.Master.Permissions;
using iOne.Master.ResDocumentTypes;

namespace iOne.Master.Controllers;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Area(MasterRemoteServiceConsts.ModuleName)]
[Route("api/master/document-types")]
[Authorize]
public class ResDocumentTypeController : AbpControllerBase
{
    protected IResDocumentTypeAppService AppService { get; }

    public ResDocumentTypeController(IResDocumentTypeAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(ResDocumentTypePermissions.View)]
    public virtual Task<PagedResultDto<ResDocumentTypeDto>> GetListAsync(GetResDocumentTypesInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(ResDocumentTypePermissions.View)]
    public virtual Task<ResDocumentTypeDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpGet("/code/{code}")]
    [Authorize(ResDocumentTypePermissions.View)]
    public virtual Task<ResDocumentTypeDto> GetByCode(string code)
    {
        return AppService.GetByCodeAsync(code);
    }

    [HttpGet("by-document-group/{documentGroupCode}")]
    [Authorize(ResDocumentTypePermissions.View)]
    public virtual Task<List<ResDocumentTypeDto>> GetListByDocumentGroupCodeAsync(string documentGroupCode)
    {
        return AppService.GetListByDocumentGroupCodeAsync(documentGroupCode);
    }

    [HttpPost]
    [Authorize(ResDocumentTypePermissions.Create)]
    public virtual Task<ResDocumentTypeDto> CreateAsync(CreateResDocumentTypeDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(ResDocumentTypePermissions.Edit)]
    public virtual Task<ResDocumentTypeDto> UpdateAsync(Guid id, UpdateResDocumentTypeDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(ResDocumentTypePermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }
}
