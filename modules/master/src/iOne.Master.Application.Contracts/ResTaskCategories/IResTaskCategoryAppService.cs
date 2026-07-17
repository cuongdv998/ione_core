using System;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResTaskCategories;

public interface IResTaskCategoryAppService : ICrudAppService<
    ResTaskCategoryDto,
    Guid,
    GetResTaskCategoriesInput,
    CreateResTaskCategoryDto,
    UpdateResTaskCategoryDto>
{
}
