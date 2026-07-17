using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResSequences;

public interface IResSequenceAppService : ICrudAppService<
    ResSequenceDto,
    Guid,
    GetResSequencesInput,
    CreateResSequenceDto,
    UpdateResSequenceDto>
{
    /// <summary>
    /// Lấy sequence tiếp theo theo cấu hình (tự sinh mã)
    /// </summary>
    Task<GetNextSequenceOutput> GetNextSequenceAsync(GetNextSequenceInput input);
}

