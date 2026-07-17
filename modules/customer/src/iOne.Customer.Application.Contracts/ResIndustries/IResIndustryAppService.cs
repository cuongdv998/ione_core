using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace iOne.Customer.ResIndustries;

public interface IResIndustryAppService : ICrudAppService<
    ResIndustryDto,
    Guid,
    GetResIndustriesInput,
    CreateResIndustryDto,
    UpdateResIndustryDto>
{
}


