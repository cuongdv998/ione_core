using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Master.Permissions;
using iOne.Master.ResCarGroups;

namespace iOne.Master.Controllers;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Area(MasterRemoteServiceConsts.ModuleName)]
[Route("api/master/car-groups")]
//[Authorize]
public class ResCarGroupController : AbpControllerBase
{
    protected IResCarGroupAppService AppService { get; }

    public ResCarGroupController(IResCarGroupAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(ResCarGroupPermissions.View)]
    public virtual Task<PagedResultDto<ResCarGroupDto>> GetListAsync(GetResCarGroupsInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("lookup-list")]
    public virtual Task<ListResultDto<ResCarGroupDto>> GetLookupListAsync()
    {
        return AppService.GetLookupListAsync();
    }

    [HttpGet("{id}")]
    [Authorize(ResCarGroupPermissions.View)]
    public virtual Task<ResCarGroupDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(ResCarGroupPermissions.Create)]
    public virtual Task<ResCarGroupDto> CreateAsync(CreateResCarGroupDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(ResCarGroupPermissions.Edit)]
    public virtual Task<ResCarGroupDto> UpdateAsync(Guid id, UpdateResCarGroupDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(ResCarGroupPermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }

    [HttpPost("import-excel")]
    [Authorize(ResCarGroupPermissions.Create)]
    public virtual async Task<ImportResCarGroupResultDto> ImportExcelAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw new UserFriendlyException("File là bắt buộc buộc");
        }

        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) &&
            !file.FileName.EndsWith(".xls", StringComparison.OrdinalIgnoreCase))
        {
            throw new UserFriendlyException("Chỉ hỗ trợ định dạng (.xlsx, .xls)");
        }

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var fileBytes = memoryStream.ToArray();

        return await AppService.ImportExcelAsync(fileBytes);
    }

    [HttpGet("export-template")]
    [Authorize(ResCarGroupPermissions.Create)]
    public virtual async Task<IActionResult> ExportTemplateAsync()
    {
        var fileBytes = await AppService.ExportTemplateAsync();
        var fileName = $"ResCarGroup_Template_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
        
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
}
