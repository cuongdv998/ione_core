using iOne.ResBusinessAuthorities;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResBusinessAuthorities;

public class GetResBusinessAuthoritiesInput : PagedAndSortedResultRequestDto
{
    public string? Code { get; set; }

    public string? BusinessCode { get; set; }

    public string? Name { get; set; }

    public ResBusinessAuthorityStatus? Status { get; set; }
}
