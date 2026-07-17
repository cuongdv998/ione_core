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
using iOne.Master.ResObjectTypeItems;

namespace iOne.Master.Controllers;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Area(MasterRemoteServiceConsts.ModuleName)]
[Route("api/master/object-type-items")]
//[Authorize]
public class ResObjectTypeItemController : AbpControllerBase
{
    protected IResObjectTypeItemAppService AppService { get; }

    public ResObjectTypeItemController(IResObjectTypeItemAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(ResObjectTypeItemPermissions.View)]
    public virtual Task<PagedResultDto<ResObjectTypeItemDto>> GetListAsync(GetResObjectTypeItemsInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(ResObjectTypeItemPermissions.View)]
    public virtual Task<ResObjectTypeItemDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(ResObjectTypeItemPermissions.Create)]
    public virtual Task<ResObjectTypeItemDto> CreateAsync(CreateResObjectTypeItemDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(ResObjectTypeItemPermissions.Edit)]
    public virtual Task<ResObjectTypeItemDto> UpdateAsync(Guid id, UpdateResObjectTypeItemDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(ResObjectTypeItemPermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }

    [HttpPost("import-excel")]
    [Authorize(ResObjectTypeItemPermissions.Create)]
    public virtual async Task<ImportResObjectTypeItemResultDto> ImportExcelAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw new UserFriendlyException("File là bắt buộc buộc");
        }

        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) &&
            !file.FileName.EndsWith(".xls", StringComparison.OrdinalIgnoreCase))
        {
            throw new UserFriendlyException("Chỉ hỗ trợ định dạng (.xlsx, .xls)");
        }

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var fileBytes = memoryStream.ToArray();

        return await AppService.ImportExcelAsync(fileBytes);
    }

    [HttpGet("export-template")]
    [Authorize(ResObjectTypeItemPermissions.Create)]
    public virtual async Task<IActionResult> ExportTemplateAsync()
    {
        var fileBytes = await AppService.ExportTemplateAsync();
        var fileName = $"ResObjectTypeItem_Template_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
        
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
}
