using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ResMotorClasses;

public interface IResMotorClassRepository : IRepository<ResMotorClass, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null);
}

