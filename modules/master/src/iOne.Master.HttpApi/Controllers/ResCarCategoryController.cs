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
using iOne.Master.ResCarCategories;

namespace iOne.Master.Controllers;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Area(MasterRemoteServiceConsts.ModuleName)]
[Route("api/master/car-categories")]
//[Authorize]
public class ResCarCategoryController : AbpControllerBase
{
    protected IResCarCategoryAppService AppService { get; }

    public ResCarCategoryController(IResCarCategoryAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(ResCarCategoryPermissions.View)]
    public virtual Task<PagedResultDto<ResCarCategoryDto>> GetListAsync(GetResCarCategoriesInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(ResCarCategoryPermissions.View)]
    public virtual Task<ResCarCategoryDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(ResCarCategoryPermissions.Create)]
    public virtual Task<ResCarCategoryDto> CreateAsync(CreateResCarCategoryDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(ResCarCategoryPermissions.Edit)]
    public virtual Task<ResCarCategoryDto> UpdateAsync(Guid id, UpdateResCarCategoryDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(ResCarCategoryPermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }

    [HttpPost("import-excel")]
    [Authorize(ResCarCategoryPermissions.Create)]
    public virtual async Task<ImportResCarCategoryResultDto> ImportExcelAsync(IFormFile file)
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

        return await AppService.ImportExcelAsync(fileBytes);
    }

    [HttpGet("export-template")]
    [Authorize(ResCarCategoryPermissions.Create)]
    public virtual async Task<IActionResult> ExportTemplateAsync()
    {
        var fileBytes = await AppService.ExportTemplateAsync();
        var fileName = $"ResCarCategory_Template_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
        
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
}


