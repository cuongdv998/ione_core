using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Master;
using iOne.Master.Permissions;
using iOne.Master.ResTaskCategories;

namespace iOne.Master.Controllers;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Area(MasterRemoteServiceConsts.ModuleName)]
[Route("api/master/res-task-categories")]
[Authorize]
public class ResTaskCategoryController : AbpControllerBase
{
    protected IResTaskCategoryAppService AppService { get; }

    public ResTaskCategoryController(IResTaskCategoryAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(ResTaskCategoryPermissions.View)]
    public virtual Task<PagedResultDto<ResTaskCategoryDto>> GetListAsync(GetResTaskCategoriesInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(ResTaskCategoryPermissions.View)]
    public virtual Task<ResTaskCategoryDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(ResTaskCategoryPermissions.Create)]
    public virtual Task<ResTaskCategoryDto> CreateAsync(CreateResTaskCategoryDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(ResTaskCategoryPermissions.Edit)]
    public virtual Task<ResTaskCategoryDto> UpdateAsync(Guid id, UpdateResTaskCategoryDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(ResTaskCategoryPermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }
}
