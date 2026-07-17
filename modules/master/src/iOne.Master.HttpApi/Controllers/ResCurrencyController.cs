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
using iOne.Master.ResCurrencies;

namespace iOne.Master.Controllers;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Area(MasterRemoteServiceConsts.ModuleName)]
[Route("api/master/currencies")]
//[Authorize]
public class ResCurrencyController : AbpControllerBase
{
    protected IResCurrencyAppService AppService { get; }

    public ResCurrencyController(IResCurrencyAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(ResCurrencyPermissions.View)]
    public virtual Task<PagedResultDto<ResCurrencyDto>> GetListAsync(GetResCurrenciesInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(ResCurrencyPermissions.View)]
    public virtual Task<ResCurrencyDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(ResCurrencyPermissions.Create)]
    public virtual Task<ResCurrencyDto> CreateAsync(CreateResCurrencyDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(ResCurrencyPermissions.Edit)]
    public virtual Task<ResCurrencyDto> UpdateAsync(Guid id, UpdateResCurrencyDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(ResCurrencyPermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }

    [HttpPost("import-excel")]
    [Authorize(ResCurrencyPermissions.Create)]
    public virtual async Task<ImportResCurrencyResultDto> ImportExcelAsync(IFormFile file)
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
    [Authorize(ResCurrencyPermissions.Create)]
    public virtual async Task<IActionResult> ExportTemplateAsync()
    {
        var fileBytes = await AppService.ExportTemplateAsync();
        var fileName = $"ResCurrency_Template_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
        
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
}
