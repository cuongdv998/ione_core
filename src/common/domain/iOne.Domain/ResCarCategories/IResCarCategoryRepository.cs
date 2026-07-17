using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ResCarCategories;

public interface IResCarCategoryRepository : IRepository<ResCarCategory, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null);
    
    /// <summary>
    /// Check if any CarCategory is using the specified CarBrand
    /// </summary>
    Task<bool> HasCategoriesForBrandAsync(Guid carBrandId);
    
    /// <summary>
    /// Check if any CarCategory is using the specified CarModel
    /// </summary>
    Task<bool> HasCategoriesForModelAsync(Guid carModelId);
    
    /// <summary>
    /// Check if any CarCategory is using the specified CarLine
    /// </summary>
    Task<bool> HasCategoriesForLineAsync(Guid carLineId);
}


