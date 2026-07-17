using System;
using Volo.Abp.Application.Services;

namespace iOne.Partner.ResAgreementTerms;

public interface IResAgreementTermAppService : ICrudAppService<
    ResAgreementTermDto,
    Guid,
    GetResAgreementTermsInput,
    CreateResAgreementTermDto,
    UpdateResAgreementTermDto>
{
}

