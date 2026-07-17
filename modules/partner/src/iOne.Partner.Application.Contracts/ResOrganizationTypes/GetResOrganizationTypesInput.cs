using iOne.ResOrganizationTypes;
using Volo.Abp.Application.Dtos;

namespace iOne.Partner.ResOrganizationTypes;

public class GetResOrganizationTypesInput : PagedAndSortedResultRequestDto
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public ResOrganizationTypeStatus? Status { get; set; }
}

