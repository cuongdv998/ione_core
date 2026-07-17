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
using iOne.Master.ResCarBrands;

namespace iOne.Master.Controllers;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Area(MasterRemoteServiceConsts.ModuleName)]
[Route("api/master/car-brands")]
//[Authorize]
public class ResCarBrandController : AbpControllerBase
{
    protected IResCarBrandAppService AppService { get; }

    public ResCarBrandController(IResCarBrandAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(ResCarBrandPermissions.View)]
    public virtual Task<PagedResultDto<ResCarBrandDto>> GetListAsync(GetResCarBrandsInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(ResCarBrandPermissions.View)]
    public virtual Task<ResCarBrandDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(ResCarBrandPermissions.Create)]
    public virtual Task<ResCarBrandDto> CreateAsync(CreateResCarBrandDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(ResCarBrandPermissions.Edit)]
    public virtual Task<ResCarBrandDto> UpdateAsync(Guid id, UpdateResCarBrandDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(ResCarBrandPermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }

    [HttpPost("import-excel")]
    [Authorize(ResCarBrandPermissions.Create)]
    public virtual async Task<ImportResCarBrandResultDto> ImportExcelAsync(IFormFile file)
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
    [Authorize(ResCarBrandPermissions.Create)]
    public virtual async Task<IActionResult> ExportTemplateAsync()
    {
        var fileBytes = await AppService.ExportTemplateAsync();
        var fileName = $"ResCarBrand_Template_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
        
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
}

