using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ResCarModels;

public interface IResCarModelRepository : IRepository<ResCarModel, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null);
    
    Task<bool> HasActiveModelsForBrandAsync(Guid carBrandId);
}



