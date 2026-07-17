using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Master.Permissions;
using iOne.Master.InsurerDictionaries;

namespace iOne.Master.Controllers;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Area(MasterRemoteServiceConsts.ModuleName)]
[Route("api/master/insurer-dictionaries")]
//[Authorize]
public class InsurerDictionaryController : AbpControllerBase
{
    protected IInsurerDictionaryAppService AppService { get; }

    public InsurerDictionaryController(IInsurerDictionaryAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(InsurerDictionaryPermissions.View)]
    public virtual Task<PagedResultDto<InsurerDictionaryDto>> GetListAsync(GetInsurerDictionariesInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(InsurerDictionaryPermissions.View)]
    public virtual Task<InsurerDictionaryDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(InsurerDictionaryPermissions.Create)]
    public virtual Task<InsurerDictionaryDto> CreateAsync(CreateInsurerDictionaryDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(InsurerDictionaryPermissions.Edit)]
    public virtual Task<InsurerDictionaryDto> UpdateAsync(Guid id, UpdateInsurerDictionaryDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(InsurerDictionaryPermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }

    [HttpGet("export-template")]
    [Authorize(InsurerDictionaryPermissions.Create)]
    public virtual async Task<IActionResult> ExportTemplateAsync()
    {
        var fileBytes = await AppService.ExportTemplateAsync();
        var fileName = $"InsurerDictionary_Template_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    [HttpPost("import-excel")]
    [Authorize(InsurerDictionaryPermissions.Create)]
    public virtual async Task<ImportInsurerDictionaryResultDto> ImportExcelAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new UserFriendlyException("File là bắt buộc");
        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) &&
            !file.FileName.EndsWith(".xls", StringComparison.OrdinalIgnoreCase))
            throw new UserFriendlyException("Chỉ hỗ trợ định dạng (.xlsx, .xls)");

        using var memoryStream = new System.IO.MemoryStream();
        await file.CopyToAsync(memoryStream);
        var fileBytes = memoryStream.ToArray();
        return await AppService.ImportExcelAsync(fileBytes);
    }
}
