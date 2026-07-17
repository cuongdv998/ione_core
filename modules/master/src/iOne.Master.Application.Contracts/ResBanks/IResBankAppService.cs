using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResBanks;

public interface IResBankAppService : ICrudAppService<
    ResBankDto,
    Guid,
    GetResBanksInput,
    CreateResBankDto,
    UpdateResBankDto>
{
}

