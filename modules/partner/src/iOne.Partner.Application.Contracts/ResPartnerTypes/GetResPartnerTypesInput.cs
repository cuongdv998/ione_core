using System;
using iOne.ResPartnerTypes;
using Volo.Abp.Application.Dtos;

namespace iOne.Partner.ResPartnerTypes;

public class GetResPartnerTypesInput : PagedAndSortedResultRequestDto
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public ResPartnerTypeStatus? Status { get; set; }
}

