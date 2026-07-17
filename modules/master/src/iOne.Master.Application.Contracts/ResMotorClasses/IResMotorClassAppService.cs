using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResMotorClasses;

public interface IResMotorClassAppService : ICrudAppService<
    ResMotorClassDto,
    Guid,
    GetResMotorClassesInput,
    CreateResMotorClassDto,
    UpdateResMotorClassDto>
{
}

