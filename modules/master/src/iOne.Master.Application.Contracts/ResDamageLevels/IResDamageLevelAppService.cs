using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResDamageLevels;

public interface IResDamageLevelAppService : ICrudAppService<
    ResDamageLevelDto,
    Guid,
    GetResDamageLevelsInput,
    CreateResDamageLevelDto,
    UpdateResDamageLevelDto>
{
}

