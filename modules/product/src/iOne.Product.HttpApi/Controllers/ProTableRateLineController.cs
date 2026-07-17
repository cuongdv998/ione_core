using System;
using System.IO;
using System.Threading.Tasks;
using iOne.Product;
using iOne.Product.ProTableRateLines;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace iOne.Product.Controllers;

[RemoteService(Name = ProductRemoteServiceConsts.RemoteServiceName)]
[Area(ProductRemoteServiceConsts.ModuleName)]
[Route("api/product/table-rate-lines")]
[Authorize]
public class ProTableRateLineController : AbpControllerBase
{
    protected IProTableRateLineAppService AppService { get; }

    public ProTableRateLineController(IProTableRateLineAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    public virtual Task<PagedResultDto<ProTableRateLineDto>> GetListAsync(GetProTableRateLinesInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    public virtual Task<ProTableRateLineDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    public virtual Task<ProTableRateLineDto> CreateAsync(CreateProTableRateLineDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    public virtual Task<ProTableRateLineDto> UpdateAsync(Guid id, UpdateProTableRateLineDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }

    [HttpPost("import-excel")]
    public virtual async Task<ImportProTableRateLineResultDto> ImportExcelAsync(IFormFile file, [FromQuery] Guid tableRateId)
    {
        if (file == null || file.Length == 0)
        {
            throw new UserFriendlyException("File is required");
        }

        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) &&
            !file.FileName.EndsWith(".xls", StringComparison.OrdinalIgnoreCase))
        {
            throw new UserFriendlyException("Only Excel files (.xlsx, .xls) are allowed");
        }

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var fileBytes = memoryStream.ToArray();

        return await AppService.ImportExcelAsync(fileBytes, tableRateId);
    }

    [HttpGet("export-template")]
    public virtual async Task<IActionResult> ExportTemplateAsync([FromQuery] Guid tableRateId)
    {
        var fileBytes = await AppService.ExportTemplateAsync(tableRateId);
        var fileName = $"ProTableRateLine_Template_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
        
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
}
