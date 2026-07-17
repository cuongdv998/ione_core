using iOne.ResAgreementTerms;
using Volo.Abp.Application.Dtos;

namespace iOne.Partner.ResAgreementTerms;

public class GetResAgreementTermsInput : PagedAndSortedResultRequestDto
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public ResAgreementTermStatus? Status { get; set; }
}

